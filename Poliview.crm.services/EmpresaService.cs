using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface IEmpresaService
    {
        public ListarEmpresasResposta Listar();
        public ListarEmpresaResposta ListarPorDominio(string dominio);
        public Task<Retorno> Create(Empresa obj);
        public Task<Retorno> Update(Empresa obj);
        public Task<Retorno> Delete(int id);
    }

    public class EmpresaService : IEmpresaService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;
        
        public EmpresaService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListarEmpresasResposta Listar()
        {
            var retorno = new ListarEmpresasResposta();

            try
            {                
                using var connection = new SqlConnection(_connectionString);
                var query = $"select * from cad_empresa ";

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.Query<Empresa>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }

        public ListarEmpresaResposta ListarPorDominio(string dominio)
        {
            var retorno = new ListarEmpresaResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var querycount = $"select count(*) as qtde from cad_empresa";
                var registros = connection.QueryFirst<int>(querycount);                
                Console.WriteLine($"registro de dominio = {registros.ToString()}");
                var query = $"select * from cad_empresa where dominioempresa=@dominio";

                if (registros==1) query = $"select top 1 * from cad_empresa";
                Console.WriteLine(query);
                
                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.QueryFirstOrDefault<Empresa>(query, new { dominio=dominio });

                if (retorno.objeto == null)
                {
                    retorno.sucesso = false;
                    retorno.mensagem = $"Empresa não encontrada para o dominio {dominio}";
                }
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;
        }

        public async Task<Retorno> Create(Empresa obj)
        {
            var ret = new Retorno();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = @"INSERT INTO [dbo].[CAD_EMPRESA]
                           ([nomeEmpresa]
                           ,[dominioempresa]
                           ,[idcontaemail]
                           ,[ativo]
                           ,[principal]
                           ,[principaldark]
                           ,[principallight]
                           ,[fundo]
                           ,[texto]
                           ,[urllogo]
                           ,[urlimgprincipal])
                     VALUES
                           (@nomeEmpresa
                           ,@dominioempresa
                           ,@idcontaemail
                           ,@ativo
                           ,@principal
                           ,@principaldark
                           ,@principallight
                           ,@fundo
                           ,@texto
                           ,@urllogo
                           ,@urlimgprincipal)";
                await connection.ExecuteAsync(query, new
                {
                    nomeEmpresa = obj.nomeempresa,
                    dominioempresa = obj.dominioempresa,
                    idcontaemail = obj.idcontaemail,
                    ativo = obj.ativo,
                    principal = obj.principal ?? "",
                    principaldark = obj.principaldark ?? "",
                    principallight = obj.principallight ?? "",
                    fundo = obj.fundo ?? "",
                    texto = obj.texto ?? "",
                    urllogo = obj.urllogo ?? "",
                    urlimgprincipal = obj.urlimgprincipal ?? ""
                });
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

        public async Task<Retorno> Update(Empresa obj)
        {
            var ret = new Retorno();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = @"UPDATE [dbo].[CAD_EMPRESA]
                           SET [nomeEmpresa] = @nomeEmpresa
                              ,[dominioempresa] = @dominioempresa
                              ,[idcontaemail] = @idcontaemail
                              ,[ativo] = @ativo
                              ,[principal] = @principal
                              ,[principaldark] = @principaldark
                              ,[principallight] = @principallight
                              ,[fundo] = @fundo
                              ,[texto] = @texto
                              ,[urllogo] = @urllogo
                              ,[urlimgprincipal] = @urlimgprincipal
                         WHERE id = @id";
                await connection.ExecuteAsync(query, new
                {
                    id = obj.id,
                    nomeEmpresa = obj.nomeempresa,
                    dominioempresa = obj.dominioempresa,
                    idcontaemail = obj.idcontaemail,
                    ativo = obj.ativo,
                    principal = obj.principal ?? "",
                    principaldark = obj.principaldark ?? "",
                    principallight = obj.principallight ?? "",
                    fundo = obj.fundo ?? "",
                    texto = obj.texto ?? "",
                    urllogo = obj.urllogo ?? "",
                    urlimgprincipal = obj.urlimgprincipal ?? ""
                });
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

        public async Task<Retorno> Delete(int id)
        {
            var ret = new Retorno();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "DELETE FROM [dbo].[CAD_EMPRESA] WHERE id = @id";
                await connection.ExecuteAsync(query, new { id });
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
