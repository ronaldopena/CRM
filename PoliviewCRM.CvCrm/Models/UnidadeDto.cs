namespace PoliviewCrm.CvCrm.Models;

using System.Text.Json.Serialization;

public class UnidadeDto
{
    [JsonPropertyName("idunidade")]
    public int IdUnidade { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }

    [JsonPropertyName("empreendimento")]
    public Empreendimento Empreendimento { get; set; }

    [JsonPropertyName("bloco")]
    public Bloco Bloco { get; set; }
}

public class Empreendimento
{
    [JsonPropertyName("idempreendimento")]
    public int IdEmpreendimento { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }
}

public class Bloco
{
    [JsonPropertyName("idbloco")]
    public int IdBloco { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; }
}