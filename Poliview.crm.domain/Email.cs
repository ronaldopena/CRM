using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Email
    {
        public int id { get; set; }
        public DateTime datainclusao { get; set; }
        public string nomeremetente { get; set; }
        public string emailremetente { get; set; }
        public string emaildestinatario { get; set; }
        public string assunto { get; set; }
        public string corpo { get; set; }
        public int corpohtml { get; set; }
        public int idstatusenvio { get; set; }
        public DateTime dataenvio { get; set; }
        public int entregue { get; set; }
        public int idchamado { get; set; }
        public int idocorrencia { get; set; }
        public string urlanexo { get; set; }
        public int tentativasenvio { get; set; }
        public string erroenvio { get; set; }
        public int processado { get; set; }
        public string tipoemail { get; set; }
        public int iddocumento { get; set; }
        public int prioridade { get; set; }
        public int idaviso { get; set; }
        public int idemailorigem { get; set; }
        public int idcontaemail { get; set; }
        public int classificacaoemail { get; set; }
    }

    public class EmailAnexo
    {
        public int idarquivo { get; set; }
        public byte[]? arquivo { get; set; }
        public string? nome { get; set; }
        public string? extensao { get; set; }
    }

}
