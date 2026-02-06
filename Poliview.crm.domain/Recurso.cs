using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Recurso
	{
        public int CD_Usuario { get; set; }
        public string? NM_Usuario { get; set; }
        public string? DS_Email { get; set; }
        public int CD_Grupo { get; set; }
        public string? NM_Grupo { get; set; }
        public string? IN_Master { get; set; }        
    }
}
