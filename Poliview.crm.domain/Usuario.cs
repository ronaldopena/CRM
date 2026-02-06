using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class UsuarioBase
    {
        public int CD_USUARIO { get; set; }
        public string? NM_USUARIO { get; set; }
        public string? DS_EMAIL { get; set; }
        public string? DS_SENHA { get; set; }
        public string? IN_BLOQUEADO { get; set; }
        public string? IN_STATUS { get; set; }
        public string? NR_CPFCNPJ { get; set; }
        public string? IN_USUARIOSISTEMA { get; set; }
        public string? IN_CLIENTE { get; set; }
        public string? CD_CLIENTESP7 { get; set; }
        public int CD_CLIENTE { get; set; }
        public string? Data_Nascimento { get; set; }
        public int idempresa { get; set; }
        public int cadastramentodireto { get; set; }
    }

    public class Usuario : UsuarioBase
    {
        public virtual Boolean acessopadrao { get; set; }
        public virtual string? token { get; set; }
        public virtual int cadastramentodireto { get; set; }
        public string? versaoapp { get; set; }
        public string? versaoportal { get; set; }
        public string? versaoappandroid { get; set; }

    }

    public class dadosUsuario
    {
        public string? nome { get; set; }
        public string? email { get; set; }
        public string? cpf { get; set; }
        public string? codigoclientesp7 { get; set; }
        public string? celular { get; set; }
        public string? endereco { get; set; }
        public string? bairro { get; set; }
        public string? cidade { get; set; }
        public string? estado { get; set; }
        public string? cep { get; set; }
        public int idempresa { get; set; }

    }

    public class usuariosGrupo
    {
        public int idusuario { get; set; }
        public string? nome { get; set; }
        public string? email { get; set; }
        public int idgrupo { get; set; }
        public string? grupo { get; set; }
        public int master { get; set; }
        public int supervisor { get; set; }
    }
}
