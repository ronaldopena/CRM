using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class IntegracaoClienteService
    {
        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        private FbConnection _conectionFB;
        private SqlConnection _conectionMSSQL;
        public IntegracaoClienteService(string _connectionStringFb, string _connectionStringMssql)
        {           
            _conectionFB = new FirebirdConnectionService(_connectionStringFb).CreateConnection();
            _conectionMSSQL = new SqlServerConnectionService(_connectionStringMssql).CreateConnection();
        }

        public void Integrar(string cpf)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();            
            var registrosOrigem = this.ListarOrigem(cpf);
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

        private void ExcluirRegistro(string cliente)
        {
            var connection = _conectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_CLIENTE] WHERE CD_ClienteSP7='{cliente}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private int ProximoCodigoCliente()
        {
            var connection = _conectionMSSQL;
            var sql = $"SELECT COALESCE(MAX(CD_CLIENTE)+1,1) as codigo FROM [dbo].[CAD_CLIENTE]";
            var proximo = connection.Query<int>(sql).ToList();
            return proximo[0];
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _conectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO WHERE TABELA='CLIENTES' and CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _conectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='CLIENTES' and chave is not null ";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private void DeletarOrigemExclusaoNulo()
        {
            var connection = _conectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where CHAVE is null";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ClientesIntegracao> ListarOrigem(string cpf)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                           "FORN_CNPJ as codigoclientesp7, FORN_RAZAO as nome, FORN_CPFCNPJ as cpfcnpj, FORN_EMAIL as email, " +
                           "FORN_TPLOGRADOURORES||' '||FORN_LOGRADOURORES||', '||FORN_NUMERORES AS endereco, " +
                           "FORN_BAIRRORES AS bairro, FORN_CIDRES AS cidade, FORN_UF as estado, FORN_CEPRES AS cep, " +
                           "FORN_DDDCEL1 as ddd, FORN_TELEFONE1 as telefone, FORN_CELULAR1 as celular, FORN_CONTATO1 as contato, FORN_DTNASCIMENTO as datanascimento, " +
                           "FORN_DTLIDOCRM as datahoraultimaatualizacao " +
                           "FROM CADCPG_FORNECEDOR " +
                           $"WHERE FORN_TPCLIENTE=1 AND FORN_CPFCNPJ='{cpf}'";
            return connection.Query<ClientesIntegracao>(sql).ToList();
        }

        private List<ClientesIntegracao> ListarOrigem(DateTime _DataHoraUlimaIntegracao)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                           "FORN_CNPJ as codigoclientesp7, FORN_RAZAO as nome, FORN_CPFCNPJ as cpfcnpj, FORN_EMAIL as email, " +
                           "FORN_TPLOGRADOURORES||' '||FORN_LOGRADOURORES||', '||FORN_NUMERORES AS endereco, " +
                           "FORN_BAIRRORES AS bairro, FORN_CIDRES AS cidade, FORN_UF as estado, FORN_CEPRES AS cep, " +
                           "FORN_DDD as ddd, FORN_TELEFONE1 as telefone, FORN_CELULAR1 as celular, FORN_CONTATO1 as contato, FORN_DTNASCIMENTO as datanascimento, " +
                           "FORN_DTLIDOCRM as datahoraultimaatualizacao " +
                           "FROM CADCPG_FORNECEDOR " +
                           $"WHERE FORN_TPCLIENTE=1 AND FORN_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<ClientesIntegracao>(sql).ToList();
        }


        private void salvarDadosDestino(List<ClientesIntegracao> dadosorigem)
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

                AlterarEmailOpeUsuario(item);
                AlterarEmailCadCliente(item);
            }
        }

        private void Incluir(ClientesIntegracao obj)
        {
            var sql = "";

            if (obj.endereco != null)
            {
                if (obj.endereco == " , ") obj.endereco = "";
            }

            var dtnascimento = obj.datanascimento.ToString("yyyy-MM-dd");
            dtnascimento = dtnascimento == "0001-01-01" ? null : dtnascimento;
            var codigocliente = ProximoCodigoCliente();
            var connection = _conectionMSSQL;
            sql = "set dateformat YMD; " +
            $"INSERT INTO [dbo].[CAD_CLIENTE] " +
            $"           ([CD_BancoDados] " +
            $"           ,[CD_Mandante] " +
            $"           ,[CD_ClienteSP7] " +
            $"           ,[CD_Cliente] " +
            $"           ,[NM_Cliente] " +
            $"           ,[NM_ClienteFantasia] " +
            $"           ,[NR_CPFCNPJ] " +
            $"           ,[DS_Email] " +
            $"           ,[DS_Endereco] " +
            $"           ,[NM_Bairro] " +
            $"           ,[NM_Cidade] " +
            $"           ,[NM_UF] " +
            $"           ,[NR_CEP] " +
            $"           ,[NR_DDD] " +
            $"           ,[NR_Telefone] " +
            $"           ,[NR_Celular] " +
            $"           ,[NM_Contato] " +
            $"           ,[DT_Controle] " +
            $"           ,[HR_Controle] " +
            $"           ,[IN_ContratoResp] " +
            $"           ,[Data_Nascimento]) " +
            $"     VALUES " +
            $"           (1 " +
            $"           ,1 " +
            $"           ,@codigoclientesp7 " +
            $"           ,@codigocliente " +
            $"           ,@nome " +
            $"           ,@nome " +
            $"           ,@cpfcnpj " +
            $"           ,@email " +
            $"           ,@endereco " +
            $"           ,@bairro " +
            $"           ,@cidade " +
            $"           ,@estado " +
            $"           ,@cep " +
            $"           ,@ddd " +
            $"           ,@telefone  " +
            $"           ,@celular " +
            $"           ,@contato " +
            $"           ,@dataatualizacao " +
            $"           ,@horaatualizacao " +
            $"           ,0 " +
            $"           ,@datanascimento); ";

            var parameters = new
            {
                codigoclientesp7 = obj.codigoclientesp7,
                codigocliente = codigocliente,
                nome = obj.nome,
                cpfcnpj = obj.cpfcnpj,
                email = obj.email,
                endereco = obj.endereco,
                bairro = obj.bairro,
                cidade = obj.cidade,
                estado = obj.estado,
                cep = obj.cep,
                ddd = obj.ddd,
                telefone = obj.telefone,
                celular = obj.celular,
                contato = obj.contato,
                dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                datanascimento = dtnascimento
            };

            connection.ExecuteAsync(sql, parameters).Wait();
        }

        private clientesConsulta ConsultarDadosCliente(string codigoclientesp7)
        {
            var connection = _conectionMSSQL;
            var sql = $"select coalesce(DS_Email,'') as email, coalesce(NR_DDD,'') as ddd, coalesce(NR_Telefone,'') as telefone, coalesce(NR_Celular,'') as celular from CAD_CLIENTE where CD_CLIENTESP7='{codigoclientesp7}'";
            return connection.QueryFirst<clientesConsulta>(sql);
        }

        private void Alterar(ClientesIntegracao obj)
        {
            var connection = _conectionMSSQL;

            var dadosatuaiscliente = this.ConsultarDadosCliente(obj.codigoclientesp7);

            if (obj.endereco != null)
            {
                if (obj.endereco == " , ") obj.endereco = "";
            }

            if (obj.ddd == null) obj.ddd = "";

            if (String.IsNullOrEmpty(obj.email)) obj.email = dadosatuaiscliente.email;
            if (String.IsNullOrEmpty(obj.ddd))
            {
                if (!String.IsNullOrEmpty(dadosatuaiscliente.ddd))
                    obj.ddd = dadosatuaiscliente.ddd;
                else
                    obj.ddd = "";
            }

            if (String.IsNullOrEmpty(obj.telefone)) obj.telefone = dadosatuaiscliente.telefone;
            if (String.IsNullOrEmpty(obj.celular)) obj.celular = dadosatuaiscliente.celular;

            if (obj.ddd.Length > 2) obj.ddd = obj.ddd.Substring(0, 2);

            var sql = "SET DATEFORMAT YMD; ";

            sql += $"UPDATE [dbo].[CAD_CLIENTE] ";
            sql += $"   SET [NM_Cliente] = @nome ";
            sql += $"      ,[NM_ClienteFantasia] = @nome ";
            sql += $"      ,[NR_CPFCNPJ] = @cpfcnpj ";
            sql += $"      ,[DS_Email] = @email ";
            sql += $"      ,[DS_Endereco] = @endereco ";
            sql += $"      ,[NM_Bairro] = @bairro ";
            sql += $"      ,[NM_Cidade] = @cidade  ";
            sql += $"      ,[NM_UF] = @estado ";
            sql += $"      ,[NR_CEP] = @cep ";
            sql += $"      ,[NR_DDD] = @ddd ";
            sql += $"      ,[NR_Telefone] = @telefone ";
            sql += $"      ,[NR_Celular] = @celular ";
            sql += $"      ,[NM_Contato] = @contato ";
            sql += $"      ,[DT_Controle] = @dataatualizacao ";
            sql += $"      ,[HR_Controle] = @horaatualizacao ";
            if (obj.datanascimento.Year > 1900) sql += $"      ,[Data_Nascimento] = @datanascimento ";
            sql += $" WHERE [CD_ClienteSP7]=@codigoclientesp7 ";

            var parameters = new
            {
                codigoclientesp7 = obj.codigoclientesp7,
                nome = obj.nome,
                cpfcnpj = obj.cpfcnpj,
                email = obj.email,
                endereco = obj.endereco,
                bairro = obj.bairro,
                cidade = obj.cidade,
                estado = obj.estado,
                cep = obj.cep,
                ddd = obj.ddd,
                telefone = obj.telefone,
                celular = obj.celular,
                contato = obj.contato,
                dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                datanascimento = obj.datanascimento.ToString("yyyy-MM-dd")
            };

            connection.ExecuteAsync(sql, parameters).Wait();
        }

        private Boolean JaEstaCadastrado(ClientesIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"select 1 from CAD_CLIENTE where NR_CPFCNPJ='{obj.cpfcnpj}'";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }

        private void AlterarEmailOpeUsuario(ClientesIntegracao obj)
        {
            if (!String.IsNullOrEmpty(obj.email))
            {
                var connection = _conectionMSSQL;
                var sql = $"update OPE_USUARIO set DS_Email='{obj.email}' where NR_CPFCNPJ='{obj.cpfcnpj}'";
                connection.ExecuteAsync(sql).Wait();
            }
        }

        private void AlterarEmailCadCliente(ClientesIntegracao obj)
        {
            if (!String.IsNullOrEmpty(obj.email))
            {
                var connection = _conectionMSSQL;
                var sql = $"update CAD_CLIENTE set DS_Email='{obj.email}' where NR_CPFCNPJ='{obj.cpfcnpj}'";
                connection.ExecuteAsync(sql).Wait();
            }
        }

        private List<validacao> ListarSiecon()
        {
            var connection = _conectionFB;
            var sql = "SELECT FORN_CNPJ as codigoclientesp7 FROM CADCPG_FORNECEDOR WHERE FORN_TPCLIENTE=1 ";
            return connection.Query<validacao>(sql).ToList();
        }

        private List<validacao> ListarCrm()
        {
            var connection = _conectionMSSQL; ;
            var sql = "SELECT CD_CLIENTESP7 as codigoclientesp7 FROM CAD_CLIENTE ";
            return connection.Query<validacao>(sql).ToList();
        }

        private DateTime DataHoraDaUltimaIntegracao()
        {
            var connection = _conectionMSSQL;
            var sql = $"select DataUltimaIntegracao from CAD_INTEGRACAO where CD_Tabela=1 AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            return connection.QueryFirstOrDefault<DateTime>(sql);
        }

        private void AlterarDataHoraDaUltimaIntegracao(DateTime _DataHoraAtual)
        {
            var connection = _conectionMSSQL;
            var sql = $"UPDATE CAD_INTEGRACAO SET integrar=0, DataUltimaIntegracao='{_DataHoraAtual.ToString("yyyy-MM-dd HH:mm:ss")}' where CD_Tabela=1 AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            connection.ExecuteAsync(sql).Wait();
        }

        public string RetornaCodigoCliente(string cpf)
        {
            var connection = _conectionFB;
            var sql = "SELECT FORN_CNPJ as codigoclientesp7 FROM CADCPG_FORNECEDOR " +
                           $"WHERE FORN_TPCLIENTE=1 AND FORN_CPFCNPJ='{cpf}'";
            var registro = connection.Query<ClientesIntegracao>(sql).ToList();

            if (registro.Count == 0) return "";

            return registro[0].codigoclientesp7;
        }


        private string retornaString(string str, int tamanho)
        {
            if (str == null) return "";

            str = str.Replace("'", "''");

            if (str.Length > tamanho) return str.Substring(0, tamanho);

            return str;
        }
    }
}
