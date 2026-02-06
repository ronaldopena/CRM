using System.Data;
using System.Diagnostics;
using System.ServiceProcess;

namespace poliview.crm.instalador
{
    public static class ServicosUtil
    {

        public static Boolean ServicoInstalado(string nomeservico)
        {
            ServiceController[] services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => s.ServiceName == nomeservico);
            return service != null;
        }

        public static string StatusServico(string nomeServico)
        {
            try
            {

                ServiceController[] services = ServiceController.GetServices();
                var service = services.FirstOrDefault(s => s.ServiceName == nomeServico);

                if (service != null)
                {
                    var statusServico = service.Status;

                    if (service.Status == ServiceControllerStatus.Stopped) return "parado";
                    else
                        if (service.Status == ServiceControllerStatus.Running) return "rodando";
                    else
                        return "indefinido";
                }
                else
                {
                    return "não instalado"; // Não instalado
                }

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string IniciarPararServico(string nomeServico)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                var service = services.FirstOrDefault(s => s.ServiceName == nomeServico);

                if (service == null)
                    return String.Format("O Serviço {0} não está instalado!", nomeServico);

                if (service.Status == ServiceControllerStatus.Stopped) service.Start();
                else
                    if (service.Status == ServiceControllerStatus.Running) service.Stop();
                return "ok";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string DesinstalarServico(string nomeServico)
        {
            try
            {

                ServiceController[] services = ServiceController.GetServices();
                var service = services.FirstOrDefault(s => s.ServiceName == nomeServico);

                if (service != null)
                {
                    var retorno = "";
                    try
                    {
                        ExecutarCMD(string.Format(@"sc stop {0} ", nomeServico));
                        retorno = ExecutarCMD(string.Format(@"sc delete {0} ", nomeServico));
                        // ServicosUtil.ExcluirServicoaSerMonitorado(nomeServico);
                    }
                    catch (Exception e)
                    {
                        retorno = e.Message;
                    }
                    return retorno;

                }
                else
                {
                    return "Serviço não instalado!";
                }

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string InstalarServico(string nomeServico, string caminhoServico, string nomeAmigavelServico, string descricaoServico, string chave)
        {
            try
            {

                ServiceController[] services = ServiceController.GetServices();
                var service = services.FirstOrDefault(s => s.ServiceName == nomeServico);

                if (service == null)
                {
                    var retorno = "";

                    if (File.Exists(caminhoServico))
                    {
                        try
                        {
                            retorno = ExecutarCMD(string.Format(@"sc create {0} binpath=""{1}"" displayName=""{2}"" start=auto ", nomeServico, caminhoServico, nomeAmigavelServico));
                            ExecutarCMD(string.Format(@"sc description {0} ""{1}"" ", nomeServico, descricaoServico));

                            //ParametroBL.IncluirServicoaSerMonitorado(nomeServico, descricaoServico, chave);

                            //retorno = "OK";
                        }
                        catch (Exception e)
                        {
                            retorno = e.Message;
                        }
                    }
                    else
                    {
                        retorno = string.Format(@"Serviço não encontrado na pasta {0}", caminhoServico);
                    }

                    return retorno;

                }
                else
                {
                    return "Serviço já instalado!";
                }

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string ExecutarCMD(string comando)
        {
            using (Process processo = new Process())
            {
                processo.StartInfo.FileName = Environment.GetEnvironmentVariable("comspec");

                // Formata a string para passar como argumento para o cmd.exe
                processo.StartInfo.Arguments = string.Format("/c {0}", comando);

                processo.StartInfo.RedirectStandardOutput = true;
                processo.StartInfo.UseShellExecute = false;
                processo.StartInfo.CreateNoWindow = true;

                processo.Start();
                processo.WaitForExit();

                string saida = processo.StandardOutput.ReadToEnd();
                return saida;
            }
        } 
        
        public static void ExcluirServicos(string nomecliente)
        {

            if (String.IsNullOrEmpty(nomecliente)) {
                Console.WriteLine("Nome do cliente não informado");
                return;
            }
            string searchKeyword ="Poliview CRM - " + nomecliente;

            // Obtém a lista de todos os serviços do sistema
            ServiceController[] services = ServiceController.GetServices();

            // Filtra os serviços com base na palavra-chave
            var matchingServices = services.Where(s => s.DisplayName.Contains(searchKeyword)).ToList();

            if (!matchingServices.Any())
            {
                Console.WriteLine($"Nenhum serviço encontrado contendo a palavra: {searchKeyword}");
                return;
            }

            Console.WriteLine("Serviços encontrados:");
            foreach (var service in matchingServices)
            {
                if (service.Status == ServiceControllerStatus.Running) service.Stop();

                try
                {
                    DesinstalarServico(service.ServiceName);
                    Console.WriteLine($"Serviço {service.DisplayName} excluído com sucesso");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao excluir o serviço {service.DisplayName}: {ex.Message}");
                }
            }
        }

        public static void PararTodosOsServicos(string nomecliente)
        {
            if (String.IsNullOrEmpty(nomecliente))
            {
                Console.WriteLine("Nome do cliente não informado");
                return;
            }
            string searchKeyword = "Poliview CRM - " + nomecliente;
            // Obtém a lista de todos os serviços do sistema
            ServiceController[] services = ServiceController.GetServices();
            // Filtra os serviços com base na palavra-chave
            var matchingServices = services.Where(s => s.DisplayName.Contains(searchKeyword)).ToList();
            if (!matchingServices.Any())
            {
                Console.WriteLine($"Nenhum serviço encontrado contendo a palavra: {searchKeyword}");
                return;
            }
            Console.WriteLine("Serviços encontrados:");
            foreach (var service in matchingServices)
            {
                if (service.Status == ServiceControllerStatus.Running) service.Stop();
                Console.WriteLine($"Serviço {service.DisplayName} parado com sucesso");
            }
        }
    }
}
