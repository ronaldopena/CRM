// ***********************************************
// Comandos customizados para testes da API
// ***********************************************

// Comando para fazer login na API de autenticação
Cypress.Commands.add('loginAutenticacao', (usuario, senha, origem = 0, idempresa = 1) => {
  return cy.request({
    method: 'POST',
    url: '/autenticacao/login',
    body: {
      usuario: usuario,
      senha: senha,
      origem: origem,
      idempresa: idempresa
    },
    failOnStatusCode: false
  })
})

// Comando para validar estrutura de resposta de sucesso
Cypress.Commands.add('validarRespostaSucesso', (response) => {
  expect(response.body).to.have.property('sucesso')
  expect(response.body).to.have.property('mensagem')
  expect(response.body).to.have.property('status')
})

// Comando para validar estrutura de usuário na resposta
Cypress.Commands.add('validarUsuarioResposta', (usuario) => {
  expect(usuario).to.have.property('CD_USUARIO')
  expect(usuario).to.have.property('NM_USUARIO')
  expect(usuario).to.have.property('DS_EMAIL')
  expect(usuario).to.have.property('NR_CPFCNPJ')
  expect(usuario).to.have.property('token')
})

// Comando para gerar dados de teste aleatórios
Cypress.Commands.add('gerarDadosTeste', () => {
  const timestamp = Date.now()
  return {
    usuario: `teste${timestamp}@poliview.com.br`,
    senha: 'senha123',
    cpf: '12345678901'
  }
}) 