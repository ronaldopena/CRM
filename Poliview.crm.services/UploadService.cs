using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Net.Http.Headers;

namespace Poliview.crm.services
{
    public class UploadService : IUploadService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public UploadService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public string SalvarArquivos(IFormFileCollection files)
        {
            var retorno = "";

            foreach (var file in files)
            {
                // var folderName = Path.Combine("Resources", "Images");
                // var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                Console.WriteLine(file.FileName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    // var fullPath = Path.Combine(pathToSave, fileName);
                    // var dbPath = Path.Combine(folderName, fileName);

                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        string s = Convert.ToHexString(fileBytes);
                        var idarquivo = salvarArquivoBanco(Path.GetFileNameWithoutExtension(file.FileName), Path.GetExtension(file.FileName), s);
                        if (retorno == "")
                        {
                            retorno += idarquivo.ToString();
                        }
                        else
                        {
                            retorno += "," + idarquivo.ToString();
                        }                        
                    }
                }
                else
                {
                    retorno = "";
                    Console.WriteLine("Não foi enviado nenhum arquivo");
                }

            }
            Console.WriteLine(retorno);
            return retorno.Replace("\0", string.Empty);
        }

        private int salvarArquivoBanco(string nome, string extensao, string buffer)
        {
            using var connection = new SqlConnection(_connectionString);
            var ds_arquivo = "0x" + buffer;
            var query = $"EXEC CRM_INCLUIR_ANEXO @nome = '{nome}', @extensao = '{extensao}' , @arquivo = '{ds_arquivo}' ";
            var result = connection.QueryFirst<int>(query);
            return result;
        }
    }
}
