using MvpWorker;
using System.Net.Http;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddHttpClient<Worker>("MvpClient", c =>
        {
            c.BaseAddress = new Uri("https://mvpdatasyncapi.azurewebsites.net/");
        });
    })
    .Build();

await host.RunAsync();
