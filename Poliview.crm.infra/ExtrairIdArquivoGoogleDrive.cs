
namespace Poliview.crm.infra
{
    public static class ExtrairIdArquivoGoogleDrive
    {
        public static string get(string url)
        {
            var urlAux = url;
            urlAux = urlAux.Replace("https://drive.google.com/file/d/", "");
            urlAux = urlAux.Replace("https://drive.google.com/u/0/uc?id=", "");
            urlAux = urlAux.Replace(@"/view?usp=drive_link", "");
            urlAux = urlAux.Replace("?usp=drive_link", "");
            return urlAux;
        }
    }
}


/*
 update CAD_ARQUIVO_DOWNLOAD set link=replace(link,'https://drive.google.com/file/d/','https://drive.google.com/u/0/uc?id=')
update CAD_ARQUIVO_DOWNLOAD set link=replace(link,'/view?usp=drive_link','')
update CAD_ARQUIVO_DOWNLOAD set link=replace(link,'?usp=drive_link','')
update CAD_ARQUIVO_DOWNLOAD set link=replace(link,'https://drive.google.com/drive/folders/','https://drive.google.com/u/0/uc?id=')
*/