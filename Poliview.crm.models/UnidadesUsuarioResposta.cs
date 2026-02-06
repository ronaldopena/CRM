using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class UnidadesUsuarioResposta
    {
        public string? contrato { get; set; }
        public string? nomecliente { get; set; }
        public int idcliente { get; set; }
        public string? idclientesp7 { get; set; }
        public string? empreendimento { get; set; }
        public int idempreendimento { get; set; }
        public string? bloco { get; set; }
        public int idbloco { get; set; }
        public string? unidade { get; set; }
        public string? idunidade { get; set; }
        public bool chamado { get; set; }
        public bool boleto { get; set; }
        public bool fichafinanceira { get; set; }
        public bool informerendimento { get; set; }
        public string? empreendimentosp7 { get; set; }
        public string? blocosp7 { get; set; }
        public string? unidadesp7 { get; set; }
        public string? email { get; set; }
    }
}
