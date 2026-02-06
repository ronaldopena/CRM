using Poliview.crm.domain;

namespace Poliview.crm.models
{   
    public class AgendaResposta: IRetorno
    {
        public ListaAgenda? objeto { get; set; }
    }
    public class ListaAgenda
    {
        public List<DatasAgenda>? datas { get; set; }
        public List<RecursoDatasAgenda>? recursos { get; set; }
    }
    public class DatasAgenda
    {
        public string? data { get; set; }
        public string? diasemana { get; set; }
    }
    public class RecursoDatasAgenda
    {
        public string? nome { get; set; }
        public int codigo { get; set; }
        public List<HorarioDataAgenda>? dia1 { get; set; }
        public List<HorarioDataAgenda>? dia2 { get; set; }
        public List<HorarioDataAgenda>? dia3 { get; set; }
        public List<HorarioDataAgenda>? dia4 { get; set; }
        public List<HorarioDataAgenda>? dia5 { get; set; }
        public List<HorarioDataAgenda>? dia6 { get; set; }
        public List<HorarioDataAgenda>? dia7 { get; set; }
    }

    public class HorarioDataAgenda
    {
        //public string? data { get; set; }
        //public string? hora { get; set; }
        public int reservado { get; set; }
        public string? ordemservico { get; set; }
    }
}
