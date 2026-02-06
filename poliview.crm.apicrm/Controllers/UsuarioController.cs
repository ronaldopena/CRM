using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.models;
using Poliview.crm.domain;
using Poliview.crm.infra;
using System.Security.Claims;
using System.Text;
using Poliview.crm.services;

namespace Poliview.crm.apicrm.Controllers
{
    [Route("usuario")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private IUsuarioService _usuarioService;
        private IConfiguration _configuration;
        private string connectionStringSQL;
        private string connectionStringFB;
        private readonly LogService _logService;

        public UsuarioController(IConfiguration configuration, LogService logService)
        {
            _configuration = configuration;
            connectionStringSQL = _configuration["Conexao"].ToString();
            connectionStringFB = _configuration["conexaoFirebird"].ToString();
            _usuarioService = new UsuarioService(configuration);
            _logService = logService;
        }

        [HttpPost("validarsenha")]
        public IActionResult validarSenha(validarSenhaRequisicao obj)
        {
            return Ok(_usuarioService.validarSenha(obj.idusuario, obj.senha));
        }

        [HttpPost("trocarsenha")]
        public IActionResult TrocarSenha(TrocarSenhaRequisicao obj)
        {
            return Ok(_usuarioService.TrocarSenha(obj.idusuario, obj.senhaatual, obj.novasenha, obj.repetirnovasenha));
        }

        [HttpPost("register")]
        public IActionResult Register(IncluirUsuarioRequisicao obj)
        {
            var novoid = _usuarioService.Incluir(obj);
            return Ok(novoid);
        }

        [HttpPost("auth")]
        public IActionResult Authenticate(LoginRequisicao obj)
        {
            try
            {
                var error = "OK";                
                var jwt = this.CreateJwt();
                var origem = "teste";
                if (obj.origem != 9) origem = _usuarioService.OrigemValida(obj.origem);
                var response = _usuarioService.Login(obj, jwt);
                response.origem = origem;
                response.success = true;

                if (response.CD_USUARIO == -1)
                {
                    error = $"Senha inválida";
                    response.success = false;
                }

                if (response.CD_USUARIO == -2)
                {
                    error = $"Usuário não encontrado";
                    response.success = false;
                }

                if (string.IsNullOrEmpty(origem))
                {
                    error = $"Origem inválida! Código informado {obj.origem}";
                    response.success = false;
                }

                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.info,$"LOGIN: {obj.usuario} - ORIGEM: {obj.origem} {origem} | {error}");

                if (error != "OK")
                {
                    var msgError = new HttpErrorMessage(404, error);
                    return BadRequest(msgError);
                }
                else
                    return Ok(response);

            }
            catch (Exception ex)
            {
                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.erro, $"LOGIN: {obj.usuario} - {ex.Message}");
                var msgError = new HttpErrorMessage(417, ex.Message);
                return BadRequest(msgError);
            }

        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequisicao obj)
        {
            try
            {
                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.info, $"Login: {obj.usuario} | {obj.origem} | {obj.idempresa}");

                var jwt = this.CreateJwt();

                var response = _usuarioService.Login(obj, jwt);
                response.success = true;

                if (response.CD_USUARIO == -1)
                {
                    var error = new HttpErrorMessage(400, "Senha inválida");
                    return BadRequest(error);
                }
                if (response.CD_USUARIO == -2)
                {
                    var error = new HttpErrorMessage(400, "Usuário não encontrado");
                    return BadRequest(error);
                }
                else
                {
                    var strToken = TokenService.GenerateJwtToken(response.CD_USUARIO, response.NM_USUARIO, response.DS_EMAIL, response.NR_CPFCNPJ, jwt);
                    response.token = strToken;
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.erro, $"LOGIN: {obj.usuario} - {ex.Message}");
                var msgError = new HttpErrorMessage(417, ex.Message);
                return BadRequest(msgError);
            }            
        }
               
        [HttpPost("esqueceusenha")]
        public IActionResult EsqueceuSenha(EsqueceuSenhaRequisicao obj)
        {
            var resposta = _usuarioService.EsqueceuSenha(obj);

            return Ok(resposta);
        }

        [HttpPost("primeiroacesso")]
        public IActionResult PrimeiroAcessoDados(PrimeiroAcessoRequisicao obj)
        {
            var resposta = _usuarioService.PrimeiroAcesso(obj);
            return Ok(resposta);
        }

        [HttpPost("primeiroacessodados")]
        public IActionResult PrimeiroAcessoDados(PrimeiroAcessoDadosRequisicao obj)
        {
            var resposta = _usuarioService.PrimeiroAcessodados(obj);
            return Ok(resposta);
        }

        [Authorize]
        [HttpGet("validartoken")]
        public IActionResult ValidarToken()
        {
            var identity = User.Identity as ClaimsIdentity;
            var id = int.Parse(identity?.Claims.First(x => x.Type == "id").Value);
            var user = _usuarioService.ListarUsuario(id);
            return Ok(user);
        }

        [HttpGet("jwt/{token}")]
        public IActionResult ValidarJWTToken(string token)
        {
            var jwt = this.CreateJwt();
            var ret = TokenService.ValidateJwtToken(token, jwt.key);
            return Ok(ret);
        }

        [HttpGet("nome/{token}")]
        public IActionResult NomeJWTToken(string token)
        {
            var jwt = this.CreateJwt();
            var ret = TokenService.NomeUsuario(token, jwt);
            return Ok(ret);
        }

        [HttpGet("userjwttoken/{token}")]
        public IActionResult userJwtToken(string token)
        {
            var jwt = this.CreateJwt();
            var ret = TokenService.UserInfoJwtToken(token, jwt);
            return Ok(ret);
        }

        [HttpPost("userjwttoken")]
        public IActionResult RetornaUserJwtToken(ValidarTokenRequisicao obj)
        {
            var jwt = this.CreateJwt();
            var ret = TokenService.UserInfoJwtToken(obj.token, jwt);
            return Ok(ret);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var jwt = this.CreateJwt();
            var users = _usuarioService.ListarTodos();
            return Ok(users);
        }

        [HttpGet("{idusuario}")]
        public IActionResult GetId(int idusuario)
        {
            var users = _usuarioService.ListarUsuario(idusuario);
            return Ok(users);
        }

        [HttpPost()]
        public IActionResult IncluirUsuario(IncluirUsuarioRequisicao obj)
        {
            var resposta = _usuarioService.Incluir(obj);
            return Ok(resposta);
        }        

        [HttpPost("unidades")]
        public IActionResult ListarUnidades(UnidadesUsuarioRequisicao obj)
        {
            var resposta = _usuarioService.ListarUnidades(obj);
            return Ok(resposta);
        }

        [Authorize]
        [HttpPut("{idusuario}")]
        public IActionResult Update()
        {
            return Ok("não implementado");
        }

        [HttpGet("{cpf}/dados")]
        public IActionResult GetDadosUsuario(string cpf)
        {
            var usuario = _usuarioService.RetornaDadosUsuario(cpf);
            return Ok(usuario);
        }

        [HttpGet("mensagens/{cpf}")]
        public IActionResult MensagensUsuario(string cpf)
        {
            try
            {
                var resposta = _usuarioService.ExistemMensagensParaUsuario(cpf);
                return Ok(resposta);
            }
            catch (Exception ex)
            {
                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.erro, $"mensagens: {cpf}");
                var msgError = new HttpErrorMessage(417, ex.Message);
                return BadRequest(msgError);
            }
        }

        [HttpGet("notificacoes/{cpf}")]
        public async Task<IActionResult> NotificacoesUsuarioAsync(string cpf)
        {
            _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.info,"Notificações " + cpf);
            var resposta = await _usuarioService.ExistemNotificacoesParaUsuario(cpf);
            return Ok(resposta);
        }


        [HttpGet("contratos/{codigoclientesp7}")]
        public async Task<IActionResult> ContratosUsuarioAsync(string codigoclientesp7)
        {
            try
            {
                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.info, "contratos " + codigoclientesp7);
                var resposta = await _usuarioService.ListarContratosCliente(codigoclientesp7);
                return Ok(resposta);
            }
            catch (Exception ex)
            {
                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.erro, $"contratos: {codigoclientesp7}");
                var msgError = new HttpErrorMessage(417, ex.Message);
                return BadRequest(msgError);
            }
        }

        [HttpGet("integrar/{cpf}")]
        public async Task<IActionResult> IntegrarUsuarioAsync(string cpf)
        {
            try
            {
                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.info, "cpf cliente " + cpf);
                var resposta = _usuarioService.Integrar(cpf);
                return Ok(resposta);
            }
            catch (Exception ex)
            {
                _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.erro, $"{ex.Message} cpf cliente {cpf}");
                var msgError = new HttpErrorMessage(417, ex.Message);
                return BadRequest(msgError);
            }
        }

        [HttpPost("salvarpagina")]
        public async Task<IActionResult>Pagina(SalvarPaginaRequisicao obj)
        {
            var parametro = ParametrosService.consultar(connectionStringSQL);

            string nomeArquivo = GeraNomeArquivoHtml() + ".html";

            string cssStr = string.Empty;

            string conteudoHtml = "<!DOCTYPE html>"+
                                    "<html> "+  
                                    "<head> "+    
                                    "<meta charset=\"utf-8\" /> "+    
                                    "<meta name=\"viewport\" content=\"width=device-width\" /> "+
                                    "<title>Minha página de teste</title> "+  
                                    "<style type=\"text/css\"> " +
                                    parametro.cssHTML +
                                    "</style> " +
                                    "</head> "+  
                                    "<body> "+
                                    obj.htmlText +  
                                    "</body> "+
                                    "<script> " +
                                    "function copyToClipboard(text) { " +
                                    "const el = document.createElement('textarea'); " +
                                    "el.value = text; " +
                                    "el.setAttribute('readonly', ''); " +
                                    "el.style.position = 'absolute'; " +
                                    "el.style.left = '-9999px'; " +
                                    "document.body.appendChild(el); " +
                                    "el.select(); " +
                                    "document.execCommand('copy'); " +
                                    "document.body.removeChild(el); " +
                                    "} " +
                                    "</script> " +
                                    "</html>";

            string caminhoArquivo = parametro.caminhoHTML + @"\notificacao"; 

            if (!Directory.Exists(caminhoArquivo))
            {
                Directory.CreateDirectory(caminhoArquivo);
            }
           
            using (StreamWriter file = new StreamWriter(Path.Combine(caminhoArquivo, nomeArquivo)))
            {
                await file.WriteAsync(conteudoHtml);
            }

            var ret = new SalvarPaginaResposta();
            ret.urlpagina = $"{parametro.urlExternaHTML}/notificacao/{nomeArquivo}";
            _logService.Log(repositorios.LogRepository.OrigemLog.api, repositorios.LogRepository.TipoLog.info,$"url página html {parametro.urlExternaHTML}/notificacao/{nomeArquivo}");

            return Ok(ret);
        }

        private string GeraNomeArquivoHtml()
        {
            const string caracteresPermitidos = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            Random random = new Random();
            StringBuilder codigoAleatorio = new StringBuilder();

            for (int i = 0; i < 12; i++)
            {
                int indiceCaractere = random.Next(caracteresPermitidos.Length);
                char caractereAleatorio = caracteresPermitidos[indiceCaractere];
                codigoAleatorio.Append(caractereAleatorio);
            }
            return codigoAleatorio.ToString();
        }

        private Jwt CreateJwt()
        {
            var jwt = new Jwt();
            jwt.Subject = "baseWebApiSubject";
            jwt.Issuer = "basewebApiIssuer";
            jwt.Audience = "baseWebApiAudience";
            jwt.key = "**poliview.tecnologia.crm@2022**";
			// var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
			return jwt;
        }


    }
}
