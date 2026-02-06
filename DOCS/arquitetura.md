# Arquitetura da Solução Poliview CRM

Este documento descreve a estrutura e os projetos da solution **Poliview.crm.sln**, suas responsabilidades e dependências.

---

## 1. Visão geral

A solução implementa um **CRM (Customer Relationship Management)** com:

- **APIs REST** para integração e uso por clientes
- **Front-end Blazor WebAssembly** (Espaço do Cliente)
- **Serviços em segundo plano** (Workers) para e-mail, SLA, integração e monitoramento
- **Aplicações desktop Windows** (instalador, validação de bases)
- **Camadas de domínio, modelos, infraestrutura, repositórios e serviços** compartilhadas

O stack principal é **.NET 9**, com um projeto legado em **.NET Framework 4.5.2** (Poliview.crm.servicos – serviço Windows).

---

## 2. Diagrama de dependências (resumido)

```
                    ┌─────────────────────┐
                    │ Poliview.crm.domain │  (entidades, sem dependências de outros projetos)
                    └──────────┬──────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         │                     │                     │
         ▼                     ▼                     ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐
│ Poliview.crm.   │  │ Poliview.crm.   │  │ Poliview.crm.           │
│ models          │  │ repositorios    │  │ http.service            │
│ (DTOs, view     │  │ (SQL Server,    │  │ (clientes HTTP para     │
│  models)        │  │  Firebird,      │  │  chamadas à API)        │
└────────┬────────┘  │  SQLite)       │  └────────────┬────────────┘
         │           └────────┬───────┘               │
         │                    │                        │
         └──────────┬─────────┴────────────────────────┘
                    │
                    ▼
         ┌─────────────────────┐
         │ Poliview.crm.infra   │  (utilitários, exportação, configuração)
         └──────────┬───────────┘
                    │
                    ▼
         ┌─────────────────────┐
         │ Poliview.crm.        │  (regras de negócio, orquestração)
         │ services             │
         └──────────┬───────────┘
                    │
    ┌───────────────┼───────────────┬─────────────────┬──────────────────┐
    ▼               ▼               ▼                 ▼                  ▼
┌─────────┐  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐  ┌──────────────────┐
│ .api    │  │ .espaco     │  │ Workers      │  │ .instalador  │  │ .validarbases    │
│ (Web    │  │ cliente     │  │ (email, sla, │  │ (WinForms)   │  │ (WinForms)       │
│  API)   │  │ (Blazor     │  │  integração, │  │              │  │                  │
│         │  │  WASM)      │  │  monitor)    │  │              │  │                  │
└─────────┘  └─────────────┘  └──────────────┘  └──────────────┘  └──────────────────┘
```

---

## 3. Projetos por camada e tipo

### 3.1 Camada de domínio e dados

| Projeto | Tipo | Framework | Descrição |
|--------|------|-----------|-----------|
| **Poliview.crm.domain** | Biblioteca | net9.0 | Entidades de domínio do CRM (clientes, chamados, contratos, unidades, etc.). Sem referências a outros projetos da solution. |
| **Poliview.crm.models** | Biblioteca | net9.0 | DTOs e view models. Depende apenas de **Poliview.crm.domain**. |
| **Poliview.crm.repositorios** | Biblioteca | net9.0 | Acesso a dados com Dapper (SQL Server, Firebird, SQLite). Sem ProjectReference; usa pacotes de acesso a dados. |
| **Poliview.crm.infra** | Biblioteca | net9.0 | Infraestrutura compartilhada: utilitários, exportação Excel (ClosedXML), configuração, criptografia, e-mail. Depende de **domain** e **models**. |

### 3.2 Camada de serviços e HTTP

| Projeto | Tipo | Framework | Descrição |
|--------|------|-----------|-----------|
| **Poliview.crm.http.service** | Biblioteca | net9.0 | Clientes HTTP que consomem a API principal (autenticação, blocos, empreendimentos, mensagens, notificações, unidades, usuários, etc.). Usado pelo front-end. Depende de **domain**, **infra**, **models**. |
| **Poliview.crm.services** | Biblioteca | net9.0 | Regras de negócio e orquestração: serviços de acesso, autenticação, chamados, contratos, e-mail, empreendimentos, PDF, SLA, Telegram, upload, etc. Depende de **domain**, **infra**, **models**, **repositorios**. |

### 3.3 APIs e front-end

| Projeto | Tipo | Framework | Descrição |
|--------|------|-----------|-----------|
| **Poliview.crm.api** | Web API | net9.0 | API REST principal do CRM. Controllers para acesso, autenticação, chamados, configuração, e-mail, empreendimentos, mensagens, notificações, usuários, etc. JWT, Swagger, hospedagem como Windows Service. Depende de **http.service**, **infra**, **models**, **services**. |
| **Poliview.crm.espacocliente** | Blazor WebAssembly | net9.0 | Aplicação SPA do “Espaço do Cliente” (MudBlazor, EPPlus, ClosedXML, etc.). Consome a API via **http.service**. Depende de **domain**, **http.service**, **infra**, **services**. |
| **Poliview.crm.mobuss.api** | Web API | net9.0 | API específica para integração Mobuss. Dapper, SQL Server; pode rodar como Windows Service. Sem ProjectReference para outros projetos da solution. |

### 3.4 Workers (serviços em segundo plano)

| Projeto | Tipo | Framework | Descrição |
|--------|------|-----------|-----------|
| **poliview.crm.service.email** | Worker | net9.0 | Serviço de e-mail (envio/recebimento; Gmail, Office 365). Depende de **domain**, **models**, **services**. |
| **poliview.crm.email** | Worker | net9.0 | Outro worker de e-mail. Depende de **domain**, **services**. |
| **poliview.crm.sla** | Worker | net9.0 | Processamento de SLA. Depende de **services**. |
| **Poliview.crm.integracao** | Worker | net9.0 | Integração com sistemas externos (Firebird, MimeKit). Depende de **domain**, **repositorios**, **services**. |
| **Poliview.crm.mobuss.integracao** | Worker | net9.0 | Integração com Mobuss. Sem ProjectReference. |
| **Poliview.crm.monitor.service** | Worker | net9.0 | Serviço de monitoramento. Depende de **domain**, **infra**, **services**. |

### 3.5 Aplicações desktop (Windows)

| Projeto | Tipo | Framework | Descrição |
|--------|------|-----------|-----------|
| **poliview.crm.instalador** | WinForms | net9.0-windows | Aplicativo de instalação/configuração do CRM. Dapper, SQL Server, Firebird; controle de serviços Windows. Depende de **domain**, **infra**. |
| **poliview.crm.validarbases** | WinForms | net9.0-windows | Ferramenta para validação de bases de dados. Depende de **domain** (e configuração via appsettings). |

### 3.6 Outros

| Projeto | Tipo | Framework | Descrição |
|--------|------|-----------|-----------|
| **Poliview.crm** | Blazor WebAssembly | net9.0 | Outra aplicação Blazor (provavelmente painel/admin). Depende apenas de **domain**. Usa MudBlazor, FluentValidation, Google/Gmail, MimeKit. |
| **Poliview.crm.servicos** | Windows Service (legado) | .NET Framework 4.5.2 | Serviço Windows no formato antigo (não SDK-style). Projeto separado do ecossistema .NET Core/9. |

### 3.7 Itens de solução

- Pasta **“Itens de Solução”**: contém **Directory.Packages.props** (gerenciamento central de versões de pacotes) e **versao.txt**.

---

## 4. Fluxo de dados típico

1. **Cliente (navegador)** → **Poliview.crm.espacocliente** (Blazor WASM) → **Poliview.crm.http.service** (HTTP) → **Poliview.crm.api** (REST).
2. **Poliview.crm.api** → **Poliview.crm.services** (regras de negócio) → **Poliview.crm.repositorios** ou **Poliview.crm.infra** quando necessário.
3. **Workers** (e-mail, SLA, integração, monitor) usam **services** (e eventualmente **repositorios**/infra) para tarefas em segundo plano.
4. **Poliview.crm.domain** e **Poliview.crm.models** são a base compartilhada por quase todos os projetos.

---

## 5. Tecnologias principais

- **.NET 9** (maioria dos projetos)
- **Blazor WebAssembly** (espacocliente, Poliview.crm)
- **ASP.NET Core Web API** (Poliview.crm.api, Poliview.crm.mobuss.api)
- **Worker Service** (e-mail, sla, integração, monitor, mobuss.integracao)
- **WinForms** (instalador, validarbases)
- **Dapper** (acesso a dados em vários projetos)
- **SQL Server, Firebird, SQLite** (repositorios e instalador)
- **JWT, Swagger, MudBlazor, EPPlus, ClosedXML, Serilog, Telegram.Bot**, entre outros (conforme Directory.Packages.props e csproj).

---

## 6. Observações

- **Poliview.crm.servicos** é o único projeto em .NET Framework 4.5.2; o restante está em .NET 9.
- **Poliview.crm.mobuss.api** e **Poliview.crm.mobuss.integracao** são isolados em relação aos outros projetos da solution (sem ProjectReference para domain/services).
- As dependências de **poliview.crm.service.email** para domain/models usam caminhos com letras minúsculas (`poliview.crm.domain`, `poliview.crm.models`); no Windows costuma funcionar, mas vale padronizar com o restante da solution.
- A solução utiliza **Central Package Management** (Directory.Packages.props) para versões de pacotes NuGet.

---

*Documento gerado com base na análise da Poliview.crm.sln e dos arquivos .csproj. Atualizar conforme evolução da solução.*
