# Sistema de Cores - SIECON CRM Admin

## 📋 Como Alterar as Cores do Sistema

Para alterar as cores primárias e secundárias em **todo o aplicativo**, você precisa modificar **apenas 2 arquivos**:

### 1. Arquivo CSS Principal (`wwwroot/css/app.css`)

Localize a seção `:root` no início do arquivo e altere as variáveis:

```css
:root {
    /* CORES PRIMÁRIAS - Alterar aqui para mudar em todo o app */
    --color-primary: #023A51;  ← Altere esta cor
    
    /* CORES SECUNDÁRIAS - Alterar aqui para mudar em todo o app */
    --color-secondary: #4682B4;  ← Altere esta cor
}
```

### 2. Configuração do Tailwind (`wwwroot/index.html`)

Localize a seção `tailwind.config` e atualize as cores:

```javascript
colors: {
    primary: {
        500: '#023A51',  ← Altere esta cor (mesma da primária)
        600: '#012e41',  ← Versão mais escura
        // ... outras variações
    },
    secondary: {
        500: '#4682B4',  ← Altere esta cor (mesma da secundária)
        600: '#386892',  ← Versão mais escura
        // ... outras variações
    }
}
```

## 🎨 Cores Atuais

- **Primária**: `#023A51` (Azul escuro)
- **Secundária**: `#4682B4` (Azul médio)

## 📝 Notas Importantes

1. **Sempre altere ambos os arquivos** para manter a consistência
2. As classes Tailwind `primary-*` e `secondary-*` estão disponíveis em todo o projeto
3. As variáveis CSS `--color-primary` e `--color-secondary` podem ser usadas em CSS customizado
4. Após alterar, recarregue a aplicação para ver as mudanças

## 🔍 Onde as Cores São Usadas

- Botões principais: `bg-primary-500`
- Links e textos destacados: `text-primary-600`
- Bordas e destaques: `border-primary-500`
- Estados hover: `hover:bg-primary-600`
- Menu ativo: Usa `--color-primary` via CSS

## ✅ Checklist ao Alterar Cores

- [ ] Alterar `--color-primary` em `app.css`
- [ ] Alterar `--color-secondary` em `app.css`
- [ ] Atualizar `primary.500` em `index.html`
- [ ] Atualizar `secondary.500` em `index.html`
- [ ] Testar visualmente a aplicação
- [ ] Verificar contraste de texto (acessibilidade)
