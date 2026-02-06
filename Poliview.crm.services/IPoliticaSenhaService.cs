using Poliview.crm.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.services
{
    public interface IPoliticaSenhaService
    {
        public string[] validar(int idusuario, string senha);
    }
}
