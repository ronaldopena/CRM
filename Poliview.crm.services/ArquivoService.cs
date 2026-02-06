using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Serilog;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface IArquivoService
    {
        public Arquivo GetArquivo(int id);
        public int Salvar(Arquivo arquivo, ILogger log, ref string msg);
    }

    public class ArquivoService : IArquivoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;
        private bool verQuery = true;
        private bool verDebug = true;
        private bool verErros = true;
        private bool verInfos = true;

        public ArquivoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
            verQuery = Convert.ToBoolean(configuration["verQuery"]);
            verDebug = Convert.ToBoolean(configuration["verDebug"]);
            verErros = Convert.ToBoolean(configuration["verErros"]);
            verInfos = Convert.ToBoolean(configuration["verInfos"]);
        }

        public Arquivo GetArquivo(int id)
        {
            throw new NotImplementedException();
        }

        public int Salvar(Arquivo arquivo, ILogger log, ref string msg)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "";
            query += " INSERT INTO OPE_ARQUIVOS (";
            query += " IN_PADRAO, NM_ARQUIVO, DS_EXTENSAO, DT_CONTROLE, CD_USUARIO, IN_CHAMADO, BANCO)";
            query += " VALUES (";
            query += " @IN_Padrao, @NM_Arquivo, @DS_Extensao, @DT_Controle, @CD_Usuario, @IN_Chamado, @BANCO); SELECT @@IDENTITY;";
            if (verQuery) log.Debug(query);
            
            var nomebanco = RetornaNomeBanco();
            var numerobanco = RetornaNumeroBanco();

            var parameters = new
            {
                IN_Padrao = "N",
                NM_Arquivo = arquivo.arquivo,
                DS_Extensao = arquivo.extensao,
                DT_Controle = arquivo.data,
                CD_Usuario = arquivo.idusuario,
                IN_Chamado = arquivo.chamado,
                BANCO = numerobanco
            };

            msg = query;
            var idarquivo = connection.QueryFirst<int>(query, parameters);                     

            var ds_arquivo = "0x" + Convert.ToHexString(arquivo.conteudo);
            var ds_arquivo2 = Convert.ToHexString(arquivo.conteudo);
            var ds_arquivo3 = arquivo.conteudo.ToString();
            
            query = "";
            if (numerobanco == "0")
                query += $" INSERT INTO {nomebanco}_Arquivos.dbo.OPE_ARQUIVOS (ID, DS_Arquivo)";
            else
                query += $" INSERT INTO {nomebanco}_Arquivos{numerobanco}.dbo.OPE_ARQUIVOS (ID, DS_Arquivo)";
            query += $" VALUES (@id, @conteudo)";

            var parameters2 = new
            {
                id = idarquivo,
                conteudo = arquivo.conteudo
            };
                        
            msg += query;

            if (verQuery) log.Debug(query);

            connection.Query(query, parameters2);

            return idarquivo;
        }

        public void SalvarAnexoChamado(int idchamado, int idocorrencia, int idanexo, string nomeanexo, ILogger log, ref string msg)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "";
                query += " INSERT INTO OPE_CHAMADO_ANEXO ";
                query += " (CD_BancoDados, CD_Mandante, CD_Chamado, CD_Ocorrencia, CD_Anexo, NM_Anexo, DS_ANEXO) ";
                query += " VALUES ";
                query += $" (1, 1, {idchamado}, {idocorrencia}, {idanexo}, '{nomeanexo}', 'enviado por email') ";
                if (verQuery) log.Debug(query);
                msg = query;
                connection.Query(query);
            }
            catch (Exception ex)
            {
                log.Error("erro: " + ex.Message + " " + nomeanexo.Length.ToString());
                throw;
            }
        }

        public void SalvarAnexoEmail(int idemail, int idarquivo, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "";
            query += "INSERT INTO [dbo].[OPE_EMAIL_ARQUIVOS] ";
            query += "([ID_Email],[ID_Arquivo],[DT_Controle],[CD_Aviso]) ";
            query += "VALUES ";
            query += $"({idemail},{idarquivo},GetDate(),null) ";
            if (verQuery) log.Debug(query);
            connection.Query(query);
        }

        public List<int> AnexosDeUmEmail(int idemail, ILogger log)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "";            
            query += $"select ID_Arquivo as idarquivo from OPE_EMAIL_ARQUIVOS e ";
            query += $"left join OPE_ARQUIVOS a on a.ID = e.ID_Arquivo ";
            query += $"where a.NM_Arquivo <> 'image001.png' and a.NM_Arquivo <> 'image001.jpg' and ID_Email = {idemail}";
            if (verQuery) log.Debug(query);
            return connection.Query<int>(query).ToList();
        }

        private string RetornaNomeBanco()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT DB_NAME()";
            Console.WriteLine(query);
            return connection.QueryFirst<string>(query);
        }

        private string RetornaNumeroBanco()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select cast(coalesce(bancoarquivos,0) as varchar(10)) from OPE_PARAMETRO where CD_BancoDados=1 and CD_Mandante=1";
            Console.WriteLine(query);
            return connection.QueryFirst<string>(query);
        }

        /*
        public void SaveAttachmentToLocalDisk(string server, int port, bool useSsl, string username, string password, int messageNumber, int attachmentIndex, string saveFilePath)
        {
            using (Pop3Client client = new Pop3Client())
            {
                client.Connect(server, port, useSsl);
                client.Authenticate(username, password);

                Message message = client.GetMessage(messageNumber);
                MessagePart attachment = message.FindAllAttachments()[attachmentIndex];

                using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create))
                {
                    byte[] attachmentData = attachment.Body;
                    fileStream.Write(attachmentData, 0, attachmentData.Length);
                }

                client.Disconnect();
            }
         }
        */


    }

}
