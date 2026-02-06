
namespace Poliview.crm.domain
{    
    public class GrupoMidia
    {
        public int id { get; set; }
        public string? descricao { get; set; }
        public string? linkimagem { get; set; }
        public string? grupo { get; set; }
    }

    public class GrupoMidiaArquivo
    {
        public int idgrupo { get; set; }
        public string? grupo { get; set; }
        public string? linkimagemgrupo { get; set; }
        public int qtde { get; set; }
        public string? data { get; set; }
    }

    public class GrupoMidiaArquivoDetalhe
    {
        public int id { get; set; }
        public string? descricao { get; set; }
        public int idgrupo { get; set; }
        public string? grupo { get; set; }
        public string? linkdownload { get; set; }

        public int lida { get; set; }
    }

    public class GrupoMidiaMensagem
    {
        public int idgrupo { get; set; }
        public string? grupo { get; set; }
        public string? linkimagemgrupo { get; set; }
        public int qtde { get; set; }
        public string? data { get; set; }
        
    }

    public class GrupoMidiaMensagemDetalhe
    {
        public int id { get; set; }
        public string? descricao { get; set; }
        public int idgrupo { get; set; }
        public string? grupo { get; set; }
        public string? urlimagem { get; set; }
        public string? linkimagem { get; set; }
        public string? linkimagemgrupo { get; set; }
        public int lida { get; set; }
    }
}