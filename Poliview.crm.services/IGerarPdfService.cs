using Poliview.crm.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.services
{
    public interface IGerarPdfService
    {
        public GerarPdfResposta listar(GerarPdfRequisicao pdf);
        public void download(string arquivo);
    }
}
