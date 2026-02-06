using Dapper;
using poliview.crm.cessadireito.Repositorios;
using poliview.crm.cessadireito.Services;
using Poliview.crm.domain;
using System.Collections.Generic;
using System;

namespace poliview.crm.cessadireito
{
    public class IntegracaoContratos : IIntegracao
    {
        private DateTime _DataHoraUlimaIntegracao;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;

        public IntegracaoContratos(DateTime DataHoraUltimaIntegracao, 
                                   string connectionStringMssql, 
                                   string connectionStringFb,
                                   LogService logService
                                   )
        {
            _logService = logService;
            _DataHoraUlimaIntegracao = DataHoraUltimaIntegracao;
            _connectionStringFb = connectionStringFb;
            _connectionStringMssql = connectionStringMssql;
        }
        public bool Integrar()
        {
            _logService.Log(LogLevel.Information, $"CONTRATOS: inicio de integração {_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}").Wait();
            var registrosOrigem = this.ListarOrigem();
            _logService.Log(LogLevel.Information, $"CONTRATOS: foram encontrados {registrosOrigem.Count} registro para integração").Wait();
            if (registrosOrigem != null)
            {
                salvarDadosDestino(registrosOrigem);
            }
            _logService.Log(LogLevel.Information, "CONTRATOS: fim de integração").Wait();
            return true;
        }

        private List<ContratoIntegracao> ListarOrigem()
        {
            var connection = new FirebirdConnectionFactory(_connectionStringFb).CreateConnection();
            var sql = "SELECT FIRST 10 " +
                        "CTR_CDG AS contratosp7, CTR_FORNECEDOR as codigoclientesp7, CTR_EMPRE as codigoempresasp7, " +
                        "CTR_EMPRD as codigoempreendimentosp7, CTR_BLOCO as codigoblocosp7, CTR_UNDEMPRD as codigounidadesp7, " +
                        "CTR_STATUS as statuscontratosp7, CTR_STATUSDISTRATO as statusdistrato, CTR_REMANEJADO as statusremanejado, " +
                        "CTR_CNPJCPFRESP as cpfresponsavel, CTR_NOMERESP as nomeresponsavel, CTR_TELEFONERESP as telefoneresponsavel, " +
                        "CTR_CELULARRESP as celularresponsavel, CTR_EMAILRESP as emailresponsavel, CTR_DTLIDOCRM as datahoraultimaatualizacao " +
                        "FROM EMP_CTR " +
                        $"WHERE CTR_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<ContratoIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<ContratoIntegracao> dadosorigem)
        {
            foreach (var contrato in dadosorigem)
            {
                if (JaEstaCadastrado(contrato))
                {
                    Alterar(contrato);
                }
                else
                {
                    Incluir(contrato);
                }
            }
        }

        private void Incluir(ContratoIntegracao obj)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = "INSERT INTO [dbo].[CAD_CONTRATO] " +
            "           ([CD_BancoDados] " +
            "           ,[CD_Mandante] " +
            "           ,[CD_ContratoSP7] " +
            "           ,[CD_ClienteSP7] " +
            "           ,[CD_EmpresaSP7] " +
            "           ,[CD_EmpreeSP7] " +
            "           ,[CD_BlocoSP7] " +
            "           ,[NR_UnidadeSP7] " +
            "           ,[IN_StatusSP7] " +
            "           ,[IN_StatusDistratoSP7] " +
            "           ,[IN_StatusRemanejadoSP7] " +
            "           ,[DT_Controle] " +
            "           ,[HR_Controle] " +
            "           ,[In_StatusIntegracao] " +
            "           ,[CTR_CNPJCPFRESP] " +
            "           ,[CTR_NOMERESP] " +
            "           ,[CTR_TELEFONERESP] " +
            "           ,[CTR_CELULARRESP] " +
            "           ,[CTR_EMAILRESP] " +
            "           ,[DTVENDARESP]) " +
            "     VALUES " +
            "           (1 " +
            "           ,1 " +
            $"           ,{obj.codigocontrato} " +
            $"           ,'{obj.codigoclientesp7}'" +
            $"           ,{obj.codigoempresasp7} " +
            $"           ,{obj.codigoempreendimentosp7} " +
            $"           ,{obj.codigoblocosp7} " +
            $"           ,'{obj.codigounidadesp7}' " +
            $"           ,{obj.statuscontratosp7} " +
            $"           ,{obj.statusdistrato}" +
            $"           ,{obj.statusremanejado} " +
            $"           ,'{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}' " +
            $"           ,'{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
            $"           ,0 " +
            $"           ,'{obj.cpfresponsavel}'" +
            $"           ,'{obj.nomeresponsavel}'" +
            $"           ,'{obj.telefoneresponsavel}'" +
            $"           ,'{obj.celularresponsavel}'" +
            $"           ,'{obj.emailresponsavel}' " +
            $"           ,null) ";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogLevel.Information, $"CONTRATO: incluido registro. Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
        }

        private void Alterar(ContratoIntegracao obj)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = $"UPDATE [dbo].[CAD_CONTRATO] " +
                    $"   SET [CD_BancoDados] = 1 " +
                    $"      ,[CD_Mandante] = 1 " +
                    $"      ,[CD_ContratoSP7] = '{obj.contratosp7}' " +
                    $"      ,[CD_ClienteSP7] = '{obj.codigoclientesp7}' " +
                    $"      ,[CD_EmpresaSP7] = {obj.codigoempresasp7} " +
                    $"      ,[CD_EmpreeSP7] = {obj.codigoempreendimentosp7} " +
                    $"      ,[CD_BlocoSP7] = {obj.codigoblocosp7} " +
                    $"      ,[NR_UnidadeSP7] = {obj.codigounidadesp7} " +
                    $"      ,[IN_StatusSP7] = {obj.statuscontratosp7} " +
                    $"      ,[IN_StatusDistratoSP7] = {obj.statusdistrato} " +
                    $"      ,[IN_StatusRemanejadoSP7] = {obj.statusremanejado} " +
                    $"      ,[DT_Controle] = '{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}' " +
                    $"      ,[HR_Controle] = '{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
                    $"      ,[In_StatusIntegracao] = '{obj.statusintegracao}' " +
                    $"      ,[CTR_CNPJCPFRESP] = '{obj.cpfresponsavel}' " +
                    $"      ,[CTR_NOMERESP] = '{obj.nomeresponsavel}' " +
                    $"      ,[CTR_TELEFONERESP] = '{obj.telefoneresponsavel}' " +
                    $"      ,[CTR_CELULARRESP] = '{obj.celularresponsavel}' " +
                    $"      ,[CTR_EMAILRESP] = '{obj.emailresponsavel}' " +
                    $"      ,[DTVENDARESP] = null " +
                    $" WHERE CD_ContratoSP7={obj.contratosp7} "; 
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogLevel.Information, $"CONTRATO: alterado registro. Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
        }

        private Boolean JaEstaCadastrado(ContratoIntegracao contrato)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = $"select * from CAD_CONTRATO WHERE CD_ContratoSP7='{contrato.contratosp7}'";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }

    }
}
