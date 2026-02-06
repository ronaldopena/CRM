using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class RelatorioCienciaArquivo
    {
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
        public string? origemultimoacesso { get; set; }
        public string datahoraultimoacesso { get; set; }
        public string dataultimoacesso { get; set; }
        public string horaultimoacesso { get; set; }
        public int idarquivodownload { get; set; }
        public string? descricaoarquivo { get; set; }
        public string? datainicialfiltro { get; set; }
        public string? datahoradownload { get; set; }
        public string? datadownload { get; set; }
        public string? horadownload { get; set; }
        public string? origemdownload { get; set; }
    }
}
