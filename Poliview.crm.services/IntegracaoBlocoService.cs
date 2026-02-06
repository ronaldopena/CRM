using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class IntegracaoBlocoService
    {
        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        private FbConnection _conectionFB;
        private SqlConnection _conectionMSSQL;
        private int _CodigoTabela = 4;

        public IntegracaoBlocoService(string _connectionStringFb, string _connectionStringMssql)
        {           
            _conectionFB = new FirebirdConnectionService(_connectionStringFb).CreateConnection();
            _conectionMSSQL = new SqlServerConnectionService(_connectionStringMssql).CreateConnection();
        }

        public void Integrar(int codigoempreendimentosp7, int codigoblocosp7)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();            
            var registrosOrigem = this.ListarOrigem(codigoempreendimentosp7, codigoblocosp7);
            if (registrosOrigem != null)
            {
                ExcluirRegistros();
                salvarDadosDestino(registrosOrigem);
            }
    
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
            _conectionFB.Close();
            _conectionMSSQL.Close();
        }

        private bool ExcluirRegistros()
        {
            try
            {
                var registrosOrigemExclusao = this.ListarOrigemExclusao();
                if (registrosOrigemExclusao != null)
                {
                    foreach (var item in registrosOrigemExclusao)
                    {
                        var chave = item.chave.Split(";");

                        ExcluirRegistro(chave[0], chave[1]);

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private void ExcluirRegistro(string empreendimento, string bloco)
        {
            var connection = _conectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_BLOCO] WHERE AND CD_EmpreeSP7={empreendimento} AND CD_BlocoSP7={bloco}";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _conectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='BLOCOS' and chave is not null ";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private List<BlocosIntegracao> ListarOrigem(int codigoempreendimentosp7, int codigoblocosp7)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                    "BLOCO_EMPRD as codigoempreendimentosp7, BLOCO_CDG as codigoblocosp7, BLOCO_DESC as nome, " +
                    "BLOCO_ABREV as abreviatura, BLOCO_END as endereco, BLOCO_BAIRRO as bairro, BLOCO_CID as cidade, BLOCO_UF as estado, BLOCO_CEP as cep, " +
                    "BLOCO_DTLIDOCRM as datahoraultimaatualizacao " +
                    "FROM CADDVS_BLOCO " +
                    $"WHERE BLOCO_EMPRD={codigoempreendimentosp7} and BLOCO_CDG={codigoblocosp7}";
            return connection.Query<BlocosIntegracao>(sql).ToList();
        }

        private List<BlocosIntegracao> ListarOrigem(DateTime _DataHoraUlimaIntegracao)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                    "BLOCO_EMPRD as codigoempreendimentosp7, BLOCO_CDG as codigoblocosp7, BLOCO_DESC as nome, " +
                    "BLOCO_ABREV as abreviatura, BLOCO_END as endereco, BLOCO_BAIRRO as bairro, BLOCO_CID as cidade, BLOCO_UF as estado, BLOCO_CEP as cep, " +
                    "BLOCO_DTLIDOCRM as datahoraultimaatualizacao " +
                    "FROM CADDVS_BLOCO " +
                    $"WHERE BLOCO_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<BlocosIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<BlocosIntegracao> dadosorigem)
        {
            foreach (var item in dadosorigem)
            {
                if (JaEstaCadastrado(item))
                {
                    Alterar(item);
                }
                else
                {
                    Incluir(item);
                }
            }
        }

        private int ProximoCodigo()
        {
            var connection = _conectionMSSQL;
            var sql = $"SELECT COALESCE(MAX(CD_Bloco)+1,1) as codigo FROM [dbo].[CAD_BLOCO]";
            var proximo = connection.Query<int>(sql).ToList();
            return proximo[0];
        }

        private void Incluir(BlocosIntegracao obj)
        {
            var codigo = ProximoCodigo();
            var connection = _conectionMSSQL;
            var sql = "" +
                    $"INSERT INTO [dbo].[CAD_BLOCO] " +
                    $"           ([CD_BancoDados] " +
                    $"           ,[CD_Mandante] " +
                    $"           ,[CD_Bloco] " +
                    $"           ,[CD_EmpreeSP7] " +
                    $"           ,[CD_BlocoSP7] " +
                    $"           ,[NM_Bloco] " +
                    $"           ,[NM_BlocoAbrev] " +
                    $"           ,[DS_Endereco] " +
                    $"           ,[NM_Bairro] " +
                    $"           ,[NM_Cidade] " +
                    $"           ,[NM_UF] " +
                    $"           ,[NR_CEP] " +
                    $"           ,[DT_Controle] " +
                    $"           ,[HR_Controle]) " +
                    $"     VALUES " +
                    $"           (1 " +
                    $"           ,1 " +
                    $"           ,@codigo " +
                    $"           ,@codigoempreendimentosp7 " +
                    $"           ,@codigoblocosp7 " +
                    $"           ,@nome " +
                    $"           ,@abreviatura " +
                    $"           ,@endereco " +
                    $"           ,@bairro " +
                    $"           ,@cidade " +
                    $"           ,@estado " +
                    $"           ,@cep " +
                    $"           ,@dataatualizacao " +
                    $"           ,@horaatualizacao )";

            var parameters = new
            {
                codigo = codigo,
                codigoempreendimentosp7 = obj.codigoempreendimentosp7,
                codigoblocosp7 = obj.codigoblocosp7,
                nome = obj.nome,
                abreviatura = obj.abreviatura,
                endereco = obj.endereco,
                bairro = obj.bairro,
                cidade = obj.cidade,
                estado = obj.estado,
                cep = obj.cep,
                dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
            };
            connection.ExecuteAsync(sql, parameters).Wait();
        }

        private void Alterar(BlocosIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = "" +
                    $"UPDATE [dbo].[CAD_BLOCO] " +
                    $"   SET [CD_EmpreeSP7] = @codigoempreendimentosp7 " +
                    $"      ,[CD_BlocoSP7] = @codigoblocosp7 " +
                    $"      ,[NM_Bloco] = @nome " +
                    $"      ,[NM_BlocoAbrev] = @abreviatura " +
                    $"      ,[DS_Endereco] = @endereco " +
                    $"      ,[NM_Bairro] = @bairro " +
                    $"      ,[NM_Cidade] = @cidade " +
                    $"      ,[NM_UF] = @estado " +
                    $"      ,[NR_CEP] = @cep " +
                    $"      ,[DT_Controle] = @dataatualizacao " +
                    $"      ,[HR_Controle] = @horaatualizacao " +
                    $" WHERE [CD_EmpreeSP7]=@codigoempreendimentosp7 and  [CD_BlocoSP7]=@codigoblocosp7";

            var parameters = new
            {
                codigoempreendimentosp7 = obj.codigoempreendimentosp7,
                codigoblocosp7 = obj.codigoblocosp7,
                nome = obj.nome,
                abreviatura = obj.abreviatura,
                endereco = obj.endereco,
                bairro = obj.bairro,
                cidade = obj.cidade,
                estado = obj.estado,
                cep = obj.cep,
                dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
            };
            connection.ExecuteAsync(sql, parameters).Wait();
        }

        private Boolean JaEstaCadastrado(BlocosIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"select 1 from CAD_BLOCO where CD_BLOCOSP7={obj.codigoblocosp7} AND CD_EmpreeSP7={obj.codigoempreendimentosp7}";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }


        private DateTime DataHoraDaUltimaIntegracao()
        {
            var connection = _conectionMSSQL;
            var sql = $"select DataUltimaIntegracao from CAD_INTEGRACAO where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            return connection.QueryFirstOrDefault<DateTime>(sql);
        }
        private void AlterarDataHoraDaUltimaIntegracao(DateTime _DataHoraAtual)
        {
            var connection = _conectionMSSQL;
            var sql = $"UPDATE CAD_INTEGRACAO SET integrar=0, DataUltimaIntegracao='{_DataHoraAtual.ToString("yyyy-MM-dd HH:mm:ss")}' where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            connection.ExecuteAsync(sql).Wait();
        }
    }
}
