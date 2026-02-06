using Dapper;
using poliview.crm.conexao.services;
using poliview.crm.domain;

namespace poliview.crm.services
{
    public static class UnidadesServices
    {
        public static void comparar(string connectionStringFirebird, string connectionStringSqlServer, ListBox listResumo, ListBox listInclusao, ListBox listExclusao, ListBox listScript)
        {
            listResumo.Items.Add(" ");
            listResumo.Items.Add("-----UNIDADES-------------------------------------------------------------------------");
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
                var existe = registroscrm.Any((x => x.idempreendimentosp7 == item.idempreendimentosp7 &&
                                                    x.idblocosp7 == item.idblocosp7 &&
                                                    x.idunidadesp7 == item.idunidadesp7));
                if (!existe)
                {
                    listInclusao.Items.Add($"emprd: {item.idempreendimentosp7} bloco: {item.idblocosp7} unidade: {item.idunidadesp7}");
                    reginclusao++;
                }
            }
            
            foreach (var item in registroscrm)
            {
                //bool existeRegistro = suaLista.Any(item => item.SeuCampo == "SeuValor");
                var existe = registrossiecon.Any((x => x.idempreendimentosp7 == item.idempreendimentosp7 &&
                                                    x.idblocosp7 == item.idblocosp7 &&
                                                    x.idunidadesp7 == item.idunidadesp7));
                if (!existe)
                {
                    listExclusao.Items.Add($"emprd: {item.idempreendimentosp7} bloco: {item.idblocosp7} unidade: {item.idunidadesp7}");
                    listScript.Items.Add($"delete from OPE_UNIDADE WHERE cd_unidade={item.idunidade}");
                    listScript.Items.Add($"delete from CAD_UNIDADE WHERE CD_EmpreeSP7={item.idempreendimentosp7} and CD_BlocoSP7={item.idblocosp7} and NR_UnidadeSP7='{item.idunidadesp7}'");
                    regexclusao++;
                }
            }

            listExclusao.Items.Add("finalização da verificação de exclusão");

            listResumo.Items.Add($"Total de registros de inclusão no CRM: {reginclusao}");
            listResumo.Items.Add($"Total de registros de exclusão no CRM: {regexclusao}");
            listResumo.Items.Add($"");
            listResumo.Items.Add($"Total de Registro depois das INCLUSÕES E EXCLUSÕES: {regcrm - regexclusao + reginclusao}");
            listResumo.Items.Add("-----UNIDADES-------------------------------------------------------------------------");

        }

        private static IEnumerable<UnidadesCrm> ListarCrm(string connectionstring)
        {
            var conn = Conexao.SqlServer(connectionstring);
            var sql = "select CD_EmpreeSP7 as idempreendimentosp7, CD_BlocoSP7 as idblocosp7, NR_UnidadeSP7 as idunidadesp7, cd_unidade as idunidade " +
                      "from CAD_UNIDADE " +
                      "order by 1,2,3 ";
            conn.Open();
            var registros = conn.Query<UnidadesCrm>(sql);
            conn.Close();
            return registros;           
        }

        private static IEnumerable<UnidadesSiecon> ListarSiecon(string connectionstring)
        {
            var conn = Conexao.Firebird(connectionstring);
            var sql = "select emp.emprd_cdg as idempreendimentosp7, " +
                      "bl.bloco_cdg as idblocosp7, " + 
                      "un.undemprd_cdg as idunidadesp7 " +
                      "from emp_undemprd un " +
                      "left join caddvs_bloco bl on bl.bloco_cdg=un.undemprd_bloco and bl.bloco_emprd=un.undemprd_emprd " +
                      "left join caddvs_empreend emp on emp.emprd_cdg=un.undemprd_emprd " + 
                      "order by 1,2,3";
            conn.Open();
            var registros = conn.Query<UnidadesSiecon>(sql);
            conn.Close();
            return registros;
        }
    }
}
