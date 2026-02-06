using Dapper;
using poliview.crm.conexao.services;
using poliview.crm.domain;

namespace poliview.crm.services
{
    public static class ClientesServices
    {

        public static void comparar(string connectionStringFirebird, string connectionStringSqlServer, ListBox listResumo, ListBox listInclusao, ListBox listExclusao, ListBox listScript)
        {
            listResumo.Items.Add(" ");
            listResumo.Items.Add("-----Clientes-------------------------------------------------------------------------");

            //var servico = new ClientesServices();
            var reginclusao = 0;
            var regexclusao = 0;
            var regcrm = 0;

            var registroscrm = ListarCrm(connectionStringSqlServer);
            var registrossiecon = ListarSiecon(connectionStringFirebird);

            regcrm = registroscrm.Count();
            listResumo.Items.Add($"Total de registros no SIECON: {registrossiecon.Count()} ");
            listResumo.Items.Add($"Total de registros no CRM...: {registroscrm.Count()} ");

            foreach (var item in registrossiecon)
            {
                //bool existeRegistro = suaLista.Any(item => item.SeuCampo == "SeuValor");
                var existe = registroscrm.Any(x => x.codigoclientesp7==item.codigoclientesp7);
                if (!existe)
                {
                    listInclusao.Items.Add($"codigocliente: {item.codigoclientesp7} cpf: {item.cpfcnpj} nome: {item.nomecliente}");
                    reginclusao++;
                }
            }

            
            foreach (var item in registroscrm)
            {
                //bool existeRegistro = suaLista.Any(item => item.SeuCampo == "SeuValor");
                var existe = registrossiecon.Any(x => x.codigoclientesp7 == item.codigoclientesp7);
                if (!existe)
                {
                    listExclusao.Items.Add($"codigocliente: {item.codigoclientesp7} cpf: {item.cpfcnpj} nome: {item.nomecliente}");
                    listScript.Items.Add($"delete from CAD_CLIENTE where cd_Clientesp7='{item.codigoclientesp7}'");
                    regexclusao++;
                }
            }

            listExclusao.Items.Add("finalização da verificação de exclusão");

            listResumo.Items.Add($"Total de registros de inclusão no CRM: {reginclusao}");
            listResumo.Items.Add($"Total de registros de exclusão no CRM: {regexclusao}");
            listResumo.Items.Add($"");
            listResumo.Items.Add($"Total de Registro depois das INCLUSÕES E EXCLUSÕES: {regcrm - regexclusao + reginclusao}");
            listResumo.Items.Add("-----Clientes-------------------------------------------------------------------------");

        }

        private static IEnumerable<Clientes> ListarCrm(string connectionstring)
        {
            var conn = Conexao.SqlServer(connectionstring);
            var sql = "select CD_Cliente as codigocliente, CD_ClienteSP7 as codigoclientesp7, NM_Cliente as nomecliente, NR_CPFCNPJ as cpfcnpj, DS_Email as email from CAD_CLIENTE WHERE CD_ClienteSP7<>'0' order by 1 ";
            conn.Open();
            var registros = conn.Query<Clientes>(sql);
            conn.Close();
            return registros;           
        }

        private static IEnumerable<Clientes> ListarSiecon(string connectionstring)
        {
            var conn = Conexao.Firebird(connectionstring);
            var sql = "select FORN_CNPJ as codigoclientesp7, forn_razao as nomecliente, forn_cpfcnpj as cpfcnpj from cadcpg_fornecedor WHERE FORN_TPCLIENTE=1 order by 1 ";
            conn.Open();
            var registros = conn.Query<Clientes>(sql);
            conn.Close();
            return registros;
        }
    }
}
