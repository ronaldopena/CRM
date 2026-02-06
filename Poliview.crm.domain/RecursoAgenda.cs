namespace Poliview.crm.domain
{
    public class RecursoAgenda
    {
        public int idrecurso { get; set; }
        public int idgrupo { get; set; }
        public string dataagenda { get; set; }
        public string horaini { get; set; }
        public string horafim { get; set; }
        public int idordemservico { get; set; }
        public int indisponivel { get; set; }
        public string observacao { get; set; }
    }
}