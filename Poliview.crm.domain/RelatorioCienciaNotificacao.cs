using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class RelatorioCienciaNotificacao
    {
        public string? notificacao { get; set; }
        public string? datahoraleitura { get; set; }
        public string? dataleitura { get; set; }
        public string? horaleitura { get; set; }
        public int idorigemleitura { get; set; }
        public string? origemleitura { get; set; }
        public string? cliente { get; set; }
        public string? cpfcnpj { get; set; }
        public string? email { get; set; }
        public string? primeiroacesso { get; set; }
        public string? telefone { get; set; }
        public int idempreendimento { get; set; }
        public string? empreendimento { get; set; }
        public int idbloco { get; set; }
        public string? bloco { get; set; }
        public int idunidade { get; set; }
        public string? unidade { get; set; }
        public int idcontrato { get; set; }
        public string? contrato { get; set; }        
        public string? origemacesso { get; set; }
        public string? datahoraultimoacesso { get; set; }
        public string? dataultimoacesso { get; set; }
        public string? horaultimoacesso { get; set; }

    }
}
