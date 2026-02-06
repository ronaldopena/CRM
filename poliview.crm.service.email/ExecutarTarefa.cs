using Dapper;
using Poliview.crm.services;
using Microsoft.Data.SqlClient;

namespace poliview.crm.service.email
{
    public static class ExecutarTarefa
    {
        public static void execute(IConfiguration _configuration, Serilog.ILogger log, int count)
        {
            ExecutarProcedureTarefas(_configuration, log);

            if (count % 10 == 0 || count == 1)
            {
                ExecutarVerificacaoServicoEmail(_configuration, log);
            }
        }

        private static void ExecutarProcedureTarefas(IConfiguration _configuration, Serilog.ILogger log)
        {
            var connectionString = _configuration["conexao"].ToString();
            using var connection = new SqlConnection(connectionString);
            var query = "exec CRM_Executar_Tarefas";
            connection.Query(query);
        }

        private static void ExecutarVerificacaoServicoEmail(IConfiguration _configuration, Serilog.ILogger log)
        {
            /*
            var connectionString = _configuration["conexao"].ToString();
            using var connection = new SqlConnection(connectionString);
            var query = "EXEC DBO.CRM_Verificar_Servico_Email";
            var registros = connection.Query(query);

            var contasemailservice = new ContaEmailService(_configuration);
            var retorno = contasemailservice.Listar();
            var conta = retorno.objeto.First();

            foreach (var registro in registros)
            {
                if (registro.sucesso == 0) 
                {
                    var parametros = ParametrosService.consultar(connectionString);
                    var tipoAutenticacao = conta.tipoconta;

                    poliview.crm.service.email.Services.ISendEmailService service = (tipoAutenticacao == 0 ? new poliview.crm.service.email.Services.SendEmailPadraoService(_configuration) :
                                   new poliview.crm.service.email.Services.SendEmailOffice365Service(_configuration));
                    service.EnviarEmailAvulso(parametros.emailErrosAdmin, "SERVIÇO DE EMAIL INOPERANTE", registro.retorno, conta, log);
                }
            }            
            */
        }
    }

}
