using Newtonsoft.Json;

namespace Poliview.crm.domain
{
    public class ConfigInstaladorCRM
    {
        public string ServidorSqlServer { get; set; }
        public string InstanciaSqlServer { get; set; }
        public string BancoSqlServer { get; set; }
        public int PortaSqlServer { get; set; }
        public string UsuarioSqlServer { get; set; }
        public string SenhaSqlServer { get; set; }
        public string TimeoutSqlServer { get; set; }
        public Boolean ServicoAPI { get; set; }
        public Boolean ServicoEMAIL { get; set; }
        public Boolean ServicoINTEGRACAO { get; set; }
        public Boolean ServicoSLA { get; set; }
        public Boolean ServicoMONITORAMENTO { get; set; }
        public Boolean ServicoAPIMOBUSS { get; set; }
        public Boolean ServicoINTEGRACAOMOBUSS { get; set; }
        public int PortaApiCRM { get; set; }
        public int PortaApiMOBUSS { get; set; }
        public string ApiSIECON { get; set; }
        public string AcessoExterno { get; set; }
        public string ServidorSIECON { get; set; }
        public string BancoSIECON { get; set; }
        public int PortaSIECON { get; set; }
        public string UsuarioSIECON { get; set; }
        public string SenhaSIECON { get; set; }
        public string nomecliente { get; set; }
        public string PastaInstalacaoCRM { get; set; }
        public string PastaInstalacaoSIECON { get; set; }
        public void Salvar(string dir)
        {

            var obj = JsonConvert.SerializeObject(this);
            // var arq = dir + "\\instalador.json";
            var arq = "instalador.json";

            File.WriteAllText(arq, obj);
        }

        public ConfigInstaladorCRM()
        {
            this.TimeoutSqlServer = "120";
        }

    }
}
