using ClosedXML.Excel;
using System.Data;

namespace Poliview.crm.infra
{
    public static class ExportarExcel
    {
        public static string Exportar(DataTable dt, string caminhoXls, string urlExterna, string nomeArquivo)
        {
            try
            {
                var urlxls = urlExterna + @"/pdf/" + nomeArquivo + ".xlsx";
                var pathxls = caminhoXls + @"\" + nomeArquivo + ".xlsx";
                var urlDownload = urlExterna + @"/apicrm/arquivodownload/downloadfile?fileUrl=" + urlxls;
                XLWorkbook wb = new XLWorkbook();
                wb.Worksheets.Add(dt, "dados");
                wb.SaveAs(pathxls);
                return urlDownload;
            }
            catch(Exception ex) 
            {
                return "Erro: " + ex.Message;
            }
        }

    }
}
