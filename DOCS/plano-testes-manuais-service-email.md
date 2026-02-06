# Plano de testes manuais – poliview.crm.service.email

Este documento descreve os testes manuais a serem executados no serviço de e-mail **poliview.crm.service.email** para cobrir todas as possibilidades de **envio** e **recebimento** de e-mails, conforme os três provedores (POP/SMTP, Office 365, Gmail) e os fluxos do Worker.

---

## 1. Visão geral do serviço

- **Provedores (tipos de conta):**
  - **0 – Padrão (POP/SMTP):** envio via SMTP (MailKit), recebimento via POP3.
  - **1 – Office 365:** envio e recebimento via Microsoft Graph API.
  - **2 – Gmail:** envio e recebimento via Google Gmail API.

- **Fluxo do Worker (por ciclo):**
  1. Configurar envio/recebimento (EmailService), reset e listar contas.
  2. **ENVIAEMAIL:** para cada conta com `enviaremail = true`, buscar pendentes e enviar pelo provedor da conta.
  3. **RECEBEEMAIL:** para cada conta com `receberemail = true`, se não houver processo de recebimento ativo, baixar mensagens e processar (ProcessarEmailsRecebidos).
  4. Salvar data/hora do último processamento.

- **Pré-requisitos para testes:** base de dados com tabelas de e-mail e contas configuradas; appsettings com `conexao`, `conexaosqlite`, `Telegram` (opcional), `cliente`; contas de e-mail de teste por tipo (0, 1, 2) conforme ambiente.

---

## 2. Escopo dos testes

| Área            | Envio | Recebimento |
|-----------------|-------|-------------|
| Provedor Padrão (0) | Sim   | Sim         |
| Provedor Office 365 (1) | Sim | Sim         |
| Provedor Gmail (2)     | Sim | Sim         |
| Múltiplas contas      | Sim | Sim         |
| Flags enviaremail/receberemail | Sim | Sim |
| Cenários de conteúdo (anexo, variáveis, etc.) | Sim | — |
| Processamento (confirmação, chamado, replicar) | — | Sim |
| Tratamento de erro e notificação | Sim | Sim |

---

## 3. Testes de ENVIO

### 3.1 Por tipo de provedor

Executar com **uma conta ativa** de cada tipo; as demais contas podem estar inativas (`enviaremail = false`) para isolar o provedor.

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| E1 | Envio – conta **Padrão (POP/SMTP)** | Conta com `tipoconta = 0`, `enviaremail = true`, host/porta SMTP válidos; ao menos 1 e-mail pendente na fila. | 1. Iniciar o worker. 2. Aguardar um ciclo. | E-mail enviado pelo SMTP; log sem erro; registro marcado como enviado na base. |
| E2 | Envio – conta **Office 365** | Conta com `tipoconta = 1`, `enviaremail = true`, tenant/client/user/secret válidos; ao menos 1 e-mail pendente. | 1. Iniciar o worker. 2. Aguardar um ciclo. | E-mail enviado via Graph; log sem erro; registro marcado como enviado. |
| E3 | Envio – conta **Gmail** | Conta com `tipoconta = 2`, `enviaremail = true`, credenciais OAuth/API válidas; ao menos 1 e-mail pendente. | 1. Iniciar o worker. 2. Aguardar um ciclo. | E-mail enviado via Gmail API; log sem erro; registro marcado como enviado. |

### 3.2 Conteúdo do e-mail (a testar em ao menos um provedor)

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| E4 | Assunto com **ID do e-mail** | E-mail pendente com assunto sem `<#id#>` | Enviar ciclo. | Assunto no destino contém `<#id#>`; corpo/assunto salvos (SalvarCorpoEAssunto). |
| E5 | **RECUPERAÇÃO DE SENHA** | E-mail pendente com `assunto = "RECUPERAÇÃO DE SENHA"` e corpo com link `<a href=...>` | Enviar ciclo. | Corpo enviado no formato específico (link “aqui”); envio concluído. |
| E6 | **Variáveis no assunto/corpo** | E-mail pendente com placeholders (ex.: [CHAMADO], [CLIENTE], [NOME_CLIENTE]) e `idchamado`/`idocorrencia` válidos | Enviar ciclo. | Variáveis substituídas no assunto e no corpo. |
| E7 | Anexo por **urlanexo** | E-mail pendente com `urlanexo` apontando para arquivo existente no servidor | Enviar ciclo. | E-mail recebido com anexo; sem erro de “arquivo não encontrado”. |
| E8 | Anexo por **urlanexo inexistente** | E-mail pendente com `urlanexo` para caminho que não existe | Enviar ciclo. | Erro registrado (ex.: “arquivo não encontrado”); e-mail marcado com erro (MarcarEmailComErro); notificação Telegram (se configurada). |
| E9 | Anexos por **ListarAnexosEmail** | E-mail pendente sem urlanexo, com anexos gravados na base (listados por EmailService.ListarAnexosEmail) | Enviar ciclo. | E-mail recebido com anexos; envio concluído. |
| E10 | **Múltiplos destinatários** | E-mail pendente com `emaildestinatario` contendo vários e-mails | Enviar ciclo. | Todos recebem; To/Cc conforme regra (primeiro To, demais Cc). |
| E11 | **E-mail “em nome de” (suporte)** | Conta com `emaildestinatariosuporte` preenchido; e-mail pendente para outro destinatário | Enviar ciclo. | Envio para emaildestinatariosuporte; assunto com “enviado para: ... em nome de: destinatário”. |
| E12 | **Chamado e marcação de mensagens** | E-mail pendente com `idchamado` e destinatário igual ao e-mail do cliente do chamado | Enviar ciclo. | E-mail enviado; ChamadoService.MarcarMensagensChamado chamado; EmailService.MarcarEmailComoEnviado. |

### 3.3 Múltiplas contas e flags

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| E13 | **Várias contas com envio ativo** | Duas ou mais contas com `enviaremail = true` (tipos podem diferir) | Iniciar worker; aguardar ciclo. | Cada conta processa sua fila de pendentes; logs indicam envio por conta. |
| E14 | **Conta com envio desabilitado** | Ao menos uma conta com `enviaremail = false` | Iniciar worker; aguardar ciclo. | Log “ENVIO DE EMAILS DESABILITADO” para essa conta; nenhum envio para ela. |
| E15 | **Nenhum e-mail pendente** | Contas ativas, fila vazia | Iniciar worker; aguardar ciclo. | Log “total de emails para enviar: 0”; sem exceção; ciclo termina. |

### 3.4 Envio avulso (EnviarEmailAvulsoAsync)

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| E16 | **E-mail avulso – Padrão** | Primeira conta da lista é Padrão (0); chamada programática ou via ponto de entrada que use EnviarEmailAvulsoAsync | Chamar EnviarEmailAvulsoAsync(destinatarios, assunto, corpo, log) com dados válidos. | E-mail recebido no(s) destinatário(s); log de sucesso. |
| E17 | **E-mail avulso – Office 365** | Primeira conta é Office 365 (1) | Idem E16. | E-mail recebido via Graph. |
| E18 | **E-mail avulso – Gmail** | Primeira conta é Gmail (2) | Idem E16. | E-mail recebido via Gmail API. |

### 3.5 Erros e notificação

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| E19 | **Credenciais SMTP inválidas (Padrão)** | Conta Padrão com usuário/senha ou host/porta incorretos | Iniciar worker com 1 pendente. | Exceção; MarcarEmailComErro; notificação Telegram (INotificacaoErro); log de erro. |
| E20 | **Credenciais Office 365 inválidas** | Conta O365 com tenant/client/secret/userId inválidos | Idem. | Exceção; notificação; e-mail marcado com erro. |
| E21 | **Credenciais Gmail inválidas** | Conta Gmail com token/credenciais expirados ou inválidos | Idem. | Exceção; notificação; e-mail marcado com erro. |
| E22 | **Tipo de conta inválido** | Registro de conta com `tipoconta` diferente de 0, 1 ou 2 (ex.: 3) | Iniciar worker com essa conta ativa e pendente. | ArgumentException (factory); notificação de erro; log. |

---

## 4. Testes de RECEBIMENTO

### 4.1 Por tipo de provedor

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| R1 | Recebimento – conta **Padrão (POP3)** | Conta com `tipoconta = 0`, `receberemail = true`, host/porta POP válidos; caixa com ao menos 1 e-mail não lido. | Iniciar worker; aguardar ciclo. | Mensagens baixadas; gravadas na base; ProcessarEmailsRecebidos executado; log sem erro. |
| R2 | Recebimento – conta **Office 365** | Conta com `tipoconta = 1`, `receberemail = true`; caixa com mensagens. | Idem. | Mensagens baixadas via Graph; processamento executado. |
| R3 | Recebimento – conta **Gmail** | Conta com `tipoconta = 2`, `receberemail = true`; caixa com mensagens. | Idem. | Mensagens baixadas via Gmail API; processamento executado. |

### 4.2 Concorrência e controle de processo

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| R4 | **RecebendoEmails = 0** | Nenhum processo de recebimento ativo para a conta | Iniciar worker. | IniciarReceberEmails; ReceiveEmailAsync; PararReceberEmails; Processar. |
| R5 | **Já existe processo de recebimento** | Simular ou aguardar cenário em que RecebendoEmails(conta.id) > 0 (outro processo ativo) | Segundo ciclo na mesma conta antes do primeiro terminar (ou simulação). | Log “JÁ EXISTE UM PROCESSO DE RECEBIMENTO DE EMAIL”; não inicia novo ReceiveEmailAsync. |

### 4.3 Processamento (ProcessarEmailsRecebidos)

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| R6 | **E-mail de confirmação de entrega (aceito)** | E-mail baixado identificado como confirmação (EmailDeConfirmacaoEntrega = 1) | Enviar e-mail que gere confirmação; rodar recebimento. | MarcarEmailComoConfirmadoProvedor; Excluir e-mail; log “CONFIRMAÇÃO EMAIL”. |
| R7 | **E-mail de confirmação de entrega (recusado)** | E-mail de confirmação recusada (EmailDeConfirmacaoEntrega = 2) | Idem. | MarcarEmailComoRecusadoProvedor; Excluir; log. |
| R8 | **E-mail vinculado a chamado (CRM)** | E-mail baixado identificado como do CRM (EmailCrm); não confirmacao; não chamado cancelado/concluído | Enviar resposta de e-mail vinculada a chamado; rodar recebimento. | ReplicarEmailParaEnvolvidosComChamado; MarcarEmailComoProcessado. |
| R9 | **E-mail para chamado cancelado** | E-mail que VerificaEmailParaChamadoCancelado retorna true | Configurar chamado cancelado; enviar e-mail para a thread; rodar recebimento. | E-mail excluído; log “EMAIL EXCLUIDO”. |
| R10 | **E-mail para chamado concluído** | E-mail que VerificaEmailParaChamadoConcluido retorna true | Idem para chamado concluído. | E-mail excluído. |
| R11 | **E-mail sem vínculo CRM / não processável** | E-mail que não é confirmação, não é CRM, ou é para chamado cancelado/concluído | Enviar e-mail que se enquadre. | Excluir; log “EMAIL EXCLUIDO”. |

### 4.4 Anexos e limites

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| R12 | **Anexo dentro do limite** | appsettings `tamanhoAnexos` (ex.: 10 MB); e-mail recebido com anexo menor que o limite | Receber e-mail com anexo. | E-mail e anexo gravados; processamento normal. |
| R13 | **Anexo excede tamanho máximo** | E-mail com anexo maior que `tamanhoAnexos` | Receber esse e-mail. | EnviarEmailTamanhoMaximoAtingido chamado (notificação); fluxo de processamento segue conforme regra. |

### 4.5 Múltiplas contas e flags

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| R14 | **Várias contas com recebimento ativo** | Duas ou mais contas com `receberemail = true` | Iniciar worker; aguardar ciclo. | Cada conta executa receiveAsync; Processar para cada uma. |
| R15 | **Conta com recebimento desabilitado** | Ao menos uma conta com `receberemail = false` | Iniciar worker. | Log “RECEBIMENTO DE EMAILS DESABILITADO”; nenhum receiveAsync para essa conta. |

### 4.6 Erros e notificação

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| R16 | **Credenciais POP/Graph/Gmail inválidas** | Conta com credenciais erradas (por tipo) | Iniciar worker com receberemail = true. | Exceção em ReceiveEmailAsync; INotificacaoErro (Telegram); log de erro. |
| R17 | **Rede/ serviço indisponível** | Servidor de e-mail ou API fora do ar (ou bloqueado) | Tentar recebimento. | Erro tratado; notificação; log. |

---

## 5. Testes do Worker e ciclo completo

| ID | Cenário | Pré-condição | Passos | Resultado esperado |
|----|---------|--------------|--------|--------------------|
| W1 | **Ciclo completo – envio e recebimento** | Contas ativas para envio e recebimento; pendentes na fila e mensagens na caixa | Iniciar worker; aguardar 1 ciclo. | ENVIAEMAIL e RECEBEEMAIL executados; SalvarDataHoraUltimoProcessamentoEmail; logs de ciclo. |
| W2 | **Exceção no envio (geral)** | Forçar falha (ex.: conta inválida, BD indisponível) no envio | Iniciar worker. | Catch em ENVIAEMAIL; notificação “Erro no envio de e-mail”; aplicação pode encerrar (StopApplication). |
| W3 | **Exceção no recebimento (geral)** | Forçar falha no recebimento | Idem. | Catch em RECEBEEMAIL; notificação “Erro no recebimento de e-mail”. |
| W4 | **Listar contas falha** | ContaEmailService.Listar() retorna sucesso = false ou exceção | Configurar cenário (ex.: BD offline). | Tratamento/notificação conforme implementação; log. |
| W5 | **Configuração (appsettings)** | Verificar uso de configuração | Conferir conexao, conexaosqlite, cliente, Telegram, verQuery, verDebug, verErros, verInfos, tamanhoAnexos. | Serviço inicia e usa valores corretos; logs refletem ver* quando aplicável. |

---

## 6. Matriz de cobertura resumida

| Provedor | Envio (fila) | Envio avulso | Recebimento | Processamento |
|----------|----------------|--------------|-------------|----------------|
| Padrão (0) | E1, E4–E12, E19 | E16 | R1, R4–R5, R12–R13, R16 | R6–R11 |
| Office 365 (1) | E2, E20 | E17 | R2, R4–R5, R12–R13, R16 | R6–R11 |
| Gmail (2) | E3, E21 | E18 | R3, R4–R5, R12–R13, R16 | R6–R11 |
| Múltiplas / flags | E13–E15, E22 | — | R14–R15 | — |
| Worker / ciclo | W1–W5 | — | — | — |

---

## 7. Ordem sugerida e registro

1. **Ambiente:** garantir base de dados, appsettings e contas de teste (uma por tipo, se possível).
2. **Envio:** executar E1 → E3 (um provedor por vez); depois E4–E12 em um provedor; depois E13–E15, E16–E18, E19–E22.
3. **Recebimento:** executar R1 → R3; depois R4–R5, R6–R11, R12–R13, R14–R17.
4. **Worker:** W1–W5 conforme disponibilidade de cenários de falha.
5. **Registro:** para cada teste, anotar **ID**, **data**, **resultado (OK/Falha)** e **observações** (ex.: mensagem de erro, log relevante).

---

*Documento gerado com base na análise do projeto poliview.crm.service.email (Worker, EnviarEmail, ReceberEmail, Send* / Receive*, ProcessarEmailsRecebidos e configuração).*
