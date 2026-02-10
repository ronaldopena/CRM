using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface IContaEmailService
    {
        public ContaEmailResposta Listar();
        public Task<ContaEmail> ListarPorId(int idconta);
        public Task<IEnumerable<EmailQuarentena>> ListarQuarentenaPorContaEmail(int idContaEmail);
        public Task<Retorno> Update(ContaEmail obj);
        public Task<Retorno> Create(ContaEmail obj);
        public Task<Retorno> Delete(int idconta);
    }

    public class ContaEmailService : IContaEmailService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public ContaEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ContaEmailResposta Listar()
        {
            var retorno = new ContaEmailResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "select * from CAD_CONTA_EMAIL ";
                var registros = connection.Query<ContaEmail>(query);
                retorno.objeto = registros;
                retorno.status = 200;
                retorno.sucesso = true;
                retorno.mensagem = "OK";
            }
            catch (Exception ex)
            {
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                retorno.mensagem = ex.Message;
            }
            return retorno;
        }

        public async Task<ContaEmail> ListarPorId(int idconta)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"select * from CAD_CONTA_EMAIL where id={idconta} ";
            return await connection.QueryFirstAsync<ContaEmail>(query);
        }

        public async Task<IEnumerable<EmailQuarentena>> ListarQuarentenaPorContaEmail(int idContaEmail)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
SELECT
    id        AS Id,
    data      AS Data,
    remetente AS Remetente,
    destinatario AS Destinatario,
    assunto   AS Assunto,
    corpo     AS Corpo,
    nome      AS Nome,
    idcontaemail AS IdContaEmail
FROM OPE_EMAIL_QUARENTENA
WHERE idcontaemail = @idContaEmail
ORDER BY data DESC;";

            return await connection.QueryAsync<EmailQuarentena>(query, new { idContaEmail });
        }

        public async Task<Retorno> Update(ContaEmail obj)
        {
            var ret = new Retorno();

            using var connection = new SqlConnection(_connectionString);
            var query = $"UPDATE [dbo].[CAD_CONTA_EMAIL] " +
                        "SET [descricaoConta] = @descricaoConta " +
                        ",[tipoConta] = @tipoConta " +
                        ",[nomeRemetente] = @nomeRemetente" +
                        ",[emailRemetente] = @emailRemetente" +
                        ",[usuario] = @usuario" +
                        ",[senha] = @senha" +
                        ",[hostpop] = @hostpop" +
                        ",[portapop] = @portapop" +
                        ",[sslpop] = @sslpop" +
                        ",[hostsmtp] = @hostsmtp" +
                        ",[portasmtp] = @portasmtp" +
                        ",[sslsmtp] = @sslsmtp" +
                        ",[tenant_id] = @tenant_id" +
                        ",[client_id] = @client_id" +
                        ",[clientSecret] = @clientSecret" +
                        ",[userId] = @userId" +
                        ",[enviaremail] = @enviaremail" +
                        ",[receberemail] = @receberemail" +
                        ",[intervalorecebimento] = @intervalorecebimento" +
                        ",[intervaloenvio] = @intervaloenvio" +
                        ",[qtdeemailsrecebimento] = @qtdeemailsrecebimento" +
                        ",[qtdeemailsenvio] = @qtdeemailsenvio" +
                        ",[qtdetentativasenvio] = @qtdetentativasenvio" +
                        ",[emaildestinatariosuporte] = @emaildestinatariosuporte" +
                        ",[tamanhomaximoanexos] = @tamanhomaximoanexos" +
                        ",[ativo] = @ativo" +
                        " WHERE id=@id";

            var parameters = new
            {
                tipoConta = obj.tipoconta,
                descricaoConta = obj.descricaoConta,
                nomeRemetente = obj.nomeRemetente,
                emailRemetente = obj.emailRemetente,
                usuario = obj.usuario,
                senha = obj.senha,
                hostpop = obj.hostpop,
                portapop = obj.portapop,
                sslpop = obj.sslpop,
                hostsmtp = obj.hostsmtp,
                portasmtp = obj.portasmtp,
                sslsmtp = obj.sslsmtp,
                tenant_id = obj.tenant_id,
                client_id = obj.client_id,
                clientSecret = obj.clientSecret,
                userId = obj.userId,
                enviaremail = obj.enviaremail,
                receberemail = obj.receberemail,
                intervalorecebimento = obj.intervalorecebimento,
                intervaloenvio = obj.intervaloenvio,
                qtdeemailsrecebimento = obj.qtdeemailsrecebimento,
                qtdeemailsenvio = obj.qtdeemailsenvio,
                qtdetentativasenvio = obj.qtdetentativasenvio,
                emaildestinatariosuporte = obj.emaildestinatariosuporte,
                tamanhomaximoanexos = obj.tamanhomaximoanexos,
                ativo = obj.ativo,
                id = obj.id
            };

            var erro = "ok";
            try
            {
                var registros = await connection.ExecuteAsync(query, parameters);
            }
            catch (Exception ex)
            {
                erro = ex.Message;
            }
            ret.mensagem = erro;
            ret.sucesso = erro == "ok";
            return ret;

        }

        public void Reset()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "update OPE_EMAIL_PROCESSO set enviandoEmails=0, recebendoEmails=0 ";
                connection.Execute(query);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Retorno> Create(ContaEmail obj)
        {
            var ret = new Retorno();

            using var connection = new SqlConnection(_connectionString);
            var query = "INSERT INTO [dbo].[CAD_CONTA_EMAIL]" +
                           "([descricaoConta]" +
                           ",[tipoConta]" +
                           ",[nomeRemetente]" +
                           ",[emailRemetente]" +
                           ",[usuario]" +
                           ",[senha]" +
                           ",[hostpop]" +
                           ",[portapop]" +
                           ",[sslpop]" +
                           ",[hostsmtp]" +
                           ",[portasmtp]" +
                           ",[sslsmtp]" +
                           ",[tenant_id]" +
                           ",[client_id]" +
                           ",[clientSecret]" +
                           ",[userId]" +
                           ",[enviaremail]" +
                           ",[receberemail]" +
                           ",[intervalorecebimento]" +
                           ",[intervaloenvio]" +
                           ",[qtdeemailsrecebimento]" +
                           ",[qtdeemailsenvio]" +
                           ",[qtdetentativasenvio]" +
                           ",[emaildestinatariosuporte]" +
                           ",[tamanhomaximoanexos]" +
                           ",[ativo])" +
                     "VALUES" +
                        "(@descricaoConta" +
                        ",@tipoConta" +
                        ",@nomeRemetente" +
                        ",@emailRemetente" +
                        ",@usuario" +
                        ",@senha" +
                        ",@hostpop" +
                        ",@portapop" +
                        ",@sslpop" +
                        ",@hostsmtp" +
                        ",@portasmtp" +
                        ",@sslsmtp" +
                        ",@tenant_id" +
                        ",@client_id" +
                        ",@clientSecret" +
                        ",@userId" +
                        ",@enviaremail" +
                        ",@receberemail" +
                        ",@intervalorecebimento" +
                        ",@intervaloenvio" +
                        ",@qtdeemailsrecebimento" +
                        ",@qtdeemailsenvio" +
                        ",@qtdetentativasenvio" +
                        ",@emaildestinatariosuporte" +
                        ",@tamanhomaximoanexos" +
                        ",@ativo); SELECT SCOPE_IDENTITY() AS novoId;";

            var parameters = new
            {
                tipoConta = obj.tipoconta,
                descricaoConta = obj.descricaoConta,
                nomeRemetente = obj.nomeRemetente,
                emailRemetente = obj.emailRemetente,
                usuario = obj.usuario,
                senha = obj.senha,
                hostpop = obj.hostpop,
                portapop = obj.portapop,
                sslpop = obj.sslpop,
                hostsmtp = obj.hostsmtp,
                portasmtp = obj.portasmtp,
                sslsmtp = obj.sslsmtp,
                tenant_id = obj.tenant_id,
                client_id = obj.client_id,
                clientSecret = obj.clientSecret,
                userId = obj.userId,
                enviaremail = obj.enviaremail,
                receberemail = obj.receberemail,
                intervalorecebimento = obj.intervalorecebimento,
                intervaloenvio = obj.intervaloenvio,
                qtdeemailsrecebimento = obj.qtdeemailsrecebimento,
                qtdeemailsenvio = obj.qtdeemailsenvio,
                qtdetentativasenvio = obj.qtdetentativasenvio,
                emaildestinatariosuporte = obj.emaildestinatariosuporte,
                tamanhomaximoanexos = obj.tamanhomaximoanexos,
                ativo = obj.ativo
            };
            var erro = "ok";            
            try
            {                
                var registros = await connection.ExecuteAsync(query, parameters);                
            }
            catch (Exception ex)
            {
                erro = ex.Message;
            }
            ret.mensagem = erro;
            ret.sucesso = erro=="ok";
            return ret;
        }

        public async Task<Retorno> Delete(int idconta)
        {
            var ret = new Retorno();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "DELETE FROM [dbo].[CAD_CONTA_EMAIL] WHERE id = @id";
                await connection.ExecuteAsync(query, new { id = idconta });
                ret.sucesso = true;
                ret.mensagem = "ok";
            }
            catch (Exception ex)
            {
                ret.sucesso = false;
                ret.mensagem = ex.Message;
            }
            return ret;
        }
    }
}
