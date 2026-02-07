namespace Poliview.crm.domain
{
    public class Jornada
    {
        public int ID_Jornada { get; set; }
        public string NM_Jornada { get; set; } = string.Empty;
        public string IN_Tipo { get; set; } = string.Empty;
        public string IN_Status { get; set; } = string.Empty;
        public DateTime DT_Controle { get; set; }
    }
}
