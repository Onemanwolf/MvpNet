using MvpWorker;
using System.Net.Http;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddHttpClient<Worker>("MvpClient", c =>
        {
            c.BaseAddress = new Uri("http://localhost:5175/");
        });
    })
    .Build();

await host.RunAsync();
