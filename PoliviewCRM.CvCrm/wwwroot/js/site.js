// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Spinner global discreto
(function() {
  var spinnerEl = null;

  function getSpinner() {
    if (!spinnerEl) {
      spinnerEl = document.getElementById('global-spinner');
    }
    return spinnerEl;
  }

  window.showSpinner = function() {
    var el = getSpinner();
    if (el) { el.style.display = 'flex'; }
  };

  window.hideSpinner = function() {
    var el = getSpinner();
    if (el) { el.style.display = 'none'; }
  };

  // Opcional: mostrar spinner em qualquer submit de formulário com data-spinner="true"
  document.addEventListener('submit', function(e) {
    var form = e.target;
    if (form && (form.getAttribute('data-spinner') === 'true')) {
      showSpinner();
    }
  }, true);

  // Mostrar ampulheta ao navegar para outra página (antes de descarregar)
  window.addEventListener('beforeunload', function() {
    try { showSpinner(); } catch(_) {}
  });

  // Garantir que a ampulheta não fique visível após carregar
  window.addEventListener('load', function() {
    try { hideSpinner(); } catch(_) {}
  });

  // Ao restaurar via back/forward cache (pageshow), garantir ocultação do spinner
  window.addEventListener('pageshow', function() {
    try { hideSpinner(); } catch(_) {}
    // Fallback defensivo: ocultar novamente após pequeno atraso
    try { setTimeout(function(){ hideSpinner(); }, 200); } catch(_) {}
  });

  // Ao navegar no histórico (popstate), garantir ocultação do spinner
  window.addEventListener('popstate', function() {
    try { hideSpinner(); } catch(_) {}
  });
})();

// Funções específicas da tela de Boletos
// Mantidas aqui para evitar problemas de Razor no arquivo .cshtml
(function() {
  var boletoSelecionado = {
    numero: '',
    recto: '',
    cobranca: '',
    contrato: ''
  };

  window.mostrarCampoEmail = function(numero, recto, cobranca, contrato) {
    boletoSelecionado.numero = numero || '';
    boletoSelecionado.recto = recto || '';
    boletoSelecionado.cobranca = cobranca || '';
    boletoSelecionado.contrato = contrato || '';

    var emailSection = document.getElementById('email-section');
    if (emailSection) {
      emailSection.style.display = 'block';
      var emailInput = document.getElementById('email-input');
      if (emailInput) {
        // Pré-preencher com email padrão (do /cvcrm), se disponível
        try { emailInput.value = (window.DEFAULT_CLIENT_EMAIL || '').trim(); } catch(_) { emailInput.value = ''; }
        try { emailInput.focus(); } catch(_){}
      }
      var errorDiv = document.getElementById('email-error');
      if (errorDiv) { errorDiv.style.display = 'none'; }
    }
  };

  window.cancelarEmail = function() {
    var emailSection = document.getElementById('email-section');
    var errorDiv = document.getElementById('email-error');
    if (emailSection) emailSection.style.display = 'none';
    if (errorDiv) errorDiv.style.display = 'none';
  };

  window.enviarPorEmail = function() {
    var emailInput = document.getElementById('email-input');
    var errorDiv = document.getElementById('email-error');
    if (!emailInput || !errorDiv) return;

    var email = (emailInput.value || '').trim();
    if (!email) {
      errorDiv.textContent = 'Por favor, digite um e-mail.';
      errorDiv.style.display = 'block';
      try { emailInput.focus(); } catch(_){}
      return;
    }
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      errorDiv.textContent = 'Por favor, digite um e-mail válido.';
      errorDiv.style.display = 'block';
      try { emailInput.focus(); } catch(_){}
      return;
    }

    // Monta e submete o form
    var form = document.createElement('form');
    form.method = 'POST';
    var enviarEmailUrl = document.getElementById('enviarEmailUrl');
    form.action = enviarEmailUrl ? enviarEmailUrl.value : '';

    var campos = [
      { name: 'numero', value: boletoSelecionado.numero },
      { name: 'recto', value: boletoSelecionado.recto },
      { name: 'cobranca', value: boletoSelecionado.cobranca },
      { name: 'contrato', value: boletoSelecionado.contrato },
      { name: 'email', value: email }
    ];
    for (var i=0;i<campos.length;i++) {
      var input = document.createElement('input');
      input.type = 'hidden';
      input.name = campos[i].name;
      input.value = campos[i].value;
      form.appendChild(input);
    }

    document.body.appendChild(form);
    // Esconde a seção imediatamente para evitar reexibição durante navegação
    var emailSection = document.getElementById('email-section');
    if (emailSection) { emailSection.style.display = 'none'; }
    showSpinner();
    try { form.submit(); } catch(_) { hideSpinner(); }
  };

  window.copiarLinhaDigitavel = function(linhaDigitavel) {
    if (!linhaDigitavel) return;
    showSpinner();
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(linhaDigitavel).then(function() {
        hideSpinner();
      }).catch(function() {
          copiarTextoFallback(linhaDigitavel);
          Swal.fire({ icon: 'success', title: 'Linha digitável copiada com sucesso!', width: '450px' });
      });
    } else {
      copiarTextoFallback(linhaDigitavel);
    }
  };

  function copiarTextoFallback(texto) {
    var ta = document.createElement('textarea');
    ta.value = texto || '';
    ta.style.position = 'fixed';
    ta.style.left = '-999999px';
    ta.style.top = '-999999px';
    document.body.appendChild(ta);
    try { ta.focus(); ta.select(); } catch(_){}
    try { document.execCommand('copy'); } catch(_){}
    document.body.removeChild(ta);
    hideSpinner();
  }
})();

// Funções de email para Informe de Rendimentos (UI inline, seguindo mesma linha dos Boletos)
(function() {
  window.mostrarCampoEmailInforme = function() {
    var section = document.getElementById('informe-email-section');
    var input = document.getElementById('informe-email-input');
    var error = document.getElementById('informe-email-error');
    if (section) { section.style.display = 'block'; }
    if (error) { error.style.display = 'none'; error.textContent = ''; }
    if (input) {
      try { input.value = (window.DEFAULT_CLIENT_EMAIL || '').trim(); } catch(_) { input.value = ''; }
      try { input.focus(); } catch(_){}
    }
  };

  window.cancelarEmailInforme = function() {
    var section = document.getElementById('informe-email-section');
    var error = document.getElementById('informe-email-error');
    if (section) { section.style.display = 'none'; }
    if (error) { error.style.display = 'none'; error.textContent = ''; }
  };

  // Auto-exibir campo de email quando configurado pelo servidor
  try {
    if (window.SHOW_EMAIL === true) {
      window.mostrarCampoEmailInforme();
    }
  } catch(_) {}

  window.enviarInformeEmail = function() {
    var input = document.getElementById('informe-email-input');
    var error = document.getElementById('informe-email-error');
    var anoEl = document.getElementById('ano');
    if (!input || !anoEl || !error) return;

    var email = (input.value || '').trim();
    if (!email) {
      error.textContent = 'Por favor, digite um e-mail.';
      error.style.display = 'block';
      try { input.focus(); } catch(_){}
      return;
    }
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      error.textContent = 'Por favor, digite um e-mail válido.';
      error.style.display = 'block';
      try { input.focus(); } catch(_){}
      return;
    }

    var urlEl = document.getElementById('informeEnviarEmailUrl');
    var actionUrl = urlEl ? urlEl.value : '';
    if (!actionUrl) {
      error.textContent = 'URL de envio não configurada.';
      error.style.display = 'block';
      return;
    }

    var form = document.createElement('form');
    form.method = 'POST';
    form.action = actionUrl;

    var inputs = [
      { name: 'ano', value: anoEl.value || '' },
      { name: 'email', value: email },
      { name: 'action', value: 'email' }
    ];
    for (var i=0; i<inputs.length; i++) {
      var hidden = document.createElement('input');
      hidden.type = 'hidden';
      hidden.name = inputs[i].name;
      hidden.value = inputs[i].value;
      form.appendChild(hidden);
    }

    document.body.appendChild(form);
    // Esconde a seção imediatamente para evitar reexibição durante navegação
    var section = document.getElementById('informe-email-section');
    if (section) { section.style.display = 'none'; }
  showSpinner();
    try { form.submit(); } catch(_) { hideSpinner(); }
  };

  // Compatibilidade: caso alguma view ainda referencie a função antiga
  // enviarInformePorEmail (antes baseado em prompt), mantemos um alias
  // que apenas abre o campo inline.
  if (!window.enviarInformePorEmail) {
    window.enviarInformePorEmail = function() {
      try { window.mostrarCampoEmailInforme(); } catch(_){}
    };
  }
})();

// Removido: redirecionamento automático baseado em alertas de sucesso.
// O redirecionamento para a página de mensagem agora é feito servidor-side nos Controllers.
function mensagemPadrao(tipo, titulo, mensagem) {
    Swal.fire({
        title: titulo,
        text: mensagem,
        icon: tipo
    });
};

function EnviarEmailPdf(email, dados, gerarPdfUrl) {
    // dados: objeto recebido da view contendo informações necessárias (ex.: dadosunidadesp7)
    // Montaremos o payload final combinando os dados recebidos com o e-mail digitado.
    Swal.fire({
        title: "Enviar e-mail",
        text: "Digite o e-mail para envio do PDF",
        input: "text",
        inputValue: email || "",
        inputAttributes: {
            autocapitalize: "off"
        },
        showCancelButton: true,
        cancelButtonText: "Cancelar",
        confirmButtonText: "Enviar",
        showLoaderOnConfirm: true,
        preConfirm: async (emailDigitado) => {
                email = emailDigitado;    
                try {
                if (!gerarPdfUrl || typeof gerarPdfUrl !== 'string' || !gerarPdfUrl.trim()) {
                    return Swal.showValidationMessage('URL de geração de PDF não configurada.');
                }
                // Monta o payload usando o objeto recebido e o e-mail informado
                var payload = Object.assign({}, (dados || {}), { email: emailDigitado });

                const response = await fetch(gerarPdfUrl, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(payload)
                });

                if (!response.ok) {
                    return Swal.showValidationMessage(`
              Erro na requisição: ${response.status} - ${response.statusText}
            `);
                }

                return response.json();
            } catch (error) {
                Swal.showValidationMessage(`
            Falha na requisição: ${error}
          `);
            }
        },
        allowOutsideClick: () => !Swal.isLoading()
        }).then((result) => {
            if (result.isConfirmed) {
                if (result.value.sucesso === 1) {
                    // Sucesso - Mostrar o PDF
                    Swal.fire({
                        icon: 'success',
                        text: `PDF enviado com sucesso para o e-mail ${email}`,
                        // html: `<iframe src="${result.value.url}" width="100%" height="500px" style="border: none;"></iframe>`,
                        // width: '80%',
                        confirmButtonText: "Fechar"
                    });
                } else {
                // Erro - Mostrar mensagem de erro
                Swal.fire({
                    title: "Erro",
                    text: result.value.mensagem,
                    icon: "error"
                });
            }
        }
    });
}


