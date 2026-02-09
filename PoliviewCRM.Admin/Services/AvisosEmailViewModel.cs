using System.Text.Json.Serialization;

namespace PoliviewCRM.Admin.Services
{
    public class AvisosEmailViewModel
    {
        [JsonPropertyName("tamanhoMaximoAnexos")]
        public int? TamanhoMaximoAnexos { get; set; }

        [JsonPropertyName("emailErrosAdmin")]
        public string? emailErrosAdmin { get; set; }

        [JsonPropertyName("DiasLembrarPesquisaSatisfacao")]
        public int? DiasLembrarPesquisaSatisfacao { get; set; }

        [JsonPropertyName("qtdeAvisosLembrarPesquisa")]
        public int? qtdeAvisosLembrarPesquisa { get; set; }

        [JsonPropertyName("documentoChamadoConcluido")]
        public int? documentoChamadoConcluido { get; set; }
    }
}
