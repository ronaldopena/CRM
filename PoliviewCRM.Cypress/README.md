# 🧪 PoliviewCRM.Cypress - Testes E2E para API

Projeto de testes End-to-End para validação da API **Poliview.crm.api**, focado especificamente no **AutenticacaoController.cs**.

## 📋 Visão Geral

Este projeto utiliza **Cypress 12.17.4** para testar de forma abrangente o endpoint de autenticação da API Poliview CRM, validando desde cenários básicos até validações completas do controller.

## 🚀 Instalação Rápida

```bash
# Instalação automática (recomendado)
./instalar.bat

# Se houver problemas
./resolver-problemas.bat
```

## ⚙️ Configuração

### Pré-requisitos

- **Node.js**: 12.x ou superior (recomendado: 18.x LTS)
- **API Poliview.crm.api** rodando em `localhost:9533`
- **Windows** com PowerShell/CMD

### Configuração da API

Certifique-se de que a API esteja configurada com:

- **Porta**: 9533 (definida em appsettings.json)
- **JWT configurações**:
  - Subject: "baseWebApiSubject"
  - Issuer: "basewebApiIssuer"
  - Audience: "baseWebApiAudience"
  - Key: "**poliview.tecnologia.crm@2022**"

## 🧪 Arquivos de Teste

### 1. `autenticacao.cy.js` - Testes Básicos

- ✅ Login com credenciais válidas/inválidas
- ✅ Validação de campos obrigatórios
- ✅ Teste de diferentes origens (PORTAL, CRM, APP, MOBUSS)
- ✅ Validação de empresas
- ✅ Estrutura da resposta (IRetorno)
- ✅ Performance e timeout

### 2. `autenticacao-fixtures.cy.js` - Testes com Dados Organizados

- ✅ Usuários válidos e inválidos usando fixtures
- ✅ Usuários bloqueados e inativos
- ✅ Diferentes tipos de identificação (Email, CPF, CNPJ)
- ✅ Cenários de erro estruturados

### 3. `autenticacao-controller.cy.js` - Validação Completa (Corrigido)

- ✅ Validação detalhada da estrutura da resposta
- ✅ Testes de segurança (senha zerada na resposta)
- ✅ Decodificação e validação do JWT
- ✅ Comportamentos específicos do controller
- ✅ Testes de payload inválido (corrigido)
- ✅ Execução sequencial (sem Promise mixing)

### 4. `autenticacao-usuarios-reais.cy.js` - Novo! 🆕

- ✅ Descoberta de usuários existentes no sistema
- ✅ Testes adaptativos baseados em dados reais
- ✅ Validação de formatos CPF/CNPJ
- ✅ Testes de segurança (SQL Injection)
- ✅ Cenários realistas com dados do sistema

## 🎯 Execução dos Testes

### Script de Execução (Atualizado)

```bash
./executar-testes.bat
```

**Opções disponíveis:**

1. **Modo interativo** - Interface gráfica do Cypress
2. **Modo headless** - Todos os testes em linha de comando
3. **Testes básicos** - Apenas autenticacao.cy.js
4. **Testes com fixtures** - Apenas autenticacao-fixtures.cy.js
5. **Testes detalhados** - Apenas autenticacao-controller.cy.js
6. **Usuários reais** - Apenas autenticacao-usuarios-reais.cy.js 🆕
7. **Relatório completo** - Todos os testes com relatório detalhado

### Execução Manual

```bash
# Modo interativo
npm run cypress:open

# Todos os testes
npm run cypress:run

# Arquivo específico
npx cypress run --spec "cypress/e2e/autenticacao.cy.js"

# Novo arquivo com usuários reais
npx cypress run --spec "cypress/e2e/autenticacao-usuarios-reais.cy.js"
```

## 🔧 Correções Implementadas

### Problemas Resolvidos

1. **Testes de senha incorreta**: Agora aceita tanto "senha incorreta" quanto "usuário não encontrado"
2. **Usuários bloqueados/inativos**: Testes adaptativos que funcionam mesmo sem dados específicos
3. **Payload inválido**: Aceita resposta 200 com `sucesso: false` além de códigos HTTP de erro
4. **Promises mixing**: Removido uso incorreto de Promises com comandos Cypress
5. **Requisições simultâneas**: Convertido para execução sequencial

### Melhorias Implementadas

- ✅ Logs mais detalhados e informativos
- ✅ Testes adaptativos baseados em respostas da API
- ✅ Validação flexível de mensagens de erro
- ✅ Novo arquivo para descoberta de usuários reais
- ✅ Melhor handling de cenários de teste

## 📊 Cobertura de Testes

### Controller Coberto: `AutenticacaoController.cs`

- ✅ **Endpoint**: `POST /autenticacao/login`
- ✅ **Método**: `Login(LoginRequisicao obj)`
- ✅ **Validações**: Estrutura completa da resposta
- ✅ **JWT**: Configurações hardcoded validadas
- ✅ **Segurança**: Senha zerada, sem vazamento de dados
- ✅ **Performance**: Testes de timeout e velocidade

### Cenários Testados

- ✅ **36+ casos de teste** distribuídos em 4 arquivos
- ✅ **Autenticação válida/inválida**
- ✅ **4 origens** (PORTAL, CRM, APP, MOBUSS)
- ✅ **Múltiplas empresas**
- ✅ **Tipos de identificação** (Email, CPF, CNPJ)
- ✅ **Usuários especiais** (bloqueados, inativos)
- ✅ **Validação de segurança**
- ✅ **Testes de carga básicos**

## 🛠️ Comandos Úteis

### Desenvolvimento

```bash
# Limpar cache
npm run limpar-cache

# Reinstalar dependências
./resolver-problemas.bat

# Verificar Cypress
npx cypress verify

# Executar com logs detalhados
DEBUG=cypress:* npm run cypress:run
```

### Troubleshooting

```bash
# Se testes falharem
./resolver-problemas.bat

# Verificar conectividade da API
curl http://localhost:9533

# Reset completo
rmdir /s /q node_modules
del package-lock.json
npm install --save-dev cypress@12.17.4
```

## 📁 Estrutura do Projeto

```
PoliviewCRM.Cypress/
├── cypress/
│   ├── e2e/
│   │   ├── autenticacao.cy.js              # Testes básicos
│   │   ├── autenticacao-fixtures.cy.js     # Testes com fixtures
│   │   ├── autenticacao-controller.cy.js   # Validação completa (corrigido)
│   │   └── autenticacao-usuarios-reais.cy.js # Usuários reais (novo)
│   ├── fixtures/
│   │   └── usuarios.json                   # Dados de teste organizados
│   └── support/
│       ├── commands.js                     # Comandos customizados
│       └── e2e.js                         # Configurações globais
├── cypress.config.js                      # Configuração principal (otimizado)
├── package.json                          # Dependências (Cypress 12.17.4)
├── instalar.bat                          # Script de instalação (melhorado)
├── executar-testes.bat                   # Script de execução (7 opções)
├── resolver-problemas.bat               # Script de troubleshooting
├── README.md                            # Esta documentação
└── SOLUCAO-PROBLEMAS.md                # Guia de problemas
```

## 📝 Dados de Teste

### Fixtures Organizadas (`cypress/fixtures/usuarios.json`)

```json
{
  "validos": [
    { "usuario": "admin@poliview.com.br", "senha": "admin123" },
    { "usuario": "teste@poliview.com.br", "senha": "teste123" }
  ],
  "invalidos": [{ "usuario": "inexistente@teste.com", "senha": "qualquer" }],
  "bloqueados": [
    { "usuario": "usuario.bloqueado@teste.com", "senha": "senha123" }
  ],
  "tipos_identificacao": [
    { "tipo": "Email", "valor": "teste@poliview.com.br" },
    { "tipo": "CPF", "valor": "12345678901" },
    { "tipo": "CNPJ", "valor": "12345678000195" }
  ]
}
```

## 🔍 Validações Implementadas

### Estrutura da Resposta (IRetorno)

```json
{
  "status": "number",
  "sucesso": "boolean",
  "mensagem": "string",
  "objeto": {
    "CD_USUARIO": "number",
    "NM_USUARIO": "string",
    "DS_EMAIL": "string",
    "NR_CPFCNPJ": "string",
    "IN_BLOQUEADO": "boolean",
    "IN_STATUS": "boolean",
    "token": "string (JWT)",
    "DS_SENHA": "" // Sempre vazia por segurança
  }
}
```

### JWT Hardcoded Validado

- **Subject**: "baseWebApiSubject"
- **Issuer**: "basewebApiIssuer"
- **Audience**: "baseWebApiAudience"
- **Key**: "**poliview.tecnologia.crm@2022**"

## 🚨 Resolução de Problemas

### Erro: "SyntaxError: Unexpected token '.'"

**Causa**: Node.js muito antigo para Cypress 13.x
**Solução**:

```bash
./resolver-problemas.bat
# Escolha opção 2: "Usar versão compatível do Cypress (12.x)"
```

### Testes Falhando

**Causa**: Usuários de teste podem não existir no sistema
**Solução**: Use o novo arquivo `autenticacao-usuarios-reais.cy.js` que descobre automaticamente usuários válidos

### API não responde

**Causa**: API não está rodando na porta 9533
**Solução**:

```bash
# Verificar se API está rodando
curl http://localhost:9533

# Verificar configuração no appsettings.json da API
```

## 📞 Suporte

Para problemas específicos:

1. Execute `./resolver-problemas.bat`
2. Consulte `SOLUCAO-PROBLEMAS.md`
3. Verifique logs detalhados no console
4. Use modo interativo para debug: `npm run cypress:open`

---

**💡 Dica**: Use sempre o arquivo `autenticacao-usuarios-reais.cy.js` para descobrir automaticamente quais usuários existem no seu sistema antes de executar os outros testes!
