using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class validarSenhaRequisicao
    {
        public int idusuario { get; set; }
        public string senha { get; set; }

        public validarSenhaRequisicao()
        {
            if (this.senha == null)
            {
                this.senha = "123";
            }
        }
    }

}
