using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracaoMobussService.dominio
{
    public class ChamadoMobuss
    {
        public string? cpfCnpjCliente { get; set; }
        public string? dataAberturaSolicitacao { get; set; }
        public string? desSolicitacao { get; set; }
        public string? emailResponsavel { get; set; }
        public string? emailSolicitante { get; set; }
        public string? idLegado { get; set; }
        public string? idLocal { get; set; }
        public string? nomeSolicitante { get; set; }
        public int numSolicitacao { get; set; }
        public string? telefoneSolicitante { get; set; }

    }
}

