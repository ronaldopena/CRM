using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class ChamadoHistorioEmail
    {
        public int Id { get; set; }
        public string? Data { get; set; }
        public string? Remetente { get; set; }
        public string? Destinatario { get; set; }
        public string? Assunto { get; set; }
        public string? Corpo { get; set; }
    }
}
