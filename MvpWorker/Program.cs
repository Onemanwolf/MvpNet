using MvpWorker;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

internal class Program
{

    private static ServiceConfiguration _serviceConfiguration;
    private static async Task Main(string[] args)
    {


        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(_config =>
            {
                _config.AddEnvironmentVariables()
                       .AddUserSecrets(Assembly.GetExecutingAssembly(), true);
                IConfigurationRoot root = _config.Build();
                _serviceConfiguration = new();
                root.Bind(_serviceConfiguration);
            })

            .ConfigureServices(services =>
            {
                services.AddSingleton(typeof(ServiceConfiguration), _serviceConfiguration);
                services.AddHostedService<Worker>();
                services.AddHttpClient<Worker>("MvpClient", c =>
                {
                    c.BaseAddress = new Uri("https://mvpdatasyncapi.azurewebsites.net/");
                });
            })
            .Build();

        await host.RunAsync();
    }
}