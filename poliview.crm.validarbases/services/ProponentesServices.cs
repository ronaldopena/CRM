using Dapper;
using poliview.crm.conexao.services;
using poliview.crm.domain;

namespace poliview.crm.services
{
    public static class ProponentesServices
    {

        public static void comparar(string connectionStringFirebird, string connectionStringSqlServer, ListBox listResumo, ListBox listInclusao, ListBox listExclusao, ListBox listScript)
        {
            listResumo.Items.Add(" ");
            listResumo.Items.Add("-----PROPONENTES-------------------------------------------------------------------------");

            //var servico = new ProponentesServices();
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
                var existe = registroscrm.Any(x => x.codigoclientesp7 == item.codigoclientesp7 && x.contratosp7 == item.contratosp7);
                if (!existe)
                {
                    listInclusao.Items.Add($"codigo: {item.codigo} contrato: {item.contratosp7} codigocliente: {item.codigoclientesp7}");
                    reginclusao++;
                }
            }

            
            foreach (var item in registroscrm)
            {
                //bool existeRegistro = suaLista.Any(item => item.SeuCampo == "SeuValor");
                var existe = registrossiecon.Any(x => x.codigoclientesp7 == item.codigoclientesp7 && x.contratosp7 == item.contratosp7);
                if (!existe)
                {
                    listExclusao.Items.Add($"codigo: {item.codigo} contrato: {item.contratosp7} codigocliente: {item.codigoclientesp7}");
                    listScript.Items.Add($"delete from CAD_PROPONENTE where cd_clientesp7='{item.codigoclientesp7}' and cd_contratosp7='{item.contratosp7}'");
                    regexclusao++;
                }
            }

            listExclusao.Items.Add("finalização da verificação de exclusão");

            listResumo.Items.Add($"Total de registros de inclusão no CRM: {reginclusao}");
            listResumo.Items.Add($"Total de registros de exclusão no CRM: {regexclusao}");
            listResumo.Items.Add($"");
            listResumo.Items.Add($"Total de Registro depois das INCLUSÕES E EXCLUSÕES: {regcrm - regexclusao + reginclusao}");
            listResumo.Items.Add("-----PROPONENTES-------------------------------------------------------------------------");

        }

        private static IEnumerable<ProponentesCrm> ListarCrm(string connectionstring)
        {
            var conn = Conexao.SqlServer(connectionstring);
            var sql = "select CD_Proponente as codigo, CD_ContratoSP7 as contratosp7, CD_ClienteSP7 as codigoclientesp7, codigocessao as cessao from CAD_PROPONENTE order by 1  ";
            conn.Open();
            /*
             public int codigo { get; set; }
        public string? contratosp7 { get; set; }
        public string? codigoclientesp7 { get; set; }
        public int cessao { get; set; }  
            */
            var registros = conn.Query<ProponentesCrm>(sql);
            conn.Close();
            return registros;           
        }

        private static IEnumerable<ProponentesSiecon> ListarSiecon(string connectionstring)
        {
            var conn = Conexao.Firebird(connectionstring);
            var sql = "select PROPONENTE_CDG as codigo, PROPONENTE_CTR as contratosp7, PROPONENTE_FORNECEDOR as codigoclientesp7, PROPONENTE_PERCENTUAL as percentual, PROPONENTE_CESSAO as cessao, PROPONENTE_DTLIDOCRM as dataultimaalteracao from EMP_PROPONENTE order by 1";
            conn.Open();
            var registros = conn.Query<ProponentesSiecon>(sql);
            conn.Close();
            return registros;
        }
    }
}
