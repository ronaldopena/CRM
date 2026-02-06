using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface IArquivoDownloadService
    {
        public ListarArquivosResposta ListAll();
        public ArquivoResposta FindById(int idarquivo);
        public ArquivoEmpreendimentosResposta FindEmpreendimentosById(int idarquivo);
        public ArquivoResposta Create(ArquivoDownload Arquivo);
        public ArquivoResposta Update(ArquivoDownload Arquivo);
        public ArquivoResposta Delete(int idarquivo);
        public ArquivosUsuarioResposta ListarArquivosUsuario(string cpf, string idarquivo);
        public BaixaArquivoDownloadResposta BaixaDownloadArquivo(BaixaArquivoDownloadRequisicao obj);
        public RelatorioCienciaArquivoResposta RelatorioCienciaArquivo(RelatorioCienciaArquivoRequisicao obj);
        public ListarGrupoMidiaArquivosResposta ListarArquivosDownloadPorGrupo(string cpf);   
        public ListarGrupoMidiaArquivosDetalheResposta ListarArquivosDownloadPorGrupoDetalhe(string cpf, int idgrupomidia);
    }

    public class ArquivoDownloadService : IArquivoDownloadService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;
        public ArquivoDownloadService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }
        public ArquivoResposta FindById(int idarquivo)
        {
            var retorno = new ArquivoResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query1 = $"SELECT * FROM CAD_ARQUIVO_EMPREENDIMENTOS WHERE idarquivo={idarquivo}";
                var result1 = connection.Query<ArquivoEmpreendimentos>(query1);
                var empreendimentos = "";
                foreach (var item in result1)
                {
                    if (empreendimentos == "")
                        empreendimentos += item.idempreendimento.ToString();
                    else    
                        empreendimentos += "," + item.idempreendimento.ToString();
                }
                var query = $"SELECT * FROM CAD_ARQUIVO_DOWNLOAD WHERE id={idarquivo}";
                var result = connection.QueryFirstOrDefault<ArquivoDownload>(query);
                if (result != null)
                {
                    result.empreendimentos = empreendimentos;
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Arquivo não encontrado";                    
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public ListarArquivosResposta ListAll()
        {
            var retorno = new ListarArquivosResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                // var query = "SELECT * FROM CAD_Arquivo_download ";

                var query = "select " +
                "CASE ARQ.variosempreendimentos  " +
                "    WHEN 1 THEN '** VARIOS **' " +
                "    WHEN 0 THEN COALESCE(EMP.NM_Empree, '** Todos **') " +
                "END as empreendimento,  " +
                "CASE ARQ.variosempreendimentos  " +
                "    WHEN 1 THEN '' " +
                "    WHEN 0 THEN COALESCE(BLO.NM_Bloco,'** Todos **') " +
                "END as bloco,  " +
                "CASE ARQ.variosempreendimentos  " +
                "    WHEN 1 THEN '' " +
                "    WHEN 0 THEN COALESCE(uni.NR_UnidadeSP7,'** Todos **') " +
                "END as unidade, ARQ.* from CAD_Arquivo_download ARQ " +
                "LEFT JOIN CAD_EMPREENDIMENTO EMP ON EMP.CD_EMPREENDIMENTO=ARQ.idempreendimento " +
                "LEFT JOIN CAD_BLOCO BLO ON BLO.CD_BLOCO=ARQ.idbloco " +
                "LEFT JOIN CAD_UNIDADE UNI ON UNI.CD_Unidade=ARQ.idunidade ";

                Console.WriteLine(query);
                var result = connection.Query<ArquivoDownload>(query);
                Console.WriteLine(result.ToString());
                retorno.mensagem = "OK";
                retorno.objeto = result;
                retorno.status = 200;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }

        }
        public ArquivoEmpreendimentosResposta FindEmpreendimentosById(int idarquivo)
        {
            var retorno = new ArquivoEmpreendimentosResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"SELECT * FROM CAD_Arquivo_EMPREENDIMENTOS WHERE idarquivo={idarquivo}";
                var result = connection.Query<ArquivoEmpreendimentos>(query);

                Console.WriteLine(query);

                if (result != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Arquivo não encontrada";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public ArquivosUsuarioResposta ListarArquivosUsuario(string cpf, string idarquivo)
        {
            var retorno = new ArquivosUsuarioResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec dbo.CRM_Listar_Arquivos_Usuario @cpfcliente='{cpf}', @idarquivo={idarquivo}";
                var result = connection.Query<ArquivoUsuario>(query);

                Console.WriteLine(query);

                retorno.mensagem = "OK";
                retorno.objeto = result;
                retorno.status = 200;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public ArquivoResposta Create(ArquivoDownload arquivo)
        {
            var retorno = new ArquivoResposta();
            try
            {
                var datainicial = String.Format("{0:dd/MM/yyyy 00:00:00}", arquivo.datainicial);
                var datafinal = String.Format("{0:dd/MM/yyyy 23:59:59} ", arquivo.datafinal);

                using var connection = new SqlConnection(_connectionString);
                var query = $"SET DATEFORMAT DMY; insert into CAD_ARQUIVO_DOWNLOAD ([descricao],[link],[idgrupomidia],[variosempreendimentos],[idempreendimento],[idbloco],[idunidade],[datainicial],[datafinal]) ";
                query += " values ";
                query += $"('{arquivo.descricao}','{arquivo.link}',{arquivo.idgrupomidia},'{arquivo.variosempreendimentos}','{arquivo.idempreendimento}',{arquivo.idbloco},{arquivo.idunidade},'{datainicial}','{datafinal}'); SELECT SCOPE_IDENTITY(); ";

                Console.WriteLine(query);

                // var result = connection.Execute(query);
                var result = connection.QueryFirstOrDefault<int>(query);

                zerarEmpreendimentosArquivo(arquivo.id);
                var empreendimentos = arquivo.empreendimentos.Split(",");
                foreach (var item in empreendimentos)
                {
                    IncluirEmpreendimentosArquivo(arquivo.id, Convert.ToInt32(item));
                }

                Console.WriteLine(result);

                if (result>0)
                {
                    arquivo.id = result;
                    retorno.mensagem = "OK";
                    retorno.objeto = arquivo;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public ArquivoResposta Update(ArquivoDownload arquivo)
        {
            var retorno = new ArquivoResposta();
            try
            {
                var datainicial = String.Format("{0:dd/MM/yyyy 00:00:00}", arquivo.datainicial);
                var datafinal = String.Format("{0:dd/MM/yyyy 23:59:59} ", arquivo.datafinal);

                using var connection = new SqlConnection(_connectionString);
                var query = $"SET DATEFORMAT DMY; update CAD_ARQUIVO_DOWNLOAD set " +
                            $"[descricao]='{arquivo.descricao}',"+
                            $"[link]='{arquivo.link}',"+
                            $"[idgrupomidia]={arquivo.idgrupomidia},"+
                            $"[variosempreendimentos]={arquivo.variosempreendimentos},"+
                            $"[idempreendimento]={arquivo.idempreendimento},"+
                            $"[idbloco]={arquivo.idbloco},"+
                            $"[idunidade]={arquivo.idunidade},"+
                            $"[datainicial]='{datainicial}'," +
                            $"[datafinal]='{datafinal}' " +
                            $"where [id]={arquivo.id};";

                Console.WriteLine("ARQUIVO UPDATE");
                Console.WriteLine(query);

                var result = connection.Execute(query);

                Console.WriteLine(arquivo.id);
                Console.WriteLine(arquivo.empreendimentos);

                zerarEmpreendimentosArquivo(arquivo.id);
                var empreendimentos = arquivo.empreendimentos.Split(",");
                foreach (var item in empreendimentos)
                {
                    IncluirEmpreendimentosArquivo(arquivo.id, Convert.ToInt32(item));
                }
                
                if (result > 0)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = arquivo;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Arquivo não encontrada";
                    retorno.objeto = arquivo;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }

        }
        public ArquivoResposta Delete(int idarquivo)
        {
            var retorno = new ArquivoResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"delete from CAD_ARQUIVO_DOWNLOAD where id=${idarquivo} ";
                var result = connection.Execute(query);
                if (result > 0)
                {
                    retorno.mensagem = "Arquivo excluído com sucesso!";
                    retorno.objeto = null;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Arquivo não encontrado!";
                    retorno.objeto = null;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public BaixaArquivoDownloadResposta BaixaDownloadArquivo(BaixaArquivoDownloadRequisicao obj)
        {
            var retorno = new BaixaArquivoDownloadResposta();
            try
            {
                // @cpfcnpj varchar(18), @idarquivodownload int, @idunidade int, @idorigem int
                using var connection = new SqlConnection(_connectionString);
                Console.WriteLine("EXECUTANDO PROCEDURE PARA BAIXAR ARQUIVO");
                var query = $"exec dbo.CRM_Baixa_ArquivoDownload @cpfcnpj='{obj.cpfcnpj}', @idarquivodownload={obj.idarquivo}, @idunidade={obj.idunidade}, @idorigem={obj.idorigem}";
                var result = connection.Query(query);

                Console.WriteLine(query);

                retorno.mensagem = "OK";
                retorno.status = 200;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        public RelatorioCienciaArquivoResposta RelatorioCienciaArquivo(RelatorioCienciaArquivoRequisicao obj)
        {
            var retorno = new RelatorioCienciaArquivoResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var query = $"SET DATEFORMAT dmy; select * from vRelatorioArquivosDownload where 1=1 ";
                // datainicialfiltro between '{obj.datainicial.ToString("dd/MM/yyyy 00:00:00")}' and '{obj.datafinal.ToString("dd/MM/yyyy 23:59:59")}' ";

                if (!String.IsNullOrEmpty(obj.idarquivoList)) query += $"and idarquivodownload in ({obj.idarquivoList}) ";

                if (!String.IsNullOrEmpty(obj.idempreendimentoList)) query += $"and idempreendimento in ({obj.idempreendimentoList}) ";

                if (obj.idbloco > 0)
                    query += $"and idbloco={obj.idbloco} ";

                if (obj.idunidade > 0)
                    query += $"and idunidade={obj.idunidade} ";

                if (obj.idorigem > 0)
                    query += $"and idorigem={obj.idorigem} ";

                Console.WriteLine(query);

                // var query = $"exec dbo.CRM_REL_CIENCIA_Arquivo @idarquivo={obj.idarquivo}";
                var result = connection.Query<RelatorioCienciaArquivo>(query);
                                

                if (result != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Não há registros para apresentar";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
        protected void zerarEmpreendimentosArquivo(int idarquivo)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"delete from CAD_Arquivo_EMPREENDIMENTOS where idarquivo=${idarquivo} ";
            connection.Execute(query);
        }
        protected void IncluirEmpreendimentosArquivo(int idarquivo, int idempreendimento)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"insert into CAD_Arquivo_EMPREENDIMENTOS (idarquivo, idempreendimento) values (${idarquivo},${idempreendimento}) ";
            connection.Execute(query);
        }
        public ListarGrupoMidiaArquivosResposta ListarArquivosDownloadPorGrupo(string cpf)
        {
            var retorno = new ListarGrupoMidiaArquivosResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec [dbo].[CRM_Listar_Arquivos_Download_Usuario] @cpfcliente='{cpf}', @idgrupomidia=0, @agrupado=1 ";
                var result = connection.Query<GrupoMidiaArquivo>(query);

                Console.WriteLine(query);

                if (result != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Não há registros para apresentar";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }    
        public ListarGrupoMidiaArquivosDetalheResposta ListarArquivosDownloadPorGrupoDetalhe(string cpf, int idgrupomidia)
        {
            var retorno = new ListarGrupoMidiaArquivosDetalheResposta();
            try
            {
                Console.WriteLine("Resultado ListarArquivosDownloadPorGrupoDetalhe");
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec [dbo].[CRM_Listar_Arquivos_Download_Usuario] @cpfcliente='{cpf}',@idgrupomidia={idgrupomidia}, @agrupado=0";
                Console.WriteLine(query);

                var result = connection.Query<GrupoMidiaArquivoDetalhe>(query);            
                
                if (result != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Não há registros para apresentar";
                    retorno.objeto = null;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }

    }
}
