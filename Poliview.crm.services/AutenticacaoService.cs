using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.infra;
using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface IAutenticacaoService
    {
        public AutenticacaoResposta Login(LoginRequisicao obj, Jwt jwt);
    }

    public class AutenticacaoService : IAutenticacaoService
    {

        private readonly string _connectionString;
        private IConfiguration _configuration;

        public AutenticacaoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }
        public AutenticacaoResposta Login(LoginRequisicao obj, Jwt jwt)
        {
            Usuario user = RetornaUsuarioPorEmailOuCpf(obj.usuario);
            var retorno = new AutenticacaoResposta();

            try
            {
                if (user != null)
                {
                    retorno.mensagem = "OK";
                    retorno.sucesso = true;
                    var senhacriptografada = Criptografia.Criptografar(obj.senha);
                    var ismatch = (senhacriptografada == user.DS_SENHA || user.acessopadrao);
                    retorno.objeto = user;

                    if (retorno.objeto.IN_BLOQUEADO == "S")
                    {
                        retorno.mensagem = "Usuário bloqueado!";
                        retorno.sucesso = false;
                        retorno.objeto = null;
                    }
                    else if (retorno.objeto.IN_STATUS == "I")
                    {
                        retorno.mensagem = "Usuário inativo!";
                        retorno.sucesso = false;
                        retorno.objeto = null;
                    }
                    else if (!ismatch)
                    {
                        retorno.objeto = null;
                        retorno.sucesso = false;
                        retorno.mensagem = "senha incorreta";
                    }
                    else
                    {
                        var token = TokenService.GenerateJwtToken(user.CD_USUARIO, user.NM_USUARIO, user.DS_EMAIL, user.NR_CPFCNPJ, jwt.Subject, jwt.Issuer, jwt.Audience, jwt.key);
                        retorno.objeto.token = token;
                        retorno.objeto.DS_SENHA = ""; // zera a senha guardada na base.
                    }
                }
                else
                {
                    retorno.objeto = null;
                    retorno.mensagem = "Usuário não encontrado";
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

        public bool Logout()
        {
            throw new NotImplementedException();
        }

        public bool UsuarioLogado()
        {
            throw new NotImplementedException();
        }

        private Usuario RetornaUsuarioPorEmailOuCpf(string emailOuCpf)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"SELECT CD_USUARIO, NM_USUARIO, DS_EMAIL, DS_SENHA, NR_CPFCNPJ, IN_BLOQUEADO, 
                         IN_STATUS, IN_USUARIOSISTEMA
                         FROM OPE_USUARIO 
                         WHERE DS_EMAIL = @EmailOuCpf OR NR_CPFCNPJ = @EmailOuCpf";

            var result = connection.QueryFirstOrDefault<Usuario>(query, new { EmailOuCpf = emailOuCpf });
            return result;
        }

    }
}
