using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface IAgendaService
    {
        public AgendaResposta RetornaAgenda(int idgrupo, string data);
        public List<RecursoAgenda> RetornaDataMarcadas(int idgrupo, string data);
        public List<RecursoAgenda> RetornaDataMarcadasDia(string data, int idgrupo, int idrecurso);
        public List<DatasAgenda> RetornaDatas(string data);
        public string RetornaDiaSemana(int dia);
        public List<HorarioDataAgenda> HorarioData(string data, int idrecurso, List<RecursoAgenda> marcadas);
        public ExcluirAgendaResposta ExcluirAgenda(RecursoAgenda marcacao);
        public GravarAgendaResposta GravarAgenda(RecursoAgenda marcacao);
        public AgendaDiaResposta RetornaAgendaDia(string data, int idgrupo, int idrecurso);
    }
    public class AgendaService : IAgendaService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public AgendaService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public AgendaResposta RetornaAgenda(int idgrupo, string data)
        {
            var dia = Convert.ToInt16(data.Substring(0, 2).ToString());
            var mes = Convert.ToInt16(data.Substring(3, 2).ToString());
            var ano = Convert.ToInt16(data.Substring(6, 4).ToString());

            DateTime dt = new DateTime(ano, mes, dia);

            var obj = new ListaAgenda();
            var rec = new RecursoService(_configuration);
            var recursosGrupo = rec.ListaRecursosPorGrupo(idgrupo);

            var datasmarcadas = RetornaDataMarcadas(idgrupo, data);
            
            obj.datas = RetornaDatas(data);            
            obj.recursos = new List<RecursoDatasAgenda>();

            foreach (var recurso in recursosGrupo)
            {
                var recursosData = new RecursoDatasAgenda();
                recursosData.codigo = recurso.CD_Usuario;
                recursosData.nome = recurso.NM_Usuario;

                var datamarcadarecurso = new List<Recurso>();

                datasmarcadas.Where(x => x.idrecurso == recursosData.codigo);                
                recursosData.dia1 = HorarioData(dt.AddDays(0).ToString("dd/MM/yyyy"), Convert.ToInt16(recursosData.codigo), datasmarcadas);
                recursosData.dia2 = HorarioData(dt.AddDays(1).ToString("dd/MM/yyyy"), Convert.ToInt16(recursosData.codigo), datasmarcadas);
                recursosData.dia3 = HorarioData(dt.AddDays(2).ToString("dd/MM/yyyy"), Convert.ToInt16(recursosData.codigo), datasmarcadas);
                recursosData.dia4 = HorarioData(dt.AddDays(3).ToString("dd/MM/yyyy"), Convert.ToInt16(recursosData.codigo), datasmarcadas);
                recursosData.dia5 = HorarioData(dt.AddDays(4).ToString("dd/MM/yyyy"), Convert.ToInt16(recursosData.codigo), datasmarcadas);
                recursosData.dia6 = HorarioData(dt.AddDays(5).ToString("dd/MM/yyyy"), Convert.ToInt16(recursosData.codigo), datasmarcadas);
                recursosData.dia7 = HorarioData(dt.AddDays(6).ToString("dd/MM/yyyy"), Convert.ToInt16(recursosData.codigo), datasmarcadas);
                obj.recursos.Add(recursosData);
            }           
            return new AgendaResposta() { sucesso=true, objeto=obj, mensagem="ok", status=200 };
        }

        public ExcluirAgendaResposta ExcluirAgenda(RecursoAgenda marcacao)
        {
            var ret = new ExcluirAgendaResposta();

            try
            {
                Console.WriteLine(marcacao.dataagenda);
                var connection = new SqlConnection(_connectionString);
                var data1 = marcacao.dataagenda + " " + marcacao.horaini + ":00";
                var data2 = marcacao.dataagenda + " " + marcacao.horafim + ":00";
                Console.WriteLine($"data1={data1}");
                Console.WriteLine($"data2={data2}");
                var query = "SET DATEFORMAT dmy; " +
                        "DELETE FROM OPE_RECURSO_AGENDA " +
                        $"where CD_Recurso ={marcacao.idrecurso} and " +
                        $"CD_OrdemServico ={marcacao.idordemservico} and " +
                        $"CD_Grupo ={marcacao.idgrupo} and " +
                        $"DT_AgendaIni = '{data1}' and DT_AgendaFim = '{data2}';";
                connection.Query(query);
                Console.WriteLine(query);
                ret.sucesso = true;
                ret.mensagem = "Marcação da agenda excluida com sucesso!";
                ret.objeto = marcacao;
            }
            catch (Exception ex)
            {
                ret.sucesso = false;
                ret.mensagem = ex.Message;
                ret.objeto = null;
                Console.WriteLine("ERRO: " + ex.Message);
            }

            return ret;
        }

        public GravarAgendaResposta GravarAgenda(RecursoAgenda marcacao)
        {
            var ret = new GravarAgendaResposta();
            try
            {
                var data1 = marcacao.dataagenda + " " + marcacao.horaini + ":00";
                var data2 = marcacao.dataagenda + " " + marcacao.horafim + ":00";

                var connection = new SqlConnection(_connectionString);

                var query = "SET DATEFORMAT dmy; " +
                              "INSERT INTO OPE_RECURSO_AGENDA " +
                              "(CD_BancoDados, CD_Mandante, CD_Recurso, DT_AgendaIni, DT_AgendaFim, " +
                              "CD_OrdemServico, CD_Grupo, IN_Status, DS_Observacao, indisponivel) " +
                              $"VALUES(1, 1, { marcacao.idrecurso}, '{data1}', " +
                              $"'{data2}', { marcacao.idordemservico}, { marcacao.idgrupo}, " +
                              $"'A', '{marcacao.observacao}',{ marcacao.indisponivel});";

                connection.Query(query);

                ret.sucesso = true;
                ret.mensagem = "agenda incluída com sucesso!";
                ret.objeto = marcacao;
            }
            catch (Exception ex)
            {
                ret.sucesso = false;
                ret.mensagem = ex.Message;
                ret.objeto = null;
            }
            return ret;
        }

        public List<HorarioDataAgenda> HorarioData(string data, int idrecurso, List<RecursoAgenda> marcadas)
        {            
            var ret = new List<HorarioDataAgenda>();
            for (int i = 0; i <= 23; i++)
            {
                var item = new HorarioDataAgenda();

                var y = marcadas.Where(x => x.idrecurso == idrecurso && x.dataagenda == data && i >= Convert.ToInt16(x.horaini.Substring(0, 2)) && i < Convert.ToInt16(x.horafim.Substring(0, 2))).ToList();

                if (y.Count > 0)
                {                                        
                    if (y[0].idordemservico==0 && y[0].idgrupo==0)
                    {
                        item.ordemservico = "indisponível";
                        item.reservado = 2;
                    }
                    else
                    {
                        item.ordemservico = $"{y[0].idordemservico}";
                        item.reservado = 1;
                    }
                }
                else
                {
                    item.ordemservico = "";
                    item.reservado = 0;
                }
                ret.Add(item);
            }
            return ret;
        }   

        public List<RecursoAgenda> RetornaDataMarcadas(int idgrupo, string data)
        {
            var dia = Convert.ToInt16(data.Substring(0, 2).ToString());
            var mes = Convert.ToInt16(data.Substring(3, 2).ToString());
            var ano = Convert.ToInt16(data.Substring(6, 4).ToString());

            DateTime dt = new DateTime(ano, mes, dia);

            var data1 = dt.AddDays(0).ToString("dd/MM/yyyy");
            var data2 = dt.AddDays(7).ToString("dd/MM/yyyy");

            var ret = new List<Recurso>();
            using var connection = new SqlConnection(_connectionString);
            var query = "SET DATEFORMAT dmy; " +
                    "SELECT CD_RECURSO as idrecurso, " +
                    "CONVERT(varchar(10), DT_AgendaIni, 103) as dataagenda, " +
                    "CONVERT(varchar(8), DT_AgendaIni, 108) AS horaini, " +
                    "CONVERT(varchar(8), DT_AgendaFim, 108) AS horafim, " +
                    "cd_ordemServico as idordemservico, CD_GRUPO as idgrupo, ds_observacao as observacao " +
                    "FROM OPE_RECURSO_AGENDA " +
                    $"where cd_grupo in (0,{idgrupo}) and DT_AgendaIni>= '{data1}' and DT_AgendaIni<= '{data2}';";
            var result = connection.Query<RecursoAgenda>(query);
            return result.ToList();
        }

        public List<RecursoAgenda> RetornaDataMarcadasDia(string data, int idgrupo, int idrecurso)
        {
            var data1 = data + " 00:00:00";
            var data2 = data + " 23:59:59";

            using var connection = new SqlConnection(_connectionString);
            var query = "SET DATEFORMAT dmy; " +
                     "SELECT CD_RECURSO as idrecurso, " +
                     "CONVERT(varchar(10), DT_AgendaIni, 103) as dataagenda, " +
                     "CONVERT(varchar(8), DT_AgendaIni, 108) AS horaini, " +
                     "CONVERT(varchar(8), DT_AgendaFim, 108) AS horafim, " +
                     "cd_ordemServico as idordemservico, CD_GRUPO as idgrupo, ds_observacao as observacao " +
                     "FROM OPE_RECURSO_AGENDA " +
                    $"where cd_grupo in (0,{idgrupo}) and cd_recurso in (0,{idrecurso}) and DT_AgendaIni>= '{data1}' and DT_AgendaIni<= '{data2}';";

            var result = connection.Query<RecursoAgenda>(query);
            return result.ToList();
        }

        public List<DatasAgenda> RetornaDatas(string data)
        {

            var dia = Convert.ToInt16(data.Substring(0,2).ToString());
            var mes = Convert.ToInt16(data.Substring(3, 2).ToString());
            var ano = Convert.ToInt16(data.Substring(6, 4).ToString());

            DateTime dt = new DateTime(ano, mes, dia);

            var lista = new List<DatasAgenda>();

            for (int i = 0; i < 7; i++)
            {                
                //var dt.DayOfWeek();                
                var datax = dt.ToString("dd/MM/yyyy");
                var diax = Convert.ToInt16(datax.Substring(0, 2).ToString());
                var diasemanax = RetornaDiaSemana((int)dt.DayOfWeek);

                var dataAux = new DatasAgenda();
                dataAux.data = datax;
                dataAux.diasemana = diasemanax;

                lista.Add(dataAux);

                dt = dt.AddDays(1);
            }

            return lista;
        }

        public string RetornaDiaSemana(int dia)
        {
            string [] retorno = { "domingo", "segunda-feira", "terça-feira", "quarta-feira", "quinta-feira", "sexta-feira", "sábado" };
            return retorno[dia];
        }

        public AgendaDiaResposta RetornaAgendaDia(string data, int idgrupo, int idrecurso)
        {
            var ret = new AgendaDiaResposta();
            try
            {
                ret.objeto = RetornaDataMarcadasDia(data, idgrupo, idrecurso);
                ret.mensagem = "ok";
                ret.status = 200;
                ret.sucesso = true;
            }
            catch (Exception ex)
            {
                ret.mensagem = ex.Message;
                ret.status = 500;
                ret.sucesso = false;
            }
            
            return ret;
        }
    }
}
