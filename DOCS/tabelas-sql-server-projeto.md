# Tabelas e views do projeto no SQL Server

Lista de **tabelas e views** referenciadas no código da solução SieconCRM (Poliview CRM).  
A consulta direta ao banco via MCP falhou; esta lista foi obtida por análise estática do repositório.

---

## Tabelas OPE_ (Operação / CRM)

| Tabela / View | Uso principal no projeto |
|---------------|---------------------------|
| **OPE_EMAIL** | E-mails (envio/recebimento): fila, status, corpo, assunto, destinatário, chamado. |
| **OPE_EMAIL_ARQUIVOS** | Vínculo e-mail ↔ anexos (ID_EMAIL, ID_ARQUIVO, CD_AVISO). |
| **OPE_EMAIL_QUARENTENA** | E-mails em quarentena. |
| **OPE_EMAIL_PROCESSO** | Controle de processo por conta (recebendoEmails, enviandoEmails, idcontaemail). |
| **OPE_PARAMETRO** | Parâmetros gerais (TipoAutenticacaoEmail, TamanhoMaximoAnexos, CD_BancoDados, CD_Mandante, etc.). |
| **OPE_CHAMADO** | Chamados (CD_CHAMADO, ID_STATUS, NovasMensagens, CD_Cliente, CD_Empreendimento). |
| **OPE_CHAMADO_DET** | Ocorrências/detalhes do chamado (CD_Chamado, CD_Ocorrencia, CD_UsuRecurso, id_status). |
| **OPE_CHAMADO_DET_AVISOS** | Avisos por ocorrência (assunto, descrição, e-mail, CD_AVISO). |
| **OPE_CHAMADO_DET_LOG** | Log da ocorrência (DS_DESCRICAO, CD_AVISO). |
| **OPE_CHAMADO_ANEXO** | Anexos do chamado (CD_Anexo). |
| **OPE_ARQUIVOS** | Arquivos (ID) usados em anexos. |
| **OPE_USUARIO** | Usuários do sistema (CD_Usuario, DS_Email, NR_CPFCNPJ, etc.). |
| **OPE_USUARIO_EMAIL** | E-mail por usuário/empresa (CD_Usuario, idempresa). |
| **OPE_GRUPO** | Grupos (in_status). |
| **OPE_ACESSOS** | Acessos (idusuario, idorigem, senhapadrao). |
| **OPE_CONFIG** | Configuração por chave (chaveacesso). |
| **OPE_SERVICOS** | Serviços (Ativo). |
| **OPE_SERVICOS_MONITORADOS** | Serviços monitorados (NomeServico, chave, dataUltimoProcessamento – ex.: 'EMAIL'). |
| **OPE_CONTRATO** | Contratos operacionais (CD_Contrato, CD_Cliente, CD_Empreendimento, etc.). |
| **OPE_PARAMETROS** | Parâmetros adicionais (caminhoPDF, urlExternaHTML, caminhoHTML) – instalador. |

---

## Tabelas CAD_ (Cadastro)

| Tabela | Uso principal no projeto |
|--------|---------------------------|
| **CAD_CONTA_EMAIL** | Contas de e-mail (id, nomeRemetente, emailRemetente – usado pelo serviço de e-mail). |
| **CAD_CLIENTE** | Clientes (CD_Cliente, CD_ClienteSP7, DS_Email, NR_CPFCNPJ, etc.). |
| **CAD_CONTRATO** | Contratos (CD_ContratoSP7, CD_EmpreeSP7, CD_BlocoSP7, NR_UnidadeSP7, etc.). |
| **CAD_EMPREENDIMENTO** | Empreendimentos (CD_EmpreeSP7, CD_Empreendimento, idempresa). |
| **CAD_EMPRESA** | Empresas (id, idcontaemail). |
| **CAD_BLOCO** | Blocos (CD_BlocoSP7, CD_EmpreeSP7). |
| **CAD_UNIDADE** | Unidades (cd_unidade, CD_EmpreeSP7, CD_BlocoSP7, nr_unidadesp7, tipo). |
| **CAD_TIPO_UNIDADE** | Tipo de unidade (id, descricao, espacocliente). |
| **CAD_PROPONENTE** | Proponentes (CD_ContratoSP7, CD_ClienteSP7, ativo). |
| **CAD_INTEGRACAO** | Controle de integração (CD_Tabela, DataUltimaIntegracao, CD_BancoDados, CD_Mandante). |
| **CAD_ORIGEM_CHAMADO** | Origem do chamado (id, descricao, loginhabilitado). |

---

## Tabelas CRM_ (Procedures / exclusão)

| Nome | Tipo | Uso |
|------|------|-----|
| **CRM_EXCLUSAO** | Tabela | Fila de exclusões por TABELA e CHAVE (CLIENTES, BLOCOS, UNIDADES, CONTRATOS, PROPONENTES, EMPREENDIMENTOS). |
| **CRM_EMAILS_PENDENTE_ENVIO** | Procedure | Lista e-mails pendentes de envio por conta. |
| **CRM_EMAILS_PENDENTE_PROCESSAMENTO** | Procedure | Lista e-mails pendentes de processamento. |
| **CRM_Marca_Email_Com_Erro** | Procedure | Marca e-mail com erro de envio. |
| **CRM_Email_Enviado_Chamado_Concluido** | Procedure | Processa e-mail de chamado concluído. |
| **CRM_Email_Enviado_Chamado_Cancelado** | Procedure | Processa e-mail de chamado cancelado. |
| **CRM_LISTAR_ANEXOS_EMAIL** | Procedure | Lista anexos de um e-mail. |
| **CRM_Excluir_Anexo** | Procedure | Exclui anexo. |
| **CRM_Troca_Campos** | Função | Troca placeholders no corpo (chamado). |
| **CRM_Listar_Historico** | Procedure | Histórico do chamado. |
| **CRM_Listar_Historico_Email** | Procedure | Histórico de e-mail do chamado. |
| **CRM_API_ENVIAR_EMAIL** | Procedure | Envio de e-mail via API (ex.: aviso de tamanho máximo de anexo). |

---

## Views

| View | Uso principal |
|------|----------------|
| **vListaChamados** | Listagem de chamados (API_LISTAR_CHAMADOS / API_LISTAR_CHAMADO). |
| **vUsuariosGrupo** | Usuários por grupo (CD_Grupo, in_supervisor, in_master, IN_Status, idcontaemail). |

---

## Tabelas de integração / outros sistemas (CADCPG, CADDVS, EMP, CRB)

| Tabela | Uso |
|--------|-----|
| **CADCPG_FORNECEDOR** | Fornecedores (FORN_CNPJ, FORN_TPCLIENTE). |
| **CADDVS_BLOCO** | Blocos (sistema DVS). |
| **CADDVS_EMPREEND** | Empreendimentos (sistema DVS). |
| **EMP_CTR** | Contratos (empresa). |
| **EMP_PROPONENTE** | Proponentes (empresa). |
| **EMP_UNDEMPRD** | Unidades empreendimento. |
| **EMP_TIPO** | Tipo (TIPO_CDG, TIPO_DESC). |
| **CRB_CESSAO** | Cessão. |
| **CRB_RECTOBAIXA** | Baixa de recibo. |

---

## Procedures / funções de API e outros

| Nome | Uso |
|------|-----|
| **API_LISTAR_CHAMADO** | Detalhe de chamado. |
| **API_LISTAR_CHAMADOS** | Lista de chamados. |
| **API_DATA_CONTROLE** | Formatação de data/hora. |
| **S_CRB_CALCRECTOCOMPEMPRD** | Cálculo de recibo (table-valued). |
| **S_CRB_CALCRECTOEFETUADO** | Recibo efetuado. |
| **s_portal_parametros** | Parâmetros portal (boletos – Firebird?). |
| **CRM_Executar_Tarefas** | Tarefas agendadas (instalador/serviço). |

---

## Catálogo SQL Server (referenciado no instalador)

| Objeto | Uso |
|--------|-----|
| **INFORMATION_SCHEMA.COLUMNS** | Metadados de colunas (ex.: verificação de OPE_PARAMETRO). |
| **sys.sysusers** | Usuários do banco. |
| **sys.database_principals** | Principais do banco. |
| **sys.database_role_members** | Membros de roles. |

---

## Resumo por projeto relevante

- **poliview.crm.service.email:** OPE_EMAIL, OPE_EMAIL_ARQUIVOS, OPE_EMAIL_PROCESSO, OPE_PARAMETRO, OPE_CHAMADO, OPE_CHAMADO_DET, OPE_CHAMADO_DET_AVISOS, OPE_CHAMADO_DET_LOG, OPE_SERVICOS_MONITORADOS, CAD_CONTA_EMAIL + procedures CRM_* de e-mail.
- **Poliview.crm.services:** Todas as OPE_* e CAD_* listadas acima, views vListaChamados e vUsuariosGrupo, procedures API_* e CRM_*.
- **Poliview.crm.integracao:** CRM_EXCLUSAO, CAD_*, CADCPG_*, CADDVS_*, EMP_*, CRB_*, CAD_INTEGRACAO.
- **poliview.crm.instalador:** OPE_PARAMETRO, OPE_SERVICOS_MONITORADOS, OPE_PARAMETROS, sys.*, INFORMATION_SCHEMA.

Para obter a lista **diretamente do banco** (incluindo tabelas não referenciadas no código), use no SQL Server:

```sql
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME;
```

*Documento gerado por análise estática do repositório. Ajustar conforme schema real do banco.*
