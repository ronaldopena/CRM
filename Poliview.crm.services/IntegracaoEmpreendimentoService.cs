using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class IntegracaoEmpreendimentoService
    {
        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        private FbConnection _conectionFB;
        private SqlConnection _conectionMSSQL;
        public IntegracaoEmpreendimentoService(string _connectionStringFb, string _connectionStringMssql)
        {           
            _conectionFB = new FirebirdConnectionService(_connectionStringFb).CreateConnection();
            _conectionMSSQL = new SqlServerConnectionService(_connectionStringMssql).CreateConnection();
        }

        public void Integrar(int codigoempreendimentosp7)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();            
            var registrosOrigem = this.ListarOrigem(codigoempreendimentosp7);
            if (registrosOrigem != null)
            {
                ExcluirRegistros();
                salvarDadosDestino(registrosOrigem);
            }
    
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";

            _conectionFB.Close();
            _conectionFB.Dispose();
            _conectionMSSQL.Close();
            _conectionMSSQL.Dispose();
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

                        ExcluirRegistro(chave[0]);
                        ExcluirRegistroProcessado(item.chave);

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void ExcluirRegistro(string empreendimento)
        {
            var connection = _conectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_EMPREENDIMENTO] WHERE AND CD_EmpreeSP7={empreendimento}";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _conectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='EMPREENDIMENTOS' and chave is not null ";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _conectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where tabela='EMPREENDIMENTOS' AND CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
        }
        private List<EmpreendimentosIntegracao> ListarOrigem(DateTime _DataHoraUlimaIntegracao)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                            "EMPRD_CDG as empreendimentosp7, EMPRD_DESC as empreendimento, EMPRD_ABREV as abreviatura, EMPRD_END as endereco, " +
                            "EMPRD_BAIRRO as bairro, EMPRD_CID as cidade, EMPRD_UF as estado, EMPRD_CEP as cep, " +
                            "EMPRD_ENTIDADE as entidade, EMPRD_REGIAO as regiao, EMPRD_TEMPRD as tipoempreendimento, " +
                            "EMPRD_PADRAO as empreendimentopadrao, EMPRD_MUNICIPIO as codigomunicipio, " +
                            "EMPRD_DTLIDOCRM as datahoraultimaatualizacao " +
                            "FROM CADDVS_EMPREEND " +
                            $"WHERE EMPRD_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<EmpreendimentosIntegracao>(sql).ToList();
        }

        private List<EmpreendimentosIntegracao> ListarOrigem(int codigoempreendimentosp7)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                            "EMPRD_CDG as empreendimentosp7, EMPRD_DESC as empreendimento, EMPRD_ABREV as abreviatura, EMPRD_END as endereco, " +
                            "EMPRD_BAIRRO as bairro, EMPRD_CID as cidade, EMPRD_UF as estado, EMPRD_CEP as cep, " +
                            "EMPRD_ENTIDADE as entidade, EMPRD_REGIAO as regiao, EMPRD_TEMPRD as tipoempreendimento, " +
                            "EMPRD_PADRAO as empreendimentopadrao, EMPRD_MUNICIPIO as codigomunicipio, " +
                            "EMPRD_DTLIDOCRM as datahoraultimaatualizacao " +
                            "FROM CADDVS_EMPREEND " +
                            $"WHERE EMPRD_CDG={codigoempreendimentosp7}";
            return connection.Query<EmpreendimentosIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<EmpreendimentosIntegracao> dadosorigem)
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
            var sql = $"SELECT COALESCE(MAX(CD_Empreendimento)+1,1) as codigo FROM [dbo].[CAD_EMPREENDIMENTO]";
            var proximo = connection.Query<int>(sql).ToList();
            return proximo[0];
        }

        private void Incluir(EmpreendimentosIntegracao obj)
        {
            var codigoempreendimento = ProximoCodigo();
            obj.codigopadrao = 1;
            var connection = _conectionMSSQL;
            var sql = "" +
                    $"INSERT INTO [dbo].[CAD_EMPREENDIMENTO] " +
                    $"           ([CD_BancoDados] " +
                    $"           ,[CD_Mandante] " +
                    $"           ,[CD_Empreendimento] " +
                    $"           ,[CD_EmpreeSP7] " +
                    $"           ,[NM_Empree] " +
                    $"           ,[NM_EmpreeAbrev] " +
                    $"           ,[DS_Endereco] " +
                    $"           ,[NM_Bairro] " +
                    $"           ,[NM_Cidade] " +
                    $"           ,[NM_UF] " +
                    $"           ,[NR_CEP] " +
                    $"           ,[DT_Controle] " +
                    $"           ,[HR_Controle] " +
                    $"           ,[CD_EntidadeSP7] " +
                    $"           ,[CD_RegiaoSP7] " +
                    $"           ,[CD_TipoEmpreeSP7] " +
                    $"           ,[CD_MUNICIPIO] " +
                    $"           ,[CD_Padrao] " +
                    $"           ,[Boleto] " +
                    $"           ,[FichaFinanceira] " +
                    $"           ,[InformeRendimento] " +
                    $"           ,[Chamado] " +
                    $"           ,[idempresa]) " +
                    $"     VALUES " +
                    $"           (1 " +
                    $"           ,1 " +
                    $"           ,@codigoempreendimento " +
                    $"           ,@empreendimentosp7 " +
                    $"           ,@empreendimento' " +
                    $"           ,@abreviatura' " +
                    $"           ,@endereco' " +
                    $"           ,@bairro' " +
                    $"           ,@cidade' " +
                    $"           ,@estado' " +
                    $"           ,@cep' " +
                    $"           ,@datahoraatualizacao " +
                    $"           ,@horaatualizacao " +
                    $"           ,@entidade " +
                    $"           ,@regiao " +
                    $"           ,@tipoempreendimento " +
                    $"           ,@codigomunicipio " +
                    $"           ,1 " +
                    $"           ,1 " +
                    $"           ,1" +
                    $"           ,1 " +
                    $"           ,1 " +
                    $"           ,1) ";

            var parameters = new
            {
                codigoempreendimento = codigoempreendimento,
                empreendimentosp7 = obj.empreendimentosp7,
                empreendimento = obj.empreendimento,
                abreviatura = obj.abreviatura,
                endereco = obj.endereco,
                bairro = obj.bairro,
                cidade = obj.cidade,
                estado = obj.estado,
                cep = obj.cep,
                dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                entidade = obj.entidade,
                regiao = obj.regiao,
                tipoempreendimento = obj.tipoempreendimento,
                codigomunicipio = obj.codigomunicipio
            };            
            connection.ExecuteAsync(sql, parameters).Wait();
        }

        private void Alterar(EmpreendimentosIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = "" +
                    $"UPDATE [dbo].[CAD_EMPREENDIMENTO] " +
                    $"   SET [CD_EmpreeSP7] = @empreendimentosp7 " +
                    $"      ,[NM_Empree] = @empreendimento " +
                    $"      ,[NM_EmpreeAbrev] = @abreviatura " +
                    $"      ,[DS_Endereco] = @endereco " +
                    $"      ,[NM_Bairro] = @bairro " +
                    $"      ,[NM_Cidade] = @cidade " +
                    $"      ,[NM_UF] = @estado " +
                    $"      ,[NR_CEP] = @cep " +
                    $"      ,[DT_Controle] = @dataatualizacao " +
                    $"      ,[HR_Controle] = @horaatualizacao " +
                    $"      ,[CD_EntidadeSP7] = @entidade " +
                    $"      ,[CD_RegiaoSP7] = @regiao " +
                    $"      ,[CD_TipoEmpreeSP7] = @tipoempreendimento " +
                    $"      ,[CD_MUNICIPIO] = @codigomunicipio " +
                    $"      ,[CD_Padrao] = 1 " +
                    $" WHERE [CD_EmpreeSP7] = @empreendimentosp7 ";

            var parameters = new
            {
                empreendimentosp7 = obj.empreendimentosp7,
                empreendimento = obj.empreendimento,
                abreviatura = obj.abreviatura,
                endereco = obj.endereco,
                bairro = obj.bairro,
                cidade = obj.cidade,
                estado = obj.estado,
                cep = obj.cep,
                dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                entidade = obj.entidade,
                regiao = obj.regiao,
                tipoempreendimento = obj.tipoempreendimento,
                codigomunicipio = obj.codigomunicipio
            };

            connection.ExecuteAsync(sql, parameters).Wait();
        }

        private Boolean JaEstaCadastrado(EmpreendimentosIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"SELECT 1 FROM CAD_EMPREENDIMENTO WHERE CD_EmpreeSP7={obj.empreendimentosp7} ";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }
        private DateTime DataHoraDaUltimaIntegracao()
        {
            var connection = _conectionMSSQL;
            var sql = $"select DataUltimaIntegracao from CAD_INTEGRACAO where CD_Tabela=3 AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            return connection.QueryFirstOrDefault<DateTime>(sql);
        }
        private void AlterarDataHoraDaUltimaIntegracao(DateTime _DataHoraAtual)
        {
            var connection = _conectionMSSQL;
            var sql = $"UPDATE CAD_INTEGRACAO SET integrar=0, DataUltimaIntegracao='{_DataHoraAtual.ToString("yyyy-MM-dd HH:mm:ss")}' where CD_Tabela=3 AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            connection.ExecuteAsync(sql).Wait();
        }

    }
}
