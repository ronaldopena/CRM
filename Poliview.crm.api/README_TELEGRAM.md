# Controller Telegram - Poliview CRM API

Este documento descreve como configurar e usar o controller do Telegram para enviar mensagens via bot.

## 📋 Pré-requisitos

1. **Bot do Telegram**: Você precisa criar um bot no Telegram
2. **Token do Bot**: Obtido através do @BotFather
3. **Chat ID**: ID do chat onde as mensagens serão enviadas

## 🚀 Como criar um Bot no Telegram

1. Abra o Telegram e procure por `@BotFather`
2. Digite `/newbot` e siga as instruções
3. Escolha um nome para seu bot
4. Escolha um username (deve terminar com "bot")
5. Copie o **token** fornecido

## 🔑 Como obter o Chat ID

### Método 1: Chat privado

1. Envie uma mensagem para seu bot
2. Acesse: `https://api.telegram.org/bot<SEU_TOKEN>/getUpdates`
3. Procure por `"chat":{"id":NUMERO}` - esse número é seu Chat ID

### Método 2: Grupo

1. Adicione o bot ao grupo
2. Envie uma mensagem mencionando o bot
3. Acesse a URL acima e procure pelo Chat ID (será negativo para grupos)

## ⚙️ Configuração

### 1. Configurar appsettings.json

```json
{
  "Telegram": {
    "BotToken": "1234567890:ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789",
    "ChatId": "123456789"
  }
}
```

### 2. Configurar variáveis específicas por ambiente

**appsettings.Development.json**

```json
{
  "Telegram": {
    "BotToken": "SEU_TOKEN_DE_DESENVOLVIMENTO",
    "ChatId": "SEU_CHAT_ID_DE_TESTE"
  }
}
```

**appsettings.Production.json**

```json
{
  "Telegram": {
    "BotToken": "SEU_TOKEN_DE_PRODUCAO",
    "ChatId": "SEU_CHAT_ID_DE_PRODUCAO"
  }
}
```

## 📚 Endpoints Disponíveis

### 1. Testar Conexão

**GET** `/telegram/testar-conexao`

Testa se o bot está configurado corretamente.

**Resposta:**

```json
{
  "sucesso": true,
  "mensagem": "Conexão com o bot 'NomeDoBot' estabelecida com sucesso",
  "dados": {
    "botId": 123456789,
    "botUsername": "meu_bot",
    "chatIdConfigurado": true
  }
}
```

### 2. Enviar Mensagem Simples

**POST** `/telegram/enviar-mensagem`

**Body:**

```json
{
  "mensagem": "Olá! Esta é uma mensagem de teste.",
  "chatId": "123456789",
  "formatMarkdown": false
}
```

**Resposta:**

```json
{
  "sucesso": true,
  "mensagem": "Mensagem enviada com sucesso",
  "dados": {
    "messageId": 42,
    "chatId": "123456789",
    "dataEnvio": "2024-01-15T10:30:00Z"
  }
}
```

### 3. Enviar Mensagem com Markdown

**POST** `/telegram/enviar-mensagem`

**Body:**

```json
{
  "mensagem": "*Título em negrito*\n\n_Texto em itálico_\n\n`Código inline`\n\n[Link](https://example.com)",
  "formatMarkdown": true
}
```

### 4. Enviar Arquivo

**POST** `/telegram/enviar-arquivo`

**Body:**

```json
{
  "caminhoArquivo": "C:\\temp\\documento.pdf",
  "legenda": "Documento importante anexado",
  "chatId": "123456789"
}
```

### 5. Notificação do Sistema

**POST** `/telegram/notificacao-sistema?titulo=Alerta&mensagem=Sistema reiniciado&nivel=Warning`

**Parâmetros:**

- `titulo`: Título da notificação
- `mensagem`: Conteúdo da mensagem
- `nivel`: `Info`, `Warning`, `Error`, `Success`
- `chatId`: (opcional) Chat ID específico

**Resposta:**

```json
{
  "sucesso": true,
  "mensagem": "Notificação enviada com sucesso",
  "dados": {
    "messageId": 43,
    "chatId": "123456789",
    "dataEnvio": "2024-01-15T10:35:00Z"
  }
}
```

### 6. Informações do Bot

**GET** `/telegram/info-bot`

Retorna informações sobre o bot configurado.

## 🔧 Exemplos de Uso

### Usando cURL

```bash
# Testar conexão
curl -X GET "http://localhost:9533/telegram/testar-conexao"

# Enviar mensagem simples
curl -X POST "http://localhost:9533/telegram/enviar-mensagem" \
  -H "Content-Type: application/json" \
  -d '{
    "mensagem": "Teste de mensagem via API",
    "formatMarkdown": false
  }'

# Enviar notificação de erro
curl -X POST "http://localhost:9533/telegram/notificacao-sistema?titulo=Erro%20Crítico&mensagem=Falha%20no%20banco%20de%20dados&nivel=Error"
```

### Usando C#

```csharp
using System.Text;
using System.Text.Json;

public async Task EnviarMensagemTelegramAsync(string mensagem)
{
    var client = new HttpClient();
    var baseUrl = "http://localhost:9533";

    var request = new
    {
        mensagem = mensagem,
        formatMarkdown = false
    };

    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync($"{baseUrl}/telegram/enviar-mensagem", content);
    var resultado = await response.Content.ReadAsStringAsync();

    Console.WriteLine(resultado);
}
```

### Usando JavaScript/TypeScript

```javascript
async function enviarMensagemTelegram(mensagem) {
  const response = await fetch(
    "http://localhost:9533/telegram/enviar-mensagem",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        mensagem: mensagem,
        formatMarkdown: false,
      }),
    }
  );

  const resultado = await response.json();
  console.log(resultado);
}

// Uso
enviarMensagemTelegram("Mensagem de teste via JavaScript");
```

## 🛡️ Tratamento de Erros

### Erros Comuns

**Token inválido:**

```json
{
  "sucesso": false,
  "mensagem": "Erro na conexão com o bot: Unauthorized"
}
```

**Chat ID inválido:**

```json
{
  "sucesso": false,
  "mensagem": "Erro ao enviar mensagem: Bad Request: chat not found"
}
```

**Arquivo não encontrado:**

```json
{
  "sucesso": false,
  "mensagem": "Arquivo não encontrado"
}
```

## 🔒 Segurança

1. **Nunca commite tokens**: Use variáveis de ambiente ou cofres de segredos
2. **Valide Chat IDs**: Certifique-se de que apenas chats autorizados podem receber mensagens
3. **Rate Limiting**: O Telegram tem limites de mensagens por segundo
4. **Logs**: Monitore o uso para detectar abusos

## 🔄 Integração com outros Serviços

### Enviar notificações de chamados

```csharp
// Em ChamadoService.cs
public async Task NotificarNovoChamado(Chamado chamado)
{
    var client = new HttpClient();
    var titulo = $"Novo Chamado #{chamado.Id}";
    var mensagem = $"Cliente: {chamado.NomeCliente}\nPrioridade: {chamado.Prioridade}";

    await client.PostAsync(
        $"http://localhost:9533/telegram/notificacao-sistema?titulo={titulo}&mensagem={mensagem}&nivel=Info",
        null
    );
}
```

### Monitoramento de sistema

```csharp
// Em Program.cs ou Worker Service
public async Task EnviarStatusSistema()
{
    var client = new HttpClient();
    var titulo = "Status do Sistema";
    var mensagem = $"Sistema operacional\nUptime: {Environment.TickCount64 / 1000 / 60} minutos";

    await client.PostAsync(
        $"http://localhost:9533/telegram/notificacao-sistema?titulo={titulo}&mensagem={mensagem}&nivel=Success",
        null
    );
}
```

## 📈 Monitoramento e Logs

O controller gera logs automáticos para:

- Tentativas de conexão
- Mensagens enviadas com sucesso
- Erros de envio
- Arquivos enviados

Monitore estes logs para acompanhar o uso e detectar problemas.

---

## 🆘 Suporte

Para problemas ou dúvidas:

1. Verifique a configuração do token e Chat ID
2. Teste a conectividade com `/telegram/testar-conexao`
3. Consulte os logs da aplicação
4. Verifique a documentação oficial do Telegram Bot API

**Links úteis:**

- [Telegram Bot API](https://core.telegram.org/bots/api)
- [BotFather](https://t.me/botfather)
- [Telegram Bot Tutorial](https://core.telegram.org/bots/tutorial)
