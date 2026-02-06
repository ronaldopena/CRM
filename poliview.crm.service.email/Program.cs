using Microsoft.Extensions.Options;
using poliview.crm.service.email;
using poliview.crm.service.email.Options;
using Poliview.crm.service.email.Services;
using Poliview.crm.repositorios;
using Poliview.crm.services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<EmailWorkerOptions>(opts =>
        {
            opts.Cliente = context.Configuration["cliente"] ?? "não identificado";
            opts.VerQuery = Convert.ToBoolean(context.Configuration["verQuery"] ?? "true");
            opts.VerDebug = Convert.ToBoolean(context.Configuration["verDebug"] ?? "true");
            opts.VerErros = Convert.ToBoolean(context.Configuration["verErros"] ?? "true");
            opts.VerInfos = Convert.ToBoolean(context.Configuration["verInfos"] ?? "true");
        });

        services.AddSingleton<SqliteConnectionFactory>(provider =>
        {
            var connectionString = context.Configuration["conexaosqlite"] ?? "Data Source=logapi.db;Cache=Shared";
            return new SqliteConnectionFactory(connectionString);
        });

        services.AddHostedService<Worker>();
        services.AddSingleton<LogRepository>();
        services.AddSingleton<LogService>();
        services.AddSingleton<ILogService>(sp => sp.GetRequiredService<LogService>());
        services.AddSingleton<IntegracaoService>();
        services.AddSingleton<ITelegramService, TelegramService>();
        services.AddSingleton<INotificacaoErro, NotificacaoErroService>();
        services.AddSingleton<IEmailProviderFactory, EmailProviderFactory>();
    })
    .Build();

await host.RunAsync();
