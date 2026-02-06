using poliview.crm.cessadireito;
using poliview.crm.cessadireito.Repositorios;
using poliview.crm.cessadireito.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<SqliteConnectionFactory>(_ => new SqliteConnectionFactory("Data Source=log.db;Cache=Shared;"));
        services.AddSingleton<LogRepository>();        
        services.AddSingleton<LogService>();
    })
    .Build();

await host.RunAsync();
