using Microsoft.AspNetCore.Http;

namespace Poliview.crm.services
{
    public interface IUploadService
    {
        public string SalvarArquivos(IFormFileCollection files);
    }
}
