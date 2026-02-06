using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface INotificacaoService
    {
        public NotificacaoResposta Listar(string id);
        public NotificacaoResposta Salvar(Notificacao notificacao);
        public NotificacaoClienteResposta ListarNotificacoesCliente(string cpf);
        public LeituraNotificacaoResposta LeituraNotificacao(LeituraNotificacaoRequisicao obj);
        public RelatorioCienciaNotificacaoResposta RelatorioCienciaNotificacao(RelatorioCienciaNotificacaoRequisicao obj);
        public JaLeuNotificacaoResposta LeuNotificacao(JaLeuNotificacaoRequisicao notificacao);
        
    }



    public class NotificacaoService : INotificacaoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;
        private string URL_API_SIECON;

        public NotificacaoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
            URL_API_SIECON = configuration["urlApiSiecon"];
        }

        public NotificacaoResposta Listar(string id)
        {
            var ret = new NotificacaoResposta();
            ret.status = 200;

            try
            {                
                if (this.JaCadastrada(id))
                {
                    using var connection = new SqlConnection(_connectionString);
                    var query = $"select * from CAD_NOTIFICACAO where id='{id}'";
                    Console.WriteLine(query);
                    ret.objeto = connection.QueryFirst<Notificacao>(query);
                    ret.mensagem = "Notificação selecionada com sucesso";
                    ret.sucesso = true;
                    return ret;
                }
                else
                {
                    var obj = new Notificacao() { id = id, mensagem = "" };
                    ret.objeto = obj;
                    ret.mensagem = "Notificação selecionada com sucesso";
                    ret.sucesso = true;
                    return ret;
                }
            }
            catch (Exception ex)
            {
                ret.status = 500;
                ret.mensagem = ex.Message;
                ret.sucesso = false;
                ret.objeto = null;
                return ret;
            }

            
        }

        public NotificacaoResposta Salvar(Notificacao notificacao)
        {
            var resposta = new NotificacaoResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                if (this.JaCadastrada(notificacao.id))
                {
                    var query = $"UPDATE CAD_NOTIFICACAO SET mensagem=@mensagem where id=@id";
                    Console.WriteLine(query);
                    connection.Execute(query, new { notificacao.mensagem, notificacao.id });
                    resposta.sucesso = true;
                    resposta.status = 200;
                    resposta.objeto = null;
                    resposta.mensagem = "Notificação alterada com sucesso!";
                    return resposta;
                }
                else
                {
                    var query = $"INSERT INTO CAD_NOTIFICACAO (id, mensagem) VALUES (@id,@mensagem)";
                    Console.WriteLine(query);
                    connection.Execute(query, new { notificacao.id, notificacao.mensagem });
                    resposta.sucesso = true;
                    resposta.status = 200;
                    resposta.objeto = null;
                    resposta.mensagem = "Notificação incluída com sucesso!";
                    return resposta;
                }
            }
            catch (Exception ex)
            {
                resposta.sucesso = false;
                resposta.status = 500;
                resposta.objeto = null;
                resposta.mensagem = ex.Message;
                return resposta;
            }
        }

        private bool JaCadastrada(string id)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"SELECT 1 FROM CAD_NOTIFICACAO where id='{id}'";
            Console.WriteLine(query);
            var result = connection.Query(query);
            return (result.Count() > 0);
        }

        public NotificacaoClienteResposta ListarNotificacoesCliente(string cpf)
        {
            // {{URL_API_SIECON}}/api/DadosCliente?cnpjCpf=37251306638&dataAssCtrDe=2000-01-01&dataAssCtrAte=2023-12-15
            var ret = new NotificacaoClienteResposta();
            var res = new NotificacaoCliente();
            ret.status = 200;
            
            try
            {
                var retdados = DadosCliente(cpf);
                ret.objeto = retdados;
                ret.sucesso = true;
                return ret;
            }
            catch (Exception ex)
            {
                ret.status = 500;
                ret.mensagem = ex.Message;
                ret.sucesso = false;
                ret.objeto = null;
                return ret;
            }
        }

        private async Task<Object> DadosCliente(string cpf)
        {
            var _httpClient = new HttpClient();
            var urlApiSiecon = $"{URL_API_SIECON}/api/DadosCliente?cnpjCpf={cpf}&dataAssCtrDe=2000-01-01&dataAssCtrAte=2100-01-01";
            var response = await _httpClient.GetAsync(urlApiSiecon);
            var resposta = response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto").ToString();

            var data = new Object();

            if (!string.IsNullOrWhiteSpace(values))
            {
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Object>(values);
            }

            return data;

        }

        private async Task boletoVencido(string cpf)
        {
            var _httpClient = new HttpClient();
            var urlApiSiecon = $"{URL_API_SIECON}/api/DadosCliente?cnpjCpf={cpf}&dataAssCtrDe=2000-01-01&dataAssCtrAte=2100-01-01";
            var response = await _httpClient.GetAsync(urlApiSiecon);
            response.EnsureSuccessStatusCode();
            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("objeto").ToString();

            var data = new Object();

            if (! string.IsNullOrWhiteSpace(values))
            {
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<Notificacao>(values);
            }
            
        }

        public LeituraNotificacaoResposta LeituraNotificacao(LeituraNotificacaoRequisicao obj)
        {
            var retorno = new LeituraNotificacaoResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec dbo.CRM_Leitura_Notificacao @idnotificacao='{obj.idnotificacao}', @idunidade={obj.idunidade}, @idorigem={obj.idorigem}, @cpfcnpj='{obj.cpfcnpj}'";
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

        public RelatorioCienciaNotificacaoResposta RelatorioCienciaNotificacao(RelatorioCienciaNotificacaoRequisicao obj)
        {
            var retorno = new RelatorioCienciaNotificacaoResposta();
            try
            {
                var registros = RetornaRegistrosRelCienciaNotificacao(obj);

                if (registros != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = registros;
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

        public List<RelatorioCienciaNotificacao> RetornaRegistrosRelCienciaNotificacao(RelatorioCienciaNotificacaoRequisicao obj)
        {
            // @datainicial datetime, @datafinal datetime, @idmensagem int, @idempreendimento int, @idbloco int, @idunidade int, @idorigem int

            Console.WriteLine("ID DA NOTIFICAÇÃO");
            Console.WriteLine(obj.idnotificacao);            

            using var connection = new SqlConnection(_connectionString);

            var query = $"SET DATEFORMAT dmy; select * from vRelatorioNotificacoes where 1=1 ";
            // where dataleiturafiltro between '{obj.datainicial.ToString("dd/MM/yyyy 00:00:00")}' and '{obj.datafinal.ToString("dd/MM/yyyy 23:59:59")}' ";

            if (obj.idnotificacao != "0")
                query += $"and notificacao='{obj.idnotificacao}'";
            
            if (!String.IsNullOrEmpty(obj.idempreendimentoList)) query += $"and idempreendimento in ({obj.idempreendimentoList}) ";

            if (obj.idbloco > 0)
                query += $"and idbloco={obj.idbloco} ";

            if (obj.idunidade > 0)
                query += $"and idunidade={obj.idunidade} ";

            if (obj.idorigem > 0)
                query += $"and idorigem={obj.idorigem} ";

            Console.WriteLine(query);

            var result = connection.Query<RelatorioCienciaNotificacao>(query);

            return result.ToList();
        }

        public JaLeuNotificacaoResposta LeuNotificacao(JaLeuNotificacaoRequisicao obj)
        {
            var retorno = new JaLeuNotificacaoResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"exec dbo.CRM_Ja_Leu_Notificacao @cpf='{obj.cpf}', @mes='{obj.mes}', @ano='{obj.ano}'";
                var result = connection.QueryFirstOrDefault<JaLeuNotificacao>(query);

                Console.WriteLine(query);

                retorno.mensagem = "OK";
                retorno.status = 200;
                retorno.sucesso = true;
                retorno.objeto = result;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.status = 500;
                retorno.sucesso = false;
                retorno.objeto = null;
                return retorno;
            }
        }
    }
}
