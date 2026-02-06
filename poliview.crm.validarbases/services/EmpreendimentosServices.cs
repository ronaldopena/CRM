using Dapper;
using poliview.crm.conexao.services;
using poliview.crm.domain;

namespace poliview.crm.services
{
    public static class EmpreendimentosServices
    {

        public static void comparar(string connectionStringFirebird, string connectionStringSqlServer, ListBox listResumo, ListBox listInclusao, ListBox listExclusao, ListBox listScript)
        {
            listResumo.Items.Add(" ");
            listResumo.Items.Add("-----Empreendimentos-------------------------------------------------------------------------");

            //var servico = new EmpreendimentosServices();
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
                var existe = registroscrm.Any(x => x.codigo == item.codigo);
                if (!existe)
                {
                    listInclusao.Items.Add($"Empreendimento: {item.codigo} Nome: {item.nome} ");
                    reginclusao++;
                }
            }

            
            foreach (var item in registroscrm)
            {
                //bool existeRegistro = suaLista.Any(item => item.SeuCampo == "SeuValor");
                var existe = registrossiecon.Any(x => x.codigo == item.codigo);
                if (!existe)
                {
                    listExclusao.Items.Add($"Empreendimento: {item.codigo} Nome: {item.nome} ");
                    listScript.Items.Add($"DELETE FROM CAD_EMPREENDIMENTO WHERE CD_EmpreeSP7={item.codigo}");
                    regexclusao++;
                }
            }

            listExclusao.Items.Add("finalização da verificação de exclusão");

            listResumo.Items.Add($"Total de registros de inclusão no CRM: {reginclusao}");
            listResumo.Items.Add($"Total de registros de exclusão no CRM: {regexclusao}");
            listResumo.Items.Add($"");
            listResumo.Items.Add($"Total de Registro depois das INCLUSÕES E EXCLUSÕES: {regcrm - regexclusao + reginclusao}");
            listResumo.Items.Add("-----Empreendimentos-------------------------------------------------------------------------");

        }

        private static IEnumerable<Empreendimentos> ListarCrm(string connectionstring)
        {
            var conn = Conexao.SqlServer(connectionstring);
            var sql = "select CD_EmpreeSP7 as codigo, NM_Empree as nome from CAD_EMPREENDIMENTO order by 1 ";                      
            conn.Open();
            var registros = conn.Query<Empreendimentos>(sql);
            conn.Close();
            return registros;
        }

        private static IEnumerable<Empreendimentos> ListarSiecon(string connectionstring)
        {
            var conn = Conexao.Firebird(connectionstring);
            var sql = "select emprd_cdg as codigo, emprd_desc as nome from  caddvs_empreend order by 1";
            conn.Open();
            var registros = conn.Query<Empreendimentos>(sql);
            conn.Close();
            return registros;
        }
    }
}
