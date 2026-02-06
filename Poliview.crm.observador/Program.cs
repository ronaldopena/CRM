using Poliview.crm.http.services;
using Poliview.crm.observador;
// using Poliview.crm.repositorios;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddScoped<IAutenticacaoHttpService, AutenticacaoHttpService>();
        // services.AddSingleton<SqliteConnectionFactory>(_ => new SqliteConnectionFactory("Data Source=logobservador.db;Cache=Shared;"));
        services.AddHostedService<Worker>();            
    })
    .Build();

await host.RunAsync();
