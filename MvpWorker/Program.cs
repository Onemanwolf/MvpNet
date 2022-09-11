using MvpWorker;
using System.Net.Http;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddHttpClient<Worker>("MvpClient", c =>
        {
            c.BaseAddress = new Uri("https://mvp-api.bluebeach-987e6b47.eastus.azurecontainerapps.io/");
        });
    })
    .Build();

await host.RunAsync();
