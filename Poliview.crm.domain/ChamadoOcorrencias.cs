using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class ChamadoOcorrencias
    {
        public int idchamado { get; set; }
        public int idocorrencia { get; set; }
        public string? abertura { get; set; }
        public string? ocorrencia { get; set; }
        public string? descricao { get; set; }
    }
}
