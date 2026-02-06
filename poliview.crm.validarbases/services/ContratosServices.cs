using Dapper;
using poliview.crm.conexao.services;
using poliview.crm.domain;

namespace poliview.crm.services
{
    public static class ContratosServices
    {

        public static void comparar(string connectionStringFirebird, string connectionStringSqlServer, ListBox listResumo, ListBox listInclusao, ListBox listExclusao, ListBox listScript)
        {
            listResumo.Items.Add(" ");
            listResumo.Items.Add("-----Contratos-------------------------------------------------------------------------");

            //var servico = new ContratosServices();
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
                var existe = registroscrm.Any(x => x.codigoclientesp7==item.codigoclientesp7 && x.codigocontratosp7 == item.codigocontratosp7);
                if (!existe)
                {
                    listInclusao.Items.Add($"contrato: {item.codigocontratosp7} codigocliente: {item.codigoclientesp7} cpf: {item.cpfcnpj} nome: {item.nome}");
                    reginclusao++;
                }
            }

            
            foreach (var item in registroscrm)
            {
                //bool existeRegistro = suaLista.Any(item => item.SeuCampo == "SeuValor");
                var existe = registrossiecon.Any(x => x.codigoclientesp7 == item.codigoclientesp7 && x.codigocontratosp7 == item.codigocontratosp7);
                if (!existe)
                {
                    listExclusao.Items.Add($"contrato: {item.codigocontratosp7} codigocliente: {item.codigoclientesp7} cpf: {item.cpfcnpj} nome: {item.nome}");
                    listScript.Items.Add($"delete from OPE_CONTRATO where cd_contrato={item.codigocontrato} and cd_cliente={item.codigocliente}");
                    listScript.Items.Add($"delete from CAD_CONTRATO where cd_contratosp7='{item.codigocontratosp7}' and cd_clientesp7='{item.codigoclientesp7}'");
                    regexclusao++;
                }
            }

            listExclusao.Items.Add("finalização da verificação de exclusão");

            listResumo.Items.Add($"Total de registros de inclusão no CRM: {reginclusao}");
            listResumo.Items.Add($"Total de registros de exclusão no CRM: {regexclusao}");
            listResumo.Items.Add($"");
            listResumo.Items.Add($"Total de Registro depois das INCLUSÕES E EXCLUSÕES: {regcrm - regexclusao + reginclusao}");
            listResumo.Items.Add("-----Contratos-------------------------------------------------------------------------");

        }

        private static IEnumerable<Contratos> ListarCrm(string connectionstring)
        {
            var conn = Conexao.SqlServer(connectionstring);
            var sql = "select c.CD_Contrato as codigocontrato, c.CD_ContratoSP7 as codigocontratosp7, cli.CD_Cliente as codigocliente, c.CD_ClienteSP7 as codigoclientesp7, cli.NR_CPFCNPJ as cpfcnpj, cli.NM_Cliente as nome, emp.CD_Empreendimento as empreendimento, c.CD_EmpreeSP7 as empreendimentosp7,blo.CD_Bloco as bloco, c.CD_BlocoSP7 as blocosp7, uni.CD_Unidade as unidade, c.NR_UnidadeSP7 as unidadesp7, c.IN_StatusSP7 as status, c.IN_StatusRemanejadoSP7 as remanejado, c.IN_StatusDistratoSP7 as statusdistrato " +
                      "from CAD_CONTRATO c " +
                      "left join CAD_CLIENTE cli on cli.CD_ClienteSP7=c.CD_ClienteSP7 " +
                      "left join CAD_EMPREENDIMENTO emp on emp.CD_EmpreeSP7 = c.CD_EmpreeSP7 " +
                      "left join CAD_BLOCO blo on blo.CD_EmpreeSP7 = c.CD_EmpreeSP7 and blo.CD_BlocoSP7 = c.CD_BlocoSP7 " +
                      "left join CAD_UNIDADE uni on uni.CD_EmpreeSP7 = c.CD_EmpreeSP7 and uni.CD_BlocoSP7 = c.CD_BlocoSP7 and uni.NR_UnidadeSP7 = c.NR_UnidadeSP7 ";
            conn.Open();
            var registros = conn.Query<Contratos>(sql);
            conn.Close();
            return registros;           
        }

        private static IEnumerable<Contratos> ListarSiecon(string connectionstring)
        {
            var conn = Conexao.Firebird(connectionstring);
            var sql = "select c.ctr_cdg as codigocontratosp7, c.ctr_fornecedor as codigoclientesp7, f.forn_cpfcnpj as cpfcnpj, f.forn_razao as nome, c.ctr_emprd as empreendimentosp7, c.ctr_bloco as blocosp7, c.ctr_undemprd as unidadesp7, c.ctr_statusdistrato as statusdistrato, c.ctr_dtlidocrm as dataultimaalteracao, c.ctr_status as status, c.ctr_remanejado as remanejado from emp_ctr c " +
                      "left join cadcpg_fornecedor f on f.forn_cnpj=c.ctr_fornecedor ";
            conn.Open();
            var registros = conn.Query<Contratos>(sql);
            conn.Close();
            return registros;
        }
    }
}
