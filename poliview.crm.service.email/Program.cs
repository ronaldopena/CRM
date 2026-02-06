using poliview.crm.service.email;
using Poliview.crm.repositorios;
using Poliview.crm.services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Registrar SqliteConnectionFactory com string de conexão SQLite
        services.AddSingleton<SqliteConnectionFactory>(provider =>
        {
            var connectionString = context.Configuration["conexaosqlite"] ?? "Data Source=logapi.db;Cache=Shared";
            return new SqliteConnectionFactory(connectionString);
        });

        services.AddHostedService<Worker>();
        services.AddSingleton<LogRepository>();
        services.AddSingleton<LogService>();
        services.AddSingleton<IntegracaoService>();
        services.AddSingleton<ITelegramService, TelegramService>();
    })
    .Build();

await host.RunAsync();
