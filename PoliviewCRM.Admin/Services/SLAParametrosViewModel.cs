using System.Text.Json.Serialization;

namespace PoliviewCRM.Admin.Services
{
    public class SLAParametrosViewModel
    {
        [JsonPropertyName("NR_SLAAlerta")]
        public int? NR_SLAAlerta { get; set; }

        [JsonPropertyName("NR_SLACritico")]
        public int? NR_SLACritico { get; set; }

        [JsonPropertyName("horasUteisCalcSLA")]
        public bool horasUteisCalcSLA { get; set; }
    }
}
