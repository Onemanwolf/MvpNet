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
<<<<<<< HEAD
            c.BaseAddress = new Uri("https://mvpdatasyncapi.azurewebsites.net/");
=======
            c.BaseAddress = new Uri("https://mvp-api.bluebeach-987e6b47.eastus.azurecontainerapps.io/");
>>>>>>> d870517d6e7763cefef32d809ebc085fe02ef033
        });
    }).ConfigureAppConfiguration(_config =>
    {
        _config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddUserSecrets(Assembly.GetExecutingAssembly(), true);
    })
    .Build();

await host.RunAsync();
