using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class LoginResposta
	{
        public int CD_USUARIO { get; set; }
        public string? CD_ClienteSP7 { get; set; }
        public int CD_Cliente { get; set; }
        public string? NM_USUARIO { get; set; }
        public string? DS_EMAIL { get; set; }
        public string? DS_SENHA { get; set; }
        public string? IN_BLOQUEADO { get; set; }
        public string? IN_STATUS { get; set; }
        public string? NR_CPFCNPJ { get; set; }
        public string? IN_USUARIOSISTEMA { get; set; }
        public string? IN_CLIENTE { get; set; }
        public string? Data_Nascimento { get; set; }
        public string? token { get; set; }
        public int idempresa { get; set; }
        public bool acessopadrao { get; set; }
        public int mensagens { get; set; }
        public string? origem { get; set; }
        public bool success { get; set; }
        public int cadastramentodireto { get; set; }   
        public string versaoapp { get; set; } = string.Empty;
        public string versaoportal { get; set; } = string.Empty;
        public string versaoappandroid { get; set; } = string.Empty;
        public int NovasMensagens { get; set; }
    }

    public class LoginRespostaApp
    {
        public bool success { get; set; }
        public string? errorMessage { get; set; }
    }
}