namespace IntegracaoMobussService.Services
{
    public static class ServiceConexao
    {
        public class Conexao
        {

            public Conexao(IConfiguration config)
            {
                StringConexao = config["ConexaoBanco"] ?? string.Empty;
            }

            public string StringConexao { get; set; } = string.Empty;

        }
    }
}
