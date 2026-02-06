using Dapper;
using poliview.crm.conexao.services;
using poliview.crm.domain;

namespace poliview.crm.services
{
    public static class BlocosServices
    {

        public static void comparar(string connectionStringFirebird, string connectionStringSqlServer, ListBox listResumo, ListBox listInclusao, ListBox listExclusao, ListBox listScript)
        {
            listResumo.Items.Add(" ");
            listResumo.Items.Add("-----Blocos-------------------------------------------------------------------------");

            //var servico = new BlocosServices();
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
                var existe = registroscrm.Any(x => x.codigoempreendimentosp7 == item.codigoempreendimentosp7 && x.codigosp7 == item.codigosp7);
                if (!existe)
                {
                    listInclusao.Items.Add($"Bloco: {item.codigo} Empreendimento: {item.codigoempreendimento} Nome: {item.nome} ");
                    reginclusao++;
                }
            }

            
            foreach (var item in registroscrm)
            {
                //bool existeRegistro = suaLista.Any(item => item.SeuCampo == "SeuValor");
                var existe = registrossiecon.Any(x => x.codigoempreendimentosp7 == item.codigoempreendimentosp7 && x.codigosp7 == item.codigosp7);
                if (!existe)
                {
                    listExclusao.Items.Add($"Bloco: {item.codigo} Empreendimento: {item.codigoempreendimento} Nome: {item.nome} ");
                    listScript.Items.Add($"DELETE FROM OPE_BLOCO WHERE CD_Bloco={item.codigo} AND CD_Empreendimento={item.codigoempreendimento}");
                    listScript.Items.Add($"DELETE FROM CAD_BLOCO WHERE CD_BlocoSP7={item.codigosp7} AND CD_EmpreeSP7={item.codigoempreendimentosp7}");
                    regexclusao++;
                }
            }

            listExclusao.Items.Add("finalização da verificação de exclusão");

            listResumo.Items.Add($"Total de registros de inclusão no CRM: {reginclusao}");
            listResumo.Items.Add($"Total de registros de exclusão no CRM: {regexclusao}");
            listResumo.Items.Add($"");
            listResumo.Items.Add($"Total de Registro depois das INCLUSÕES E EXCLUSÕES: {regcrm - regexclusao + reginclusao}");
            listResumo.Items.Add("-----Blocos-------------------------------------------------------------------------");

        }

        private static IEnumerable<Blocos> ListarCrm(string connectionstring)
        {
            var conn = Conexao.SqlServer(connectionstring);
            var sql = "select b.CD_Bloco as codigo, e.CD_Empreendimento as codigoempreendimento, b.CD_BlocoSP7 as codigosp7, b.CD_EmpreeSP7 as codigoempreendimentosp7, b.NM_Bloco as nome, NM_BlocoAbrev as abreviatura from cad_bloco b left join CAD_EMPREENDIMENTO e on e.CD_EmpreeSP7=b.CD_EmpreeSP7 order by 1,2  ";
            conn.Open();
            var registros = conn.Query<Blocos>(sql);
            conn.Close();
            return registros;
        }

        private static IEnumerable<Blocos> ListarSiecon(string connectionstring)
        {
            var conn = Conexao.Firebird(connectionstring);
            var sql = "select bloco_cdg as codigosp7, bloco_emprd as codigoempreendimentosp7, bloco_desc as nome from caddvs_bloco order by 1,2";
            conn.Open();
            var registros = conn.Query<Blocos>(sql);
            conn.Close();
            return registros;
        }
    }
}
