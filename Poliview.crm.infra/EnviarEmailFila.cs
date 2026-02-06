using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using Dapper;

namespace Poliview.crm.infra
{
    public  class EnviarEmailFila
    {
        public void IncluirNaFila(string emaildestinatario, 
                                   string urlanexo, 
                                   string assunto, 
                                   string corpo,
                                   string nomeremetente, 
                                   string emailremetente, 
                                   string _connectionString)
        {
            var email = new Email();
            email.emailremetente = emailremetente;
            email.nomeremetente = nomeremetente;
            email.emaildestinatario = emaildestinatario;
            email.assunto = assunto;
            email.corpo = email.corpo;
            email.urlanexo = urlanexo;
            email.tipoemail = "E";
            email.prioridade = 0;
            email.idstatusenvio = 0;

            using var connection = new SqlConnection(_connectionString);
            var query = "INSERT INTO [dbo].[OPE_EMAIL] " +
                        "           ([DT_EmailInclusao] " +
                        "           ,[DS_EmailNomeRemetente] " +
                        "           ,[DS_EmailRemetente] " +
                        "           ,[DS_EmailDestinatario] " +
                        "           ,[DS_EmailCopia] " +
                        "           ,[DS_EmailCopiaOculta] " +
                        "           ,[DS_EmailAssunto] " +
                        "           ,[DS_EmailCorpo] " +
                        "           ,[IN_EmailCorpoHTML] " +
                        "           ,[IN_EmailStatusEnvio] " +
                        "           ,[DT_EmailEnvio] " +
                        "           ,[IN_EmailPrioridade] " +
                        "           ,[CD_Documento] " +
                        "           ,[IN_Processado] " +
                        "           ,[IN_TipoEmail] " +
                        "           ,[QTD_TentativasEnvio] " +
                        "           ,[DS_ErroEnvio] " +
                        "           ,[ID_Chamado] " +
                        "           ,[Entregue] " +
                        "           ,[CD_Aviso] " +
                        "           ,[urlanexo]" +
                        "           ,[idEmailOrigem]) " +
                        "     VALUES " +
                        $"           (GetDate() " +
                        $"           ,@nomeremetente " +
                        $"           ,@emailremetente " +
                        $"           ,@emaildestinatario " +
                        "           ,'' " +
                        "           ,'' " +
                        $"           ,@assunto" +
                        $"           ,@corpo " +
                        $"           ,@corpohtml " +
                        $"           ,@idstatusenvio " +
                        $"           , null " +
                        $"           ,@prioridade " +
                        $"           ,@iddocumento " +
                        $"           ,@processado " +
                        $"           ,@tipoemail " +
                        $"           ,0" +
                        $"           ,@erroenvio " +
                        $"           ,@idchamado " +
                        $"           ,@entregue " +
                        $"           ,@idaviso " +
                        $"           ,@urlanexo" +
                        $"           ,@idEmailOrigem)";

            connection.Execute(query, email);

        }
    }
}
