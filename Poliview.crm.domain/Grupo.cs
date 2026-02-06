
namespace Poliview.crm.domain
{    
    public class Grupo
    {
        public int CD_GRUPO { get; set; }
        public string? NM_GRUPO { get; set; }
        public int IN_Nivel { get; set; }
        public string? IN_Status { get; set; }
        public int VisualizarChamados { get; set; }
        public int VisualizarBoletos { get; set; }
        public int VisualizarInformeRendimentos { get; set; }
    }
}