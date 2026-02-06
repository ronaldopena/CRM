# 🔧 Solução de Problemas - PoliviewCRM.Cypress

Este guia ajuda a resolver problemas comuns encontrados durante a instalação e execução dos testes Cypress.

## 🚨 Erro: "SyntaxError: Unexpected token '.'"

### Problema

```
SyntaxError: Unexpected token '.'
    at wrapSafe (internal/modules/cjs/loader.js:915:16)
```

### Causa

Sua versão do Node.js é muito antiga para suportar o operador de encadeamento opcional (`?.`) usado pelo Cypress 13.x+.

### Soluções

#### Solução 1: Usar Versão Compatível do Cypress (Recomendada)

```bash
# Execute o script de resolução
./resolver-problemas.bat

# Ou manualmente:
npm uninstall cypress
npm install --save-dev cypress@12.17.4
```

#### Solução 2: Atualizar Node.js (Ideal)

1. Baixe Node.js 18.x LTS: https://nodejs.org/
2. Instale a nova versão
3. Reinicie o terminal
4. Execute:

```bash
node --version  # Deve mostrar v18.x.x
npm install --save-dev cypress@13.6.0
```

## 🔍 Verificar Compatibilidade

### Versões Recomendadas

- **Node.js**: 14.x ou superior (recomendado: 18.x LTS)
- **npm**: 6.x ou superior
- **Cypress**: 12.x para Node.js < 14, ou 13.x+ para Node.js 14+

### Comandos de Verificação

```bash
# Verificar versões
node --version
npm --version

# Verificar Cypress instalado
npm list cypress

# Verificar Cypress
npx cypress verify
```

## 🧹 Limpeza Completa

Se os problemas persistirem, faça uma limpeza completa:

```bash
# 1. Limpar cache do npm
npm cache clean --force

# 2. Remover dependências
rmdir /s /q node_modules
del package-lock.json

# 3. Reinstalar com versão compatível
npm install --save-dev cypress@12.17.4

# 4. Verificar instalação
npx cypress verify
```

## 🔧 Scripts de Ajuda

### Resolução Automática

```bash
# Execute o script de resolução de problemas
./resolver-problemas.bat
```

### Instalação Limpa

```bash
# Execute o script de instalação atualizado
./instalar.bat
```

## 📋 Problemas Comuns

### 1. Cypress não abre

**Problema**: `cypress open` não funciona
**Solução**:

```bash
npx cypress verify
npx cypress install
npm run cypress:open
```

### 2. Erro de permissão no Windows

**Problema**: Erro de permissão ao instalar
**Solução**:

```bash
# Execute como administrador ou use:
npm install --save-dev cypress@12.17.4 --no-optional
```

### 3. Testes não encontram a API

**Problema**: Testes falham com erro de conexão
**Solução**:

1. Verifique se a API está rodando: `curl http://localhost:9533`
2. Confirme a porta no `cypress.config.js`
3. Execute a API antes dos testes

### 4. Timeouts nos testes

**Problema**: Testes falham por timeout
**Solução**: Ajuste os timeouts no `cypress.config.js`:

```javascript
defaultCommandTimeout: 15000,
requestTimeout: 15000,
responseTimeout: 15000
```

## 🚀 Execução Correta

### Passo a Passo

1. **Verificar Node.js**: `node --version`
2. **Instalar dependências**: `./instalar.bat`
3. **Verificar Cypress**: `npx cypress verify`
4. **Executar API**: Rodar `Poliview.crm.api` na porta 9533
5. **Executar testes**: `./executar-testes.bat`

### Ordem de Execução

```bash
# 1. Resolução de problemas (se necessário)
./resolver-problemas.bat

# 2. Instalação
./instalar.bat

# 3. Execução
./executar-testes.bat
```

## 🔍 Logs e Debug

### Logs Detalhados

```bash
# Cypress com logs detalhados
DEBUG=cypress:* npm run cypress:run

# npm com logs detalhados
npm install --loglevel verbose
```

### Informações do Sistema

```bash
# Verificar sistema
./resolver-problemas.bat
# Escolha opção 5: "Mostrar informações do sistema"
```

## 🆘 Suporte Adicional

Se nenhuma solução funcionar:

1. **Verifique logs**: Procure por erros específicos nos logs
2. **Sistema operacional**: Confirme compatibilidade com Windows
3. **Antivírus**: Temporariamente desabilite antivírus
4. **Proxy/Firewall**: Verifique configurações de rede
5. **Espaço em disco**: Confirme espaço suficiente

## 📞 Comandos de Emergência

### Reset Completo

```bash
# Remove tudo e recomeça
rmdir /s /q node_modules
del package-lock.json
npm cache clean --force
npm install --save-dev cypress@12.17.4
npx cypress verify
```

### Instalação Mínima

```bash
# Instalação com configurações mínimas
npm install --save-dev cypress@12.17.4 --no-optional --no-audit --no-fund
```

### Verificação de Funcionamento

```bash
# Teste rápido
npx cypress run --spec "cypress/e2e/autenticacao.cy.js" --headless
```

## ✅ Checklist de Resolução

- [ ] Node.js versão 12+ instalado
- [ ] npm funcionando corretamente
- [ ] Cache limpo (`npm cache clean --force`)
- [ ] Cypress 12.x instalado
- [ ] Cypress verificado (`npx cypress verify`)
- [ ] API rodando na porta 9533
- [ ] Sem erros de firewall/antivírus
- [ ] Espaço suficiente em disco

---

**💡 Dica**: Use sempre o script `resolver-problemas.bat` primeiro - ele resolve a maioria dos problemas automaticamente!
