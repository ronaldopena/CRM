# Análise do Projeto poliview.crm.instalador

## Visão Geral

O projeto `poliview.crm.instalador` é uma aplicação Windows Forms (.NET 8.0) responsável pela instalação e configuração do sistema Poliview CRM.

## Características Técnicas

### Framework e Runtime

- Framework: .NET 8.0
- Tipo de Aplicação: Windows Forms (WinForms)
- Plataforma: Windows

### Configurações do Projeto

- Output Type: WinExe (Executável Windows)
- Nullable: Habilitado
- Uso Implícito: Habilitado
- Windows Forms: Habilitado

### Dependências NuGet

- FirebirdSql.Data.FirebirdClient
- System.Data.SqlClient
- System.Net.Http
- System.ServiceProcess.ServiceController
- System.Text.RegularExpressions

### Referências de Projetos

1. Poliview.crm.domain
2. Poliview.crm.infra

## Configurações do Sistema

O projeto utiliza um arquivo `configcrm.json` para armazenar as configurações de instalação, que inclui:

```json
{
  "Servidor": "G15\\SQLEXPRESS",
  "Banco": "CRM_4_3_7",
  "Usuario": "sa",
  "Senha": "master",
  "Porta": 1433,
  "Aplicativo": "/421",
  "Site": "Default Web Site",
  "Mensagem": "OK",
  "UsuarioAdm": "sa",
  "SenhaAdm": "master",
  "NomeInstancia": "4_3_7",
  "TimeoutSql": "120",
  "PortaApi": 8003
}
```

## Funcionalidades Principais

1. Configuração de conexão com banco de dados SQL Server
2. Configuração de IIS (Internet Information Services)
3. Gerenciamento de instâncias do CRM
4. Configuração de timeout SQL
5. Configuração de portas para API

## Observações de Segurança

- As credenciais estão armazenadas em texto plano no arquivo de configuração
- Utiliza usuário SA para conexão com banco de dados
- Recomenda-se implementar criptografia para dados sensíveis

## Pontos de Melhoria Sugeridos

1. Implementar criptografia para senhas e dados sensíveis
2. Utilizar usuário com privilégios limitados ao invés de SA
3. Adicionar validações de segurança para as configurações
4. Implementar logs de instalação
5. Adicionar backup automático antes de alterações
6. Implementar validação de configurações
7. Adicionar suporte para diferentes ambientes (Dev, Homolog, Prod)

## Dependências do Sistema

- Windows OS
- .NET 8.0 Runtime
- SQL Server
- IIS (Internet Information Services)
- Firebird Client
