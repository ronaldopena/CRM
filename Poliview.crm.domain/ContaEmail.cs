using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class ContaEmail
    {
        
        public int id { get; set; } = 0;
        public int tipoconta { get; set; } = 0;
        public string descricaoConta { get; set; } = string.Empty;
        public string nomeRemetente { get; set; } = string.Empty;
        public string emailRemetente { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string senha { get; set; } = string.Empty;
        public string hostpop { get; set; } = string.Empty;
        public Boolean sslpop { get; set; } = true;
        public int portapop { get; set; } = 995;
        public string hostsmtp { get; set; } = string.Empty;
        public Boolean sslsmtp { get; set; } = true;
        public int portasmtp { get; set; } = 587;
        public string tenant_id { get; set; } = string.Empty;
        public string client_id { get; set; } = string.Empty;
        public string clientSecret { get; set; } = string.Empty;
        public string refresh_token { get; set; }
        public string userId { get; set; } = string.Empty;  
        public Boolean enviaremail { get; set; } = true;
        public Boolean receberemail { get; set; } = true;
        public int intervalorecebimento { get; set; } = 1;
        public int intervaloenvio { get; set; } = 1;
        public int qtdeemailsrecebimento { get; set; } = 20;
        public int qtdeemailsenvio { get; set; } = 30;
        public int qtdetentativasenvio { get; set; } = 3;
        public int tamanhomaximoanexos { get; set; } = 10;
        public string emaildestinatariosuporte { get; set; } = string.Empty;
        public int ativo { get; set; } = 1;
        public string refreshtoken { get; set; }
        public int EmailsPorDia { get; set; }
        public int EmailsMalaDiretaPorDia { get; set; }
        public int SocketOptions { get; set; } = 0;
    }
}
