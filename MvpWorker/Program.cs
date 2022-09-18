using MvpWorker;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {

        services.AddHostedService<Worker>();
        services.AddHttpClient<Worker>("MvpClient", c =>
        {
            c.BaseAddress = new Uri("https://mvpdatasyncapi.azurewebsites.net/");
        });
    }).ConfigureAppConfiguration(_config =>
    {
        _config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddUserSecrets(Assembly.GetExecutingAssembly(), true);
    })
    .Build();

await host.RunAsync();
