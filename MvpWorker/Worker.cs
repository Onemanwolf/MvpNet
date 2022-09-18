using System.Text;
using Azure.Messaging.ServiceBus;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage;
using System.IO;
using Newtonsoft.Json;


namespace MvpWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IConfiguration _configuration;

    string? _connectionString;
    private DataLakeServiceClient _dataLakeServiceClient;
    string? _dataLakeConnectionString;
    private string _queueName = "myqueue";

    private readonly IHttpClientFactory _clientFactory;
    private HttpClient _client;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _configuration = configuration;
        _connectionString = _configuration["servicebus-connection-string"];
        _client = _clientFactory.CreateClient("MvpClient");

    }




    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _dataLakeConnectionString = _configuration["datalake-connection-string"];
        _connectionString = _configuration["servicebus-connection-string"];

        _dataLakeServiceClient = new DataLakeServiceClient(_dataLakeConnectionString);
        //az containerapp update -n mvp-app-worker -g my-container-apps --set-env-vars container=my-file-system directory=my-directory subdirectory=my-subdirectory \
        // --image myregistry.azurecr.io/my-app:v2.0
        var container = _configuration["container"]; //"my-file-system";//await CreateFileSystemAsync(_dataLakeServiceClient);
        var directory = _configuration["Directory"]; //"my-directory"; //await CreateDirectoryAsync(_dataLakeServiceClient, container.Name);
        ServiceBusClient client = new ServiceBusClient(_connectionString);

        while (!stoppingToken.IsCancellationRequested)
        {
            ServiceBusReceiver receiver = client.CreateReceiver(_queueName);

            // the received message is a different type as it contains some service set properties
            // a batch of messages (maximum of 2 in this case) are received
            IReadOnlyList<ServiceBusReceivedMessage> receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages: 10);

            // go through each of the messages received
            foreach (ServiceBusReceivedMessage receivedMessage in receivedMessages)
            {
                // get the message body as a string
                string body = receivedMessage.Body.ToString();
                _logger.LogInformation($"Message received: {body} time: {DateTimeOffset.Now}");

                var message = JsonConvert.DeserializeObject<Message>(body);
                var encounter = await GetEncounter(message.MessageId);
                // save encouter to datalake
                await SaveEncounter(_dataLakeServiceClient, container, encounter);
                await receiver.CompleteMessageAsync(receivedMessage);

            }
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        }
    }

    private async Task SaveEncounter(DataLakeServiceClient fileSystemClient, string fileSystemName, string encounter)
    {
        DataLakeDirectoryClient directoryClient =
        fileSystemClient.GetFileSystemClient(fileSystemName).GetDirectoryClient(_configuration["directory"]).GetSubDirectoryClient(_configuration["subdirectory"]);

        var filename = Guid.NewGuid().ToString() + ".json";
        DataLakeFileClient fileClient = directoryClient.GetFileClient(filename);



        await using var ms = new MemoryStream();
        var json = JsonConvert.SerializeObject(encounter);
        var writer = new StreamWriter(ms);
        await writer.WriteAsync(json);
        await writer.FlushAsync();
        ms.Position = 0;
        try
        {
            long fileSize = ms.Length;

            await fileClient.UploadAsync(ms, false);

            await fileClient.FlushAsync(position: fileSize);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex.Message);
        }




    }

    public async Task<DataLakeDirectoryClient> CreateDirectoryAsync
        (DataLakeServiceClient serviceClient, string fileSystemName)
    {
        DataLakeFileSystemClient fileSystemClient =
            serviceClient.GetFileSystemClient(fileSystemName);

        DataLakeDirectoryClient directoryClient =
            await fileSystemClient.CreateDirectoryAsync(_configuration["directory"]);

        return await directoryClient.CreateSubDirectoryAsync(_configuration["subdirectory"]);
    }


    public async Task<DataLakeFileSystemClient> CreateFileSystemAsync
        (DataLakeServiceClient serviceClient)
    {
        return await serviceClient.CreateFileSystemAsync(_configuration["container"]);
    }
    private async Task<string> GetEncounter(string messeageId)
    {

        var content = " ";
        try
        {

            var response = await _client.GetAsync($"Encounter/?id={messeageId}");
            _logger.LogInformation($"Response: {response.StatusCode} time: {DateTimeOffset.Now}");
            content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Response: {content} time: {DateTimeOffset.Now}");

        }
        catch (Exception ex) { _logger.LogError(ex.Message); }
        return content;

    }
}

public class Message
{

    [JsonProperty("Messageid")]
    public string? MessageId { get; set; }

}


