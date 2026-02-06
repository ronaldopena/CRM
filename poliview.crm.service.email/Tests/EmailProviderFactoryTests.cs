using Microsoft.Extensions.Configuration;
using Moq;
using Poliview.crm.service.email.Services;
using poliview.crm.service.email;
using Xunit;

namespace Poliview.crm.service.email.Tests;

public class EmailProviderFactoryTests
{
    [Fact]
    public void TipoContaEmail_Valores_Conforme_Esperado()
    {
        Assert.Equal(0, (int)TipoContaEmail.Padrao);
        Assert.Equal(1, (int)TipoContaEmail.Office365);
        Assert.Equal(2, (int)TipoContaEmail.Gmail);
    }

    [Fact]
    public void GetSendService_Lanca_ArgumentException_Para_TipoInvalido()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["conexao"]).Returns("Server=.;Database=test;");
        config.Setup(c => c["cliente"]).Returns("Teste");
        config.Setup(c => c["verQuery"]).Returns("false");
        config.Setup(c => c["verDebug"]).Returns("false");
        config.Setup(c => c["verErros"]).Returns("true");
        config.Setup(c => c["verInfos"]).Returns("false");

        var sqliteFactory = new Poliview.crm.repositorios.SqliteConnectionFactory("Data Source=:memory:");
        var logRepo = new Poliview.crm.repositorios.LogRepository(sqliteFactory);
        var logService = new Poliview.crm.services.LogService(logRepo, config.Object);
        var notificacao = new Mock<INotificacaoErro>().Object;
        var factory = new EmailProviderFactory(config.Object, logService, notificacao);

        var ex = Assert.Throws<ArgumentException>(() => factory.GetSendService(99));
        Assert.Contains("99", ex.Message);
    }
}
