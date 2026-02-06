using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;
using System.Net.Http.Headers;

namespace Poliview.crm.services
{
    public class DadosUsuario
    {
        public string? codigoClienteSP7 { get; init; }
        public string? codigoContratoSP7 { get; init; }
    }

    public class Token
    {
        public string? access_token { get; set; }
    }

    public interface IBoletoService
    {
        public Task<Retorno> ListarNovo(ListarBoletosRequisicao obj);
        public Task<List<Boleto>> Listar(ListarBoletosRequisicao obj);
    }


    public class BoletoService : IBoletoService
    {
        private readonly string _connectionStringFB;
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public BoletoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
            _connectionStringFB = configuration["conexaoFirebird"];
        }

        public async Task<Retorno> ListarNovo(ListarBoletosRequisicao obj)
        {
            var ret = new Retorno();

            try
            {
                var dadosusuario = GetDadosUsuario(obj);
                //obj.codigoContratoSP7 = dadosusuario.codigoContratoSP7;
                //obj.codigoclientesp7 = dadosusuario.codigoClienteSP7;

                Console.WriteLine("Contrato: " + obj.codigocontratosp7 + " Cliente: " + obj.codigoclientesp7);

                var lista = new List<Boleto>();
                var parametros = ParametrosService.consultar(_connectionString);

                Console.WriteLine($"Acesso Siecon: {parametros.tipoAcessoSiecon} ");

                if (parametros.tipoAcessoSiecon == 0) lista = await BoletosAcessoDireto(obj);
                else if (parametros.tipoAcessoSiecon == 1) lista = await BoletosAcessoViaAPI(obj, parametros);

                ret.mensagem = "ok";
                ret.sucesso = true;
                ret.objeto = lista;

                return ret;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ret.mensagem = ex.Message;
                ret.sucesso = false;
                ret.objeto = null;
                return ret;
            }

        }

        public async Task<List<Boleto>> Listar(ListarBoletosRequisicao obj)
        {

            //var dadosusuario = GetDadosUsuario(obj);
            //obj.codigoContratoSP7 = dadosusuario.codigoContratoSP7;
            //obj.codigoclientesp7 = dadosusuario.codigoClienteSP7;

            Console.WriteLine("Contrato: " + obj.codigocontratosp7 + " Cliente: " + obj.codigoclientesp7);

            var lista = new List<Boleto>();
            var parametros = ParametrosService.consultar(_connectionString);

            Console.WriteLine($"Acesso Siecon: {parametros.tipoAcessoSiecon} ");

            if (parametros.tipoAcessoSiecon == 0) lista = await BoletosAcessoDireto(obj);
            else if (parametros.tipoAcessoSiecon == 1) lista = await BoletosAcessoViaAPI(obj, parametros);

            return lista.ToList();
        }


        private async Task<List<Boleto>> BoletosAcessoDireto(ListarBoletosRequisicao obj)
        {
            var retorno = new List<Boleto>();

            var sendEmail = new SendEmailService(_configuration);
            var info = $"Listar Boleto Service: {obj.empreendimentosp7}, {obj.blocosp7} , {obj.unidadesp7}";
            var sql = $"select LPad(extract(day from BOLETODTVENC),2,'0') ||'/'|| LPad(extract(month from BOLETODTVENC),2,'0') ||'/'|| LPad(extract(year from BOLETODTVENC),4,'0') AS BOLETODTVENC " +
                        $", BOLETOVALOR, COBRANCA, CONTRATO, RECTO, LINHADIGITAVEL, RECTO_FORNECEDOR, PROPONENTES " +
                        $"from s_portal_parametros({obj.empreendimentosp7}, {obj.blocosp7} ,'{obj.unidadesp7}',1) where BOLETOVALOR IS NOT NULL ";
            Console.WriteLine(sql);
            Console.WriteLine(info);
            Console.WriteLine(_connectionStringFB);
            

            try
            {
                var con = new FbConnection(_connectionStringFB);
                con.Open();
                FbCommand command = new FbCommand(sql, con);

                var r = command.ExecuteReader();

                while (r.Read())
                {
                    var proponentes = r["PROPONENTES"].ToString();

                    if (proponentes == null) proponentes = r["RECTO_FORNECEDOR"].ToString();

                    if (proponentes.Contains(obj.codigoclientesp7))
                    {
                        retorno.Add(new Boleto
                        {
                            BOLETODTVENC = r["BOLETODTVENC"].ToString(),
                            BOLETOVALOR = (float)Convert.ToDouble(r["BOLETOVALOR"]),
                            COBRANCA = Convert.ToInt32(r["COBRANCA"]),
                            CONTRATO = obj.codigocontratosp7,
                            RECTO = Convert.ToInt32(r["RECTO"]),
                            LINHADIGITAVEL = r["LINHADIGITAVEL"].ToString()
                        });
                    }

                }
                r.Close();
                con.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            
            return retorno.ToList();
        }

        private async Task<List<Boleto>> BoletosAcessoDiretoAntigo(ListarBoletosRequisicao obj)
        {
            var retorno = new List<Boleto>();

            try
            {
                var sendEmail = new SendEmailService(_configuration);
                var info = $"Listar Boleto Service: {obj.empreendimentosp7}, {obj.blocosp7} , {obj.unidadesp7}";
                var sql = $"select LPad(extract(day from BOLETODTVENC),2,'0') ||'/'|| LPad(extract(month from BOLETODTVENC),2,'0') ||'/'|| LPad(extract(year from BOLETODTVENC),4,'0') AS BOLETODTVENC " +
                          $", BOLETOVALOR, COBRANCA, CONTRATO, RECTO, LINHADIGITAVEL " +
                          $"from s_portal_parametros({obj.empreendimentosp7}, {obj.blocosp7} ,'{obj.unidadesp7}',1) where BOLETOVALOR IS NOT NULL ";
                Console.WriteLine(sql);
                Console.WriteLine(info);
                Console.WriteLine(_connectionStringFB);
                for (int i = 1; i <= 3; i++)
                {
                    try
                    {
                        var con = new FbConnection(_connectionStringFB);
                        con.Open();
                        FbCommand command = new FbCommand(sql, con);

                        var r = command.ExecuteReader();

                        while (r.Read())
                        {
                            if (r["RECTO_FORNECEDOR"].ToString() == obj.codigoclientesp7)
                            {
                                retorno.Add(new Boleto
                                {
                                    BOLETODTVENC = r["BOLETODTVENC"].ToString(),
                                    BOLETOVALOR = (float)Convert.ToDouble(r["BOLETOVALOR"]),
                                    COBRANCA = Convert.ToInt32(r["COBRANCA"]),
                                    CONTRATO = r["CONTRATO"].ToString(),
                                    RECTO = Convert.ToInt32(r["RECTO"]),
                                    LINHADIGITAVEL = r["LINHADIGITAVEL"].ToString()
                                });
                            }
                        }
                        r.Close();
                        con.Close();
                        return retorno.ToList();
                    }
                    catch (Exception ex)
                    {
                        var corpo = $"tentativa: {i} - erro: {ex.Message} - {info} " + Environment.NewLine + sql;
                        Console.WriteLine(corpo);
                        sendEmail.Send("ERRO BOLETO", corpo, "ronaldo@codemaker.tech");
                        Thread.Sleep(1000);
                    }
                }
                return retorno.ToList();
            }
            catch (Exception)
            {
                throw;
            }

        }

        private async Task<List<Boleto>> BoletosAcessoViaAPI(ListarBoletosRequisicao obj, Poliview.crm.domain.Parametros parametros)
        {
            var retorno = new List<Boleto>();
            var httpClient = new HttpClient();
            var token = await ResultToken(parametros.urlApiSiecon, parametros.usuarioApiSiecon, parametros.senhaApiSiecon);

            var url = parametros.urlApiSiecon + $"/api/FichaFinanceira?dataDe=1900-01-01&dataAte=2100-01-01&codigoCliente={obj.codigoclientesp7}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync(url);
            var resp = response.EnsureSuccessStatusCode();
            var boletos = new List<BoletoAPIResposta>();


            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("Objeto").ToString();
            if (!string.IsNullOrWhiteSpace(values))
            {
                boletos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BoletoAPIResposta>>(values);
                if (boletos == null) boletos = new List<BoletoAPIResposta>();
            }

            foreach (var boletoapi in boletos)
            {
                // var retornoDetalheBoleto = await DetalheBoleto(parametros.urlApiSiecon, obj.codigoclientesp7, boletoapi.CodigoParcela, 0, token);

                if (boletoapi.CodigoContrato == obj.codigocontratosp7)
                {
                    retorno.Add(new Boleto
                    {
                        BOLETODTVENC = $"{boletoapi.DtVenc.Substring(8, 2)}/{boletoapi.DtVenc.Substring(5, 2)}/{boletoapi.DtVenc.Substring(0, 4)}",
                        BOLETOVALOR = (float)Convert.ToDouble(boletoapi.Valor),
                        COBRANCA = boletoapi.Cobranca,
                        CONTRATO = boletoapi.CodigoContrato,
                        RECTO = boletoapi.ItemCobranca,
                        LINHADIGITAVEL = boletoapi.LinhaDigitavel
                    });
                }
            }

            return retorno.ToList();
        }

        private async Task<DetalheBoletoAPIResposta> DetalheBoleto(string urlApiSiecon, string codigoclientesp7, int CodigoParcela, int geraboleto, string token)
        {
            // /api/FichaFinanceira?dataDe=1900-01-01&dataAte=2100-01-01&codigoCliente=00000000000042&codigoParcela=1707&geraBoleto=0
            var httpClient = new HttpClient();
            var url = $"{urlApiSiecon}/api/FichaFinanceira?dataDe=1900-01-01&dataAte=2100-01-01&codigoCliente={codigoclientesp7}&codigoParcela={CodigoParcela}&geraBoleto={geraboleto}";
            var response = await httpClient.GetAsync(url);
            var resp = response.EnsureSuccessStatusCode();
            var detalheboleto = new DetalheBoletoAPIResposta();

            var jsonResult = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.SelectToken("Objeto").ToString();
            if (!string.IsNullOrWhiteSpace(values))
            {
                detalheboleto = Newtonsoft.Json.JsonConvert.DeserializeObject<DetalheBoletoAPIResposta>(values);
                if (detalheboleto == null) detalheboleto = new DetalheBoletoAPIResposta();
            }
            return detalheboleto;
        }

        public async Task<string> GeraPDFBoleto(string codigoclientesp7, int CodigoParcela)
        {
            var parametros = new Poliview.crm.domain.Parametros();
            var token = await ResultToken(parametros.urlApiSiecon, parametros.usuarioApiSiecon, parametros.senhaApiSiecon);
            var boleto = await DetalheBoleto(parametros.urlApiSiecon, codigoclientesp7, CodigoParcela, 1, token);
            return boleto.ArqBoleto;
        }

        private async Task<string> ResultToken(string url, string usuario, string senha)
        {
            var data = "";
            var token = new Token();
            var dict = new Dictionary<string, string>();
            dict.Add("userName", usuario);
            dict.Add("Password", senha);
            dict.Add("grant_type", "password");
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, url + "/token") { Content = new FormUrlEncodedContent(dict) };
            var res = await client.SendAsync(req);

            var jsonResult = await res.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(jsonResult);
            string? values = jObject.ToString();
            if (!string.IsNullOrWhiteSpace(values))
            {
                token = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(values);
                if (token == null) data = ""; else data = token.access_token;
            }
            else
            {
                data = "";
            }
            return data;
        }        

        private DadosUsuario GetDadosUsuario(ListarBoletosRequisicao obj)
        {
            // and CD_ClienteSP7='{obj.codigoclientesp7}'
            using var connection = new SqlConnection(_connectionString);

            var query = "select p.CD_ClienteSP7 as codigoClienteSP7, p.CD_ContratoSP7 as codigoContratoSP7, c.CD_EmpreeSP7, c.CD_BlocoSP7, c.NR_UnidadeSP7 from CAD_PROPONENTE p " +
            "left join CAD_CONTRATO c on c.CD_ContratoSP7 = p.CD_ContratoSP7  " +
            $"where p.ativo = 'S' and p.CD_ContratoSP7 = '{obj.codigocontratosp7}' and c.cd_empreeSP7 = {obj.empreendimentosp7} and c.CD_BlocoSP7 = {obj.blocosp7} and c.NR_UnidadeSP7 = '{obj.unidadesp7}' and p.CD_ClienteSP7='{obj.codigoclientesp7}'";
            // var query = $"select CD_ClienteSP7 as codigoClienteSP7, CD_ContratoSP7 as codigoContratoSP7 from cad_contrato " +
            //    $"where cd_empreeSP7={obj.empreendimentosp7} and CD_BlocoSP7={obj.blocosp7} and NR_UnidadeSP7='{obj.unidadesp7}' and CD_ContratoSP7='{obj.codigoContratoSP7}'";
            var result = connection.QueryFirst<DadosUsuario>(query);
            return result;
        }

    }

}
