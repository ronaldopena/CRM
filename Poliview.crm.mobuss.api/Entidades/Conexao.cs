using Microsoft.Extensions.Configuration;

namespace apimobuss.Entidades
{
    public class Conexao
    {

        public Conexao(IConfiguration config)
        {
            StringConexao = config.GetValue<string>("ConexaoBanco") ?? string.Empty;
        }

        public string StringConexao { get; set; } = string.Empty;

    }
}
