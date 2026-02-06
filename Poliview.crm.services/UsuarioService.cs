using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Poliview.crm.domain;
using Poliview.crm.models;
using Poliview.crm.infra;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using FirebirdSql.Data.FirebirdClient;

namespace Poliview.crm.services
{
    public interface IUsuarioService
    {
        public int Incluir(IncluirUsuarioRequisicao obj);
        public LoginResposta Login(LoginRequisicao obj, Jwt jwt);
        public EsqueceuSenhaResposta EsqueceuSenha(EsqueceuSenhaRequisicao obj);
        public PrimeiroAcessoResposta PrimeiroAcesso(PrimeiroAcessoRequisicao obj);
        public PrimeiroAcessoDadosResposta PrimeiroAcessodados(PrimeiroAcessoDadosRequisicao obj);
        public IEnumerable<Usuario> ListarTodos();
        public Usuario ListarUsuario(int idUsuario);
        public Usuario ValidarToken(string token, string key);
        public IEnumerable<UnidadesUsuarioResposta> ListarUnidades(UnidadesUsuarioRequisicao obj);
        public AlterarUsuarioResposta Alterar(AlterarUsuarioRequisicao obj, int idUsuario);
        public string[] validarSenha(int idusuario, string senha);
        public bool ExistemMensagensParaUsuario(string cpf);
        public Task<Object> ExistemNotificacoesParaUsuario(string cpf);
        public Task<List<Contrato>> ListarContratosCliente(string? codigoclientesp7);
        public TrocarSenhaResposta TrocarSenha(int idusuario, string senhaatual, string novasenha, string repetirnovasenha);
        public dadosUsuario RetornaDadosUsuario(string cpf);
        public string OrigemValida(int idorigem);
        public Retorno Integrar(string codigoclientesp7, Boolean integrarSoClientes = false);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly string? _connectionString;
        private readonly string? _connectionStringFB;
        private IConfiguration _configuration;
        private string? usuarioSiecon;
        private string? senhaSiecon;

        public UsuarioService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
            usuarioSiecon = configuration["usuarioSiecon"];
            senhaSiecon = configuration["senhaSiecon"];
            _connectionStringFB = configuration["conexaoFirebird"];
        }

        public AlterarUsuarioResposta Alterar(AlterarUsuarioRequisicao obj, int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"UPDATE OPE_USUARIO " +
                        $"SET NM_USUARIO=@NM_USUARIO, DS_EMAIL=@DS_EMAIL, IN_STATUS=@IN_STATUS, IN_BLOQUEADO=@IN_BLOQUEADO, IN_USUARIOSISTEMA=@IN_USUARIOSISTEMA " +
                        $"WHERE CD_USUARIO=@CD_USUARIO";

            var parameters = new
            {
                CD_USUARIO = idUsuario,
                NM_USUARIO = obj.NM_USUARIO,
                DS_EMAIL = obj.DS_EMAIL,
                IN_STATUS = obj.IN_STATUS,
                IN_BLOQUEADO = obj.IN_BLOQUEADO,
            };
            var result = connection.ExecuteAsync(query, parameters);

            var Usuario = this.RetornaUsuarioPorId(idUsuario);

            var ret = new AlterarUsuarioResposta();
            ret.CD_USUARIO = Usuario.CD_USUARIO;
            ret.NM_USUARIO = Usuario.NM_USUARIO;
            ret.IN_BLOQUEADO = Usuario.IN_BLOQUEADO;
            ret.IN_USUARIOSISTEMA = Usuario.IN_USUARIOSISTEMA;
            ret.DS_EMAIL = Usuario.DS_EMAIL;
            ret.DS_SENHA = Usuario.DS_EMAIL;
            ret.IN_STATUS = Usuario.IN_STATUS;

            return ret;

        }

        public TrocarSenhaResposta TrocarSenha(int idusuario, string senhaatual, string novasenha, string repetirnovasenha)
        {
            var retorno = new TrocarSenhaResposta();

            using var connection = new SqlConnection(_connectionString);

            var user = RetornaUsuarioPorId(idusuario);

            if (user.DS_SENHA != Criptografia.Criptografar(senhaatual))
            {
                retorno.sucesso = 0;
                retorno.mensagem = "Senha atual inválida";
            }
            else
            {
                if (novasenha == repetirnovasenha)
                {
                    var ret = this.validarSenha(idusuario, novasenha);

                    if (ret.Count() == 0)
                    {
                        try
                        {
                            var senhacripto = Criptografia.Criptografar(novasenha);
                            var query = $"UPDATE OPE_USUARIO " +
                                        $"SET DS_SENHA='{senhacripto}'  " +
                                        $"WHERE CD_USUARIO={idusuario}";

                            Console.WriteLine(query);

                            var result = connection.ExecuteAsync(query);
                            retorno.sucesso = 1;
                            retorno.mensagem = "Senha alterada com sucesso!";

                        }
                        catch (System.Exception e)
                        {
                            retorno.sucesso = 0;
                            retorno.mensagem = e.Message;
                        }
                    }
                    else
                    {
                        var strmsg = "";
                        foreach (var item in ret)
                        {
                            // mensagem.Add(item);    
                            if (strmsg == "")
                                strmsg += item;
                            else
                                strmsg += "," + item;
                        }

                        retorno.sucesso = 0;
                        retorno.mensagem = strmsg;

                    }
                }
                else
                {
                    retorno.sucesso = 0;
                    retorno.mensagem = "As senhas não conferem";
                }
            }
            return retorno;
        }

        public EsqueceuSenhaResposta EsqueceuSenha(EsqueceuSenhaRequisicao obj)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"exec API_ESQUECEU_SENHA @email='{obj.email}', @idempresa={obj.idempresa}";

            Console.WriteLine(query);

            var result = connection.QueryFirst<EsqueceuSenhaResposta>(query);

            return result;
        }

        public int Incluir(IncluirUsuarioRequisicao obj)
        {
            var proxnum = ProximoNumeroUsuario();

            using var connection = new SqlConnection(_connectionString);

            var query = "INSERT INTO OPE_USUARIO " +
                        "(CD_USUARIO, NM_USUARIO, DS_EMAIL, IN_STATUS, IN_BLOQUEADO, NR_CPFCNPJ, DS_SENHA, IN_USUARIOSISTEMA) " +
                        "VALUES" +
                        "(@CD_USUARIO, @NM_USUARIO, @DS_EMAIL, @IN_STATUS, @IN_BLOQUEADO, @NR_CPFCNPJ, @DS_SENHA, @IN_USUARIOSISTEMA) ";
            var parameters = new
            {
                CD_USUARIO = proxnum,
                NM_USUARIO = obj.NM_USUARIO,
                DS_EMAIL = obj.DS_EMAIL,
                IN_STATUS = "A",
                IN_BLOQUEADO = "N",
                NR_CPFCNPJ = obj.NR_CPFCNPJ,
                DS_SENHA = Criptografia.Criptografar(obj.DS_SENHA),
                IN_USUARIOSISTEMA = "N"
            };
            var result = connection.ExecuteAsync(query, parameters);

            return proxnum;

        }

        public IEnumerable<Usuario> ListarTodos()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = string.Format("select CD_USUARIO, NM_USUARIO, DS_EMAIL, DS_SENHA, IN_BLOQUEADO, IN_STATUS, NR_CPFCNPJ, IN_USUARIOSISTEMA FROM OPE_USUARIO");
            var result = connection.Query<Usuario>(query);
            return result;
        }

        public IEnumerable<UnidadesUsuarioResposta> ListarUnidades(UnidadesUsuarioRequisicao obj)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "exec API_Lista_Unidades_Usuario @CPF = @cpf";

            var result = connection.Query<UnidadesUsuarioResposta>(query, new { cpf = obj.cpf });

            return result;
        }

        public Usuario ListarUsuario(int idUsuario)
        {
            return this.RetornaUsuarioPorId(idUsuario);
        }

        public Usuario ValidarToken(string token, string key)
        {
            // var TKS = new TokenService();
            var idusuario = TokenService.ValidateJwtToken(token, key);

            if (idusuario == null)
                idusuario = -1;

            return RetornaUsuarioPorId((int)idusuario);
        }

        public LoginResposta? Login(LoginRequisicao obj, Jwt jwt)
        {
            if (!Util.emailValido(obj.usuario))
            {
                obj.usuario = Util.RemoverPontosTracosBarras(obj.usuario);
            }

            Usuario user = RetornaUsuarioPorEmail(obj.usuario, obj.idempresa);
            var senhapadrao = this.RetornaSenhaPadrao();

            var ret = new LoginResposta();

            if (user != null)
            {
                ret.CD_USUARIO = user.CD_USUARIO;
                ret.DS_EMAIL = user.DS_EMAIL;
                ret.DS_SENHA = user.DS_SENHA;
                ret.IN_BLOQUEADO = user.IN_BLOQUEADO;
                ret.IN_STATUS = user.IN_STATUS;
                ret.IN_USUARIOSISTEMA = user.IN_USUARIOSISTEMA;
                ret.IN_CLIENTE = user.IN_CLIENTE;
                ret.NM_USUARIO = user.NM_USUARIO;
                ret.NR_CPFCNPJ = user.NR_CPFCNPJ;
                ret.Data_Nascimento = user.Data_Nascimento;
                ret.CD_ClienteSP7 = user.CD_CLIENTESP7;
                ret.CD_Cliente = user.CD_CLIENTE;
                ret.acessopadrao = (obj.senha == senhapadrao);
                ret.mensagens = this.mensagensUsuario(user.NR_CPFCNPJ, "0");
                ret.idempresa = obj.idempresa;
                ret.cadastramentodireto = user.cadastramentodireto;
                ret.versaoapp = user.versaoapp;
                ret.versaoportal = user.versaoportal;
                ret.versaoappandroid = user.versaoappandroid;
                ret.NovasMensagens = this.ExistemMensagensChamados(user.CD_CLIENTE) ? 1 : 0;
                // ronaldo
                var senhacriptografada = Criptografia.Criptografar(obj.senha);

                var ismatch = (senhacriptografada == user.DS_SENHA || ret.acessopadrao);

                if (!ismatch)
                {
                    ret.CD_USUARIO = -1;
                    ret.token = "";
                }
                else
                {
                    this.Integrar(ret.NR_CPFCNPJ);
                    if (obj.origem != 9) AdicionarAcessoUsuario(user.CD_USUARIO, obj.origem, ret.acessopadrao);
                    var token = TokenService.GenerateJwtToken(user.CD_USUARIO, user.NM_USUARIO, user.DS_EMAIL, user.NR_CPFCNPJ, jwt.Subject, jwt.Issuer, jwt.Audience, jwt.key);
                    ret.token = token;
                }
            }
            else
            {
                ret.CD_USUARIO = -2;
            }

            // tem que mostrar a senha pois vai usar para o MANTER CONECTADO.
            // ret.DS_SENHA = ""; // zera a senha guardada na base.

            return ret;

        }

        public dadosUsuario RetornaDadosUsuario(string cpf)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select CLI.NM_Cliente as nome, CLI.DS_Email as email, CLI.CD_ClienteSP7 as codigoclientesp7, CLI.NR_CPFCNPJ as cpf, cli.DS_Endereco as endereco, cli.NM_Bairro as bairro, cli.NM_Cidade as cidade, cli.NM_UF as estado, cli.NR_CEP as cep, cli.NR_DDD + ' ' +cli.NR_Celular as celular from CAD_CLIENTE CLI where NR_CPFCNPJ = @cpf";
            var result = connection.QueryFirstOrDefault<dadosUsuario>(query, new { cpf });
            return result;
        }


        public PrimeiroAcessoResposta PrimeiroAcesso(PrimeiroAcessoRequisicao obj)
        {
            using var connection = new SqlConnection(_connectionString);

            if (obj.idempresa == 0) obj.idempresa = 1;

            var senhacripto = Criptografia.Criptografar(obj.senha);

            var query = "exec API_PRIMEIRO_ACESSO @CPF = @cpf, @SENHA_CRIPTO = @senhacripto, @IDEMPRESA = @idempresa";

            var result = connection.QueryFirst<PrimeiroAcessoResposta>(query, new { cpf = obj.cpf, senhacripto, idempresa = obj.idempresa });

            return result;

        }

        public PrimeiroAcessoDadosResposta PrimeiroAcessodados(PrimeiroAcessoDadosRequisicao obj)
        {

            this.Integrar(obj.cpf, true);

            using var connection = new SqlConnection(_connectionString);

            var query = "exec API_PRIMEIRO_ACESSO_DADOS @CPF = @cpf";

            var result = connection.QueryFirst<PrimeiroAcessoDadosResposta>(query, new { cpf = obj.cpf });

            return result;
        }

        public string[] validarSenha(int idusuario, string senha)
        {
            var ret = new PoliticaSenhaService(_configuration);
            return ret.validar(idusuario, senha);
        }

        private Usuario RetornaUsuarioPorEmail(string emailOuCpf, int idempresa = 1)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "exec dbo.CRM_RETORNA_USUARIO @cpfouemail = @emailOuCpf, @idempresa = @idempresa";
            var result = connection.QueryFirstOrDefault<Usuario>(query, new { emailOuCpf, idempresa });
            return result;
        }

        private Usuario RetornaUsuarioPorId(int idUsuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $@"select usu.CD_USUARIO, cli.CD_Cliente, cli.CD_CLIENTESP7, usu.NM_USUARIO, usu.DS_EMAIL,
                           usu.DS_SENHA, usu.NR_CPFCNPJ, usu.IN_BLOQUEADO, usu.IN_STATUS, usu.IN_USUARIOSISTEMA
                           from OPE_USUARIO usu
                           LEFT JOIN CAD_CLIENTE cli on cli.NR_CPFCNPJ = usu.NR_CPFCNPJ
                           where usu.CD_USUARIO = @idUsuario";
            var result = connection.QueryFirstOrDefault<Usuario>(query, new { idUsuario });
            return result;
        }

        private int ProximoNumeroUsuario()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select MAX(CD_USUARIO)+1 as CD_USUARIO from OPE_USUARIO";
            var result = connection.QueryFirstOrDefault<Usuario>(query);

            return result.CD_USUARIO;
        }
        public bool ExistemMensagensParaUsuario(string cpf)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "exec dbo.CRM_Listar_Mensagens_Usuario_carrossel @cpfcliente = @cpf, @idmensagem = 0";
                var result = connection.Query<MensagemUsuario>(query, new { cpf });

                Console.WriteLine(query);

                return result.Count() > 0;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        public int mensagensUsuario(string cpf, string idmensagem)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "exec dbo.CRM_Listar_Mensagens_Usuario @cpfcliente = @cpf, @idmensagem = @idmensagem";
                var result = connection.Query<MensagemUsuario>(query, new { cpf, idmensagem });

                Console.WriteLine(query);

                return result.Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private void AdicionarAcessoUsuario(int idusuario, int idorigem, Boolean senhapadrao)
        {
            using var connection = new SqlConnection(_connectionString);
            var senha = senhapadrao ? "S" : "N";
            var query = "insert into OPE_ACESSOS (idusuario, idorigem, senhapadrao) values (@idusuario, @idorigem, @senha)";
            connection.Query(query, new { idusuario, idorigem, senha });
        }

        public async Task<Object> ExistemNotificacoesParaUsuario(string cpf)
        {
            var token = await loginApiSieconAsync(usuarioSiecon, senhaSiecon);

            var obj = await ConsultaDadosDoCliente(cpf, token);

            return obj;

        }

        public async Task<string> loginApiSieconAsync(string usuario, string senha)
        {
            var http = new HttpClient();
            try
            {
                var Senhabase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(senha));
                char[] charArray = Senhabase64.ToCharArray();
                Array.Reverse(charArray);
                var SenhaBase64Invertida = new string(charArray);

                Dictionary<string, string> postvalues = new Dictionary<string, string>
                {
                   { "userName", usuario },
                   { "Password", SenhaBase64Invertida },
                   { "grant_type", "password" }
                };
                var content = new FormUrlEncodedContent(postvalues);

                var response = await http.PostAsync("token", content);

                var responseString = await response.Content.ReadAsStringAsync();
                Dictionary<string, string> responseJSON = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                var token = responseJSON["access_token"];

                return token.ToString();

                // var js = new Microsoft.JSInterop.JSRuntime();
                // var localstorage = new LocalStorage
                /*
                using (var storage = new LocalStorage())
                {
                    // store any object
                    var weapon = new Weapon("Lightsaber");
                    storage.Save("weapon", weapon);

                    // ... and retrieve the object back
                    var target = storage.Get<Weapon>("weapon");

                    // or store + get a collection
                    var villains = new string[] { "Kylo Ren", "Saruman", "Draco Malfoy" };
                    storage.Save("villains", villains);

                    // ... and get it back as an IEnumerable
                    var target = storage.Query<string>("villains");

                    // finally, persist the stored objects to disk (.localstorage file)
                    storage.Persist();
                }
                */


            }
            catch (Exception)
            {

                throw;
            }
            finally { http.Dispose(); }
        }

        public async Task<object> ConsultaDadosDoCliente(string cpf, string token)
        {

            var http = new HttpClient();
            try
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await http.GetAsync($"/api/DadosCliente?cnpjCpf={cpf}&dataAssCtrDe=1900-01-01&dataAssCtrAte=2100-01-01");
                var resp = response.EnsureSuccessStatusCode();
                var jsonResult = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(jsonResult);
                //string? values = jObject.ToString();
                string? values = jObject.SelectToken("Objeto").ToString();
                if (string.IsNullOrWhiteSpace(values)) return new Object();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Object>(values);
                if (data == null) return new Object();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
            finally { http.Dispose(); }

        }

        public async Task<List<Contrato>> ListarContratosCliente(string? codigoclientesp7)
        {
            using var connection = new FbConnection(_connectionStringFB);
            var query = "SELECT distinct PROP.PROPONENTE_FORNECEDOR as codigocliente, cli.forn_razao as nome, cli.forn_dtnascimento as nascimento, " +
            "prop.proponente_percentual as percentual, " +
            // coalesce(S.RECTO_TOTSALDO,0) as saldodevedor, " +
            "con.ctr_cdg as codigocontrato, con.ctr_dtassinatura as assinatura, con.ctr_remanejado as remanejado, " +
            "con.ctr_cancelado as cancelado, " +
            "emp.emprd_desc as empreendimento, blo.bloco_desc as bloco, uni.undemprd_desc as unidade, con.CTR_STATUS as status, con.CTR_STATUSDISTRATO as distratado " +
            "FROM emp_proponente prop " +
            "LEFT JOIN EMP_CTR con on con.ctr_cdg = prop.proponente_ctr " +
            "LEFT JOIN cadcpg_fornecedor cli on cli.forn_cnpj = prop.proponente_fornecedor " +
            "LEFT JOIN caddvs_empreend emp on emp.emprd_cdg = con.ctr_emprd " +
            "left JOIN caddvs_bloco blo on blo.bloco_emprd = con.ctr_emprd and blo.bloco_cdg = con.ctr_bloco " +
            "left JOIN emp_undemprd uni on uni.undemprd_emprd = con.ctr_emprd and uni.undemprd_bloco = con.ctr_bloco and uni.undemprd_cdg = con.ctr_undemprd " +
            // "LEFT JOIN S_CRB_CALCRECTOCOMPEMPRD('-1', '-1', '-1', '-1','-1', '-1', '1900-01-01', '2100-01-01', -1, -1, -1, -1, '-1', -1, -1,NULL, 'N', -1, -1, con.ctr_cdg, -1, -1, 'S') S on s.recto_cdg>0 " +
            "WHERE prop.PROPONENTE_FORNECEDOR = @codigoclientesp7";
            var result = connection.Query<Contrato>(query, new { codigoclientesp7 });

            /*
            if (codigoclientesp7 != "00000000001183")
            {
                foreach (var registro in result)
                {
                    // var ativo = VerificaSeContratoOuSessaoEstaAtivo(registro.codigocontrato, registro.codigocliente);
                    // registro.ativo = ativo;
                    //var saldo = SaldoDevedorContrato(registro.codigocontrato);
                    // registro.saldodevedor = registro.saldodevedor;

                    if (registro.saldodevedor == 0)
                    {
                        var data = await DataQuitacaoContrato(registro.codigocontrato);
                        registro.dataquitacao = data;
                    }
                    else
                    {
                        registro.dataquitacao = DateTime.MinValue;
                    }
                }

            } 
            */

            return result.ToList();
        }

        private float SaldoDevedorContrato(string? codigoContrato)
        {
            float result = 0;
            try
            {
                using var connection = new FbConnection(_connectionStringFB);
                var query = "select SUM(S.RECTO_TOTSALDO) AS TOTAL from S_CRB_CALCRECTOCOMPEMPRD('-1', '-1', '-1', '-1','-1', '-1', '1900-01-01', '2100-01-01', -1, -1, -1, -1, '-1', -1, -1,NULL, 'N', -1, -1, @codigoContrato, -1, -1, 'S') S";
                result = (float)connection.QueryFirstOrDefault<float>(query, new { codigoContrato });
            }
            catch (Exception ex)
            {
                Console.WriteLine("erro saldo devedor contrato: " + ex.Message);
                result = 0;
            }

            return result;
        }

        public string retornaAtendenteDoChamado(int idchamado, int idocorrencia, int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "select uem.email " +
            "from ope_chamado_det det " +
            "left join ope_chamado cha on cha.CD_Chamado = det.CD_Chamado " +
            "left join CAD_EMPREENDIMENTO emp on emp.CD_Empreendimento = cha.CD_Empreendimento " +
            "left join ope_usuario_email uem on uem.CD_Usuario = det.CD_UsuRecurso and uem.idempresa = emp.idempresa " +
            "where det.CD_Chamado = @idchamado and det.CD_Ocorrencia = @idocorrencia";


            try
            {
                var result = connection.QueryFirst<string>(query, new { idchamado, idocorrencia });
                return result;
            }
            catch (Exception ex)
            {
                // Console.WriteLine("erro: " + ex.Message + " - " + query);
                return "";
            }
        }

        public IEnumerable<usuariosGrupo> retornaUsuariosMasterGrupo(int idgrupo, int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"SELECT cd_usuario as idusuario, NM_usuario as nome, " +
                        $"DS_Email as email, CD_Grupo as idgrupo, NM_Grupo as grupo, " +
                        $"IN_Master as master, IN_Supervisor as supervisor " +
                        $"FROM vUsuariosGrupo WHERE CD_Grupo = {idgrupo} and in_master=1 and IN_Status='A' and idcontaemail={idcontaemail}";

            var result = connection.Query<usuariosGrupo>(query);

            return result;
        }

        public IEnumerable<usuariosGrupo> retornaUsuariosGrupo(int idgrupo, int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"SELECT cd_usuario as idusuario, NM_usuario as nome, " +
                        $"DS_Email as email, CD_Grupo as idgrupo, NM_Grupo as grupo, " +
                        $"IN_Master as master, IN_Supervisor as supervisor " +
                        $"FROM vUsuariosGrupo WHERE CD_Grupo = {idgrupo} and IN_Status='A' and idcontaemail={idcontaemail}";

            var result = connection.Query<usuariosGrupo>(query);

            return result;
        }

        public IEnumerable<usuariosGrupo> retornaUsuariosSupervisorGrupo(int idgrupo, int idcontaemail)
        {
            using var connection = new SqlConnection(_connectionString);

            var query = $"SELECT cd_usuario as idusuario, NM_usuario as nome, " +
                        $"DS_Email as email, CD_Grupo as idgrupo, NM_Grupo as grupo, " +
                        $"IN_Master as master, IN_Supervisor as supervisor " +
                        $"FROM vUsuariosGrupo WHERE in_supervisor=1 and CD_Grupo={idgrupo} and idcontaemail={idcontaemail}";
            try
            {
                var result = connection.Query<usuariosGrupo>(query);
                return result;
            }
            catch (Exception ex)
            {
                // Console.WriteLine("erro: " + ex.Message + " - " + query);
                return new List<usuariosGrupo>();
            }
        }

        private string VerificaSeContratoOuSessaoEstaAtivo(string? codigoContrato, string codigocliente)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select ativo from CAD_PROPONENTE where CD_ContratoSP7='{codigoContrato}' and CD_ClienteSP7='{codigocliente}'";
                var result = connection.QueryFirstOrDefault(query);
                if (result == null) return "N";
                return result.ativo;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<DateTime> DataQuitacaoContrato(string? codigoContrato)
        {
            var dataRet = DateTime.MinValue;

            using var connection = new FbConnection(_connectionStringFB);
            var query = "SELECT coalesce(MAX(RB.RECTOBAIXA_DTRECTO),'01/01/0001 00:00:00') as dataquitacao " +
                        $"FROM S_CRB_CALCRECTOEFETUADO(-1, '-1', '-1', '-1', '-1', '-1', NULL, NULL,-1, '-1', '-1', -1, '-1', -1, -1, CURRENT_DATE,-1, '{codigoContrato}', -1, -1, 0, -1) C " +
                        "LEFT JOIN CRB_RECTOBAIXA RB ON C.RECTOBAIXA_CDG = RB.RECTOBAIXA_CDG ";
            var result = connection.QueryFirstOrDefault<DateTime>(query);

            return result;
        }
        private string RetornaSenhaPadrao()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"select top 1 senhaPadrao from OPE_PARAMETRO where CD_BancoDados=1 and CD_Mandante=1";
            var result = connection.QueryFirstOrDefault(query);
            if (result == null) return "999";
            return result.senhaPadrao;
        }

        public string OrigemValida(int idorigem)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"select coalesce(descricao,'') as descricao from CAD_ORIGEM_CHAMADO where id={idorigem} and loginhabilitado=1 ";
            var result = connection.QueryFirstOrDefault<string>(query);
            if (result == null) return "";
            return result;
        }

        private List<ClientesIntegracao> ListarClientes(string codigoclientesp7)
        {
            using var connection = new FbConnection(_connectionStringFB);
            var sql = "SELECT " +
                           "FORN_CNPJ as codigoclientesp7, FORN_RAZAO as nome, FORN_CPFCNPJ as cpfcnpj, FORN_EMAIL as email, " +
                           "FORN_TPLOGRADOURORES||' '||FORN_LOGRADOURORES||', '||FORN_NUMERORES AS endereco, " +
                           "FORN_BAIRRORES AS bairro, FORN_CIDRES AS cidade, FORN_UF as estado, FORN_CEPRES AS cep, " +
                           "FORN_DDD as ddd, FORN_TELEFONE1 as telefone, FORN_CELULAR1 as celular, FORN_CONTATO1 as contato, FORN_DTNASCIMENTO as datanascimento, " +
                           "FORN_DTLIDOCRM as datahoraultimaatualizacao " +
                           "FROM CADCPG_FORNECEDOR " +
                           $"WHERE FORN_TPCLIENTE=1 AND FORN_CNPJ='{codigoclientesp7}'";
            return connection.Query<ClientesIntegracao>(sql).ToList();
        }

        public List<ContratoIntegracao> ListarContratos(string codigoclientesp7)
        {
            using var connection = new FbConnection(_connectionStringFB);
            var sql = "SELECT " +
                        "CTR_CDG AS contratosp7, CTR_FORNECEDOR as codigoclientesp7, CTR_EMPRE as codigoempresasp7, " +
                        "CTR_EMPRD as codigoempreendimentosp7, CTR_BLOCO as codigoblocosp7, CTR_UNDEMPRD as codigounidadesp7, " +
                        "CTR_STATUS as statuscontratosp7, CTR_STATUSDISTRATO as statusdistrato, CTR_REMANEJADO as statusremanejado, " +
                        "CTR_CNPJCPFRESP as cpfresponsavel, CTR_NOMERESP as nomeresponsavel, CTR_TELEFONERESP as telefoneresponsavel, " +
                        "CTR_CELULARRESP as celularresponsavel, CTR_EMAILRESP as emailresponsavel, CTR_DTLIDOCRM as datahoraultimaatualizacao " +
                        "FROM EMP_CTR " +
                        $"WHERE CTR_FORNECEDOR='{codigoclientesp7}'";
            return connection.Query<ContratoIntegracao>(sql).ToList();
        }

        private List<ProponenteIntegracao> ListarProponentes(string codigocontratosp7)
        {
            using var connection = new FbConnection(_connectionStringFB);
            var sql = "SELECT " +
                    "PROPONENTE_CDG as codigoproponente, " +
                    "PROPONENTE_CTR as contratosp7, " +
                    "PROPONENTE_FORNECEDOR as codigoclientesp7, " +
                    "CASE EMP_CTR.ctr_cdg when PROPONENTE_CTR THEN 'S' ELSE 'N' end as principal, " +
                    "coalesce(crb_cessao.cessao_dt,'1900-01-01 00:00:00') as datacessao, " +
                    "coalesce(crb_cessao.cessao_cdg,0) as codigocessao, " +
                    "coalesce(crb_cessao.cessao_fornecedoratual,'') as clienteatual, " +
                    "coalesce(crb_cessao.cessao_fornecedor,'') as clientenovo, " +
                    "coalesce(crb_cessao.cessao_status,0) as statuscessao, 'N' as ativo,  emp_proponente.proponente_dtlidocrm as datahoraultimaatualizacao " +
                    "FROM EMP_PROPONENTE " +
                    "left JOIN emp_ctr ON EMP_CTR.ctr_fornecedor = EMP_PROPONENTE.proponente_fornecedor AND emp_ctr.ctr_cdg = EMP_PROPONENTE.proponente_ctr " +
                    "left join crb_cessao on cessao_cdg = proponente_cessao " +
                    $"WHERE PROPONENTE_CTR='{codigocontratosp7}'";
            return connection.Query<ProponenteIntegracao>(sql).ToList();
        }

        private List<EmpreendimentosIntegracao> ListarEmpreendimento(int codigoempreendimentosp7)
        {
            using var connection = new FbConnection(_connectionStringFB);
            var sql = "SELECT " +
                            "EMPRD_CDG as empreendimentosp7, EMPRD_DESC as empreendimento, EMPRD_ABREV as abreviatura, EMPRD_END as endereco, " +
                            "EMPRD_BAIRRO as bairro, EMPRD_CID as cidade, EMPRD_UF as estado, EMPRD_CEP as cep, " +
                            "EMPRD_ENTIDADE as entidade, EMPRD_REGIAO as regiao, EMPRD_TEMPRD as tipoempreendimento, " +
                            "EMPRD_PADRAO as empreendimentopadrao, EMPRD_MUNICIPIO as codigomunicipio, " +
                            "EMPRD_DTLIDOCRM as datahoraultimaatualizacao " +
                            "FROM CADDVS_EMPREEND " +
                            $"WHERE EMPRD_CDG={codigoempreendimentosp7}";
            return connection.Query<EmpreendimentosIntegracao>(sql).ToList();
        }

        private List<BlocosIntegracao> ListarBloco(int codigoempreendimentosp7, int codigoblocosp7)
        {
            using var connection = new FbConnection(_connectionStringFB);
            var sql = "SELECT " +
                    "BLOCO_EMPRD as codigoempreendimentosp7, BLOCO_CDG as codigoblocosp7, BLOCO_DESC as nome, " +
                    "BLOCO_ABREV as abreviatura, BLOCO_END as endereco, BLOCO_BAIRRO as bairro, BLOCO_CID as cidade, BLOCO_UF as estado, BLOCO_CEP as cep, " +
                    "BLOCO_DTLIDOCRM as datahoraultimaatualizacao " +
                    "FROM CADDVS_BLOCO " +
                    $"WHERE BLOCO_EMPRD={codigoempreendimentosp7} and BLOCO_CDG={codigoblocosp7}";
            return connection.Query<BlocosIntegracao>(sql).ToList();
        }

        private List<UnidadesIntegracao> ListarUnidade(int codigoempreendimentosp7, int codigoblocosp7, string codigounidadesp7)
        {
            using var connection = new FbConnection(_connectionStringFB);
            var sql = "SELECT " +
                            "UNDEMPRD_EMPRD as codigoempreendimentosp7, UNDEMPRD_BLOCO as codigoblocosp7, " +
                            "UNDEMPRD_CDG as codigounidadesp7, UNDEMPRD_END as endereco, UNDEMPRD_CALCSTATUSUND as status, " +
                            "UNDEMPRD_DTLIDOCRM as datahoraultimaatualizacao " +
                            "FROM EMP_UNDEMPRD " +
                            $"WHERE UNDEMPRD_CDG={codigoempreendimentosp7} and UNDEMPRD_BLOCO={codigoblocosp7} and UNDEMPRD_CDG='{codigounidadesp7}'";
            return connection.Query<UnidadesIntegracao>(sql).ToList();
        }

        public Retorno Integrar(string cpf, Boolean integrarSoClientes = false)
        {
            var ret = new Retorno();
            try
            {
                var clienteServices = new IntegracaoClienteService(_connectionStringFB, _connectionString);
                clienteServices.Integrar(cpf);

                var codigoclientesp7 = clienteServices.RetornaCodigoCliente(cpf);

                if (!integrarSoClientes)
                {
                    var proponentesService = new IntegracaoProponenteService(_connectionStringFB, _connectionString);
                    var contratosService = new IntegracaoContratoService(_connectionStringFB, _connectionString);

                    proponentesService.Integrar(codigoclientesp7);

                    var contratos = contratosService.ListarOrigem(codigoclientesp7);
                    foreach (var contrato in contratos)
                    {
                        var empreendimentosService = new IntegracaoEmpreendimentoService(_connectionStringFB, _connectionString);
                        empreendimentosService.Integrar(contrato.codigoempreendimentosp7);

                        var blocosService = new IntegracaoBlocoService(_connectionStringFB, _connectionString);
                        blocosService.Integrar(contrato.codigoempreendimentosp7, contrato.codigoblocosp7);

                        var unidadesService = new IntegracaoUnidadeService(_connectionStringFB, _connectionString);
                        unidadesService.Integrar(contrato.codigoempreendimentosp7, contrato.codigoblocosp7, contrato.codigounidadesp7);
                    }
                    contratosService.Integrar(codigoclientesp7);
                }
                ret.sucesso = true;
                ret.mensagem = "Integração realizada com sucesso!";
                ret.objeto = null;
                return ret;
            }
            catch (Exception ex)
            {
                ret.sucesso = false;
                ret.mensagem = $"Erro: {ex.Message}";
                ret.objeto = null;
                return ret;
            }

        }

        public Boolean ExistemMensagensChamados(int idcliente)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"select * from ope_chamado where CD_Cliente={idcliente} and NovasMensagens=1 ";
            var result = connection.Query(query);
            return result.Count() > 0;
        }

    }
}