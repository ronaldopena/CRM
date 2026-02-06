using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Poliview.crm.api.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.Tests.Controllers
{
    public class InfoControllerTests
    {
        [Fact]
        public void TestandoSeRetornoInformaçõesServidorCrmÉhttp200()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();   
            // Arrange
            var controller = new InfoController(configuration);

            // Act
            var result = controller.infoCrm();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)okResult.StatusCode, 200);
            // Assert.Equal("Informação esperada", okResult.Value);
        }
        [Fact]
        public void TestandoSeExecuçãoDeSqlÉhttp200()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();   
            // Arrange
            var controller = new InfoController(configuration);
            var requisicao = new requisicaoExecSQL();
            requisicao.sql = "select * from OPE_CONFIG where chaveacesso='123456'";

            // Act
            var result = controller.ExecSQL(requisicao);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)okResult.StatusCode, 200);
            // Assert.Equal("Informação esperada", okResult.Value);
        }
    }
}
