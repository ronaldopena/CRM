using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using Poliview.crm.models;
using System.Text.RegularExpressions;

namespace Poliview.crm.services
{
    public class Parametros
    {
        public int senhaVencimentoDias { get; set; }
        public int senhaComprimento { get; set; }
        public int senhaMinimoMaiusculo { get; set; }
        public int senhaMinimoMinusculo { get; set; }
        public int senhaMinimoNumerico { get; set; }
        public int senhaMinimoAlfanumerico { get; set; }
        public int senhaTentativasLogin { get; set; }
        public int senhaCoincidir { get; set; }
    }

    public class PoliticaSenhaService : IPoliticaSenhaService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;
        private Parametros parametros;
        private string senha = "";
        private int idusuario;

        public PoliticaSenhaService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
            parametros = this.ParametrosDL();
        }

        public string[] validar(int idusuario, string senha)
        {
            this.senha = senha;
            this.idusuario = idusuario;

            var result = new List<string>();        
            adicionarMensagemErro(ref result, this.validarSenhaComprimento());
            adicionarMensagemErro(ref result, this.validarSenhaMinimoMaiusculo());
            adicionarMensagemErro(ref result, this.validarSenhaMinimoMinusculo());
            adicionarMensagemErro(ref result, this.validarSenhaMinimoNumerico());
            adicionarMensagemErro(ref result, this.validarSenhaMinimoAlfanumerico());
            adicionarMensagemErro(ref result, this.validarSenhaTentativasLogin());
            adicionarMensagemErro(ref result, this.validarSenhaVencimentoDias());
            adicionarMensagemErro(ref result, this.validarPrecisaTrocaSenha());
            adicionarMensagemErro(ref result, this.validarRepetiuSenhaAnterior());          
            return result.ToArray();
        }

        private void adicionarMensagemErro(ref List<string> mensagens, string mensagem)
        {
            if ((mensagem.Trim() != ""))
            {
                mensagens.Add(mensagem);
            }
        }

        private string validarSenhaComprimento()
        {
            if (this.parametros.senhaComprimento > 0)
            {
                if (this.senha.Length < this.parametros.senhaComprimento)
                {
                    return "A senha deve ter pelo menos " + this.parametros.senhaComprimento.ToString() + " caracteres. ";
                }
            }
            return "";
        }
        private string validarSenhaMinimoMaiusculo()
        {
            var count = Regex.Matches(this.senha, "[A-Z]").Count;

            if (count < this.parametros.senhaMinimoMaiusculo)
            {
                return "A senha deve ter pelo menos " + this.parametros.senhaMinimoMaiusculo.ToString() + " letras maiúsculas. ";
            }

            return "";
        }
        private string validarSenhaMinimoMinusculo()
        {
            var count = Regex.Matches(this.senha, "[a-z]").Count;

            if (count < this.parametros.senhaMinimoMinusculo)
            {
                return "A senha deve ter pelo menos " + this.parametros.senhaMinimoMinusculo.ToString() + " letras minúsculas. ";
            }

            return "";
        }
        private string validarSenhaMinimoNumerico()
        {
            var count = Regex.Matches(this.senha, "[0-9]").Count;

            if (count < this.parametros.senhaMinimoNumerico)
            {
                return "A senha deve ter pelo menos " + this.parametros.senhaMinimoNumerico.ToString() + " números. ";
            }

            return "";
        }
        private string validarSenhaMinimoAlfanumerico()
        {
            var count1 = Regex.Matches(this.senha, "[a-z]").Count;
            var count2 = Regex.Matches(this.senha, "[A-Z]").Count;
            var count = count1 + count2;

            if (count < this.parametros.senhaMinimoAlfanumerico)
            {
                return "A senha deve ter pelo menos " + this.parametros.senhaMinimoAlfanumerico.ToString() + " caracteres alfanumericos. ";
            }

            return "";
        }
        private string validarSenhaTentativasLogin()
        {
            return "";
        }        
        private string validarSenhaVencimentoDias()
        {
            return "";
        }
        public string validarPrecisaTrocaSenha()
        {
            if (PrecisaTrocaSenhaDL(this.idusuario) == 1)
            {
                return "´Necessário a troca de senha";
            }

            return "";
        }
        public string validarRepetiuSenhaAnterior()
        {
            if (this.parametros.senhaCoincidir > 0)
            {
                var result = RepetiuSenhaAnteriorDL(this.idusuario, this.senha);
                if (result == 1)
                {
                    return "Essa senha já foi usada anteriormente. Escolha uma nova senha";
                }
                else
                {
                    return "";
                }
            }
            return "";
        }
        public Boolean SalvarSenha()
        {
            return (this.SalvarSenhaDL(this.idusuario, this.senha) == 1);
        }

        private int PrecisaTrocaSenhaDL(int codigousuario)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $"exec CRM_PrecisaTrocarSenha @codigousuario = {codigousuario} ";
            var result = connection.QueryFirst<int>(sql);
            return result;
        }

        private int RepetiuSenhaAnteriorDL(int codigousuario, string novasenha)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = string.Format("exec CRM_RepetiuSenhaAnterior @codigousuario = {0}, @novasenha = '{1}' ", codigousuario, novasenha);
            var result = connection.QueryFirst<int>(sql);
            return result;            
        }

        private int SalvarSenhaDL(int codigousuario, string senha)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = string.Format("insert into OPE_USUARIO_SENHA (CD_USUARIO, DS_SENHA) VALUES ({0},'{1}') ", codigousuario, senha);
            var result = connection.QueryFirst<int>(sql);
            return result;
        }

        private Parametros ParametrosDL()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = string.Format("select top 1 senhaVencimentoDias, senhaComprimento, senhaMinimoMaiusculo, senhaMinimoMinusculo, senhaMinimoNumerico, senhaMinimoAlfanumerico, senhaTentativasLogin, senhaCoincidir from ope_parametro where cd_bancodados=1 and cd_mandante=1 ");
            var result = connection.QueryFirst<Parametros>(sql);
            return result;
        }

    }
}
