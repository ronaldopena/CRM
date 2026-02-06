using Poliview.crm.integracao;
using Poliview.crm.repositorios;
using Poliview.crm.services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<SqliteConnectionFactory>(_ => new SqliteConnectionFactory("Data Source=logintegracao.db;Cache=Shared;"));
        services.AddSingleton<LogRepository>();
        services.AddSingleton<LogService>();
        services.AddSingleton<IntegracaoService>();
        services.AddSingleton<ITelegramService, TelegramService>();
    })
    .Build();

await host.RunAsync();
