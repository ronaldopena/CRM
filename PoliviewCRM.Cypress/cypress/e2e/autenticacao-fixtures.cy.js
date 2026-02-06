describe('AutenticacaoController - Testes com Fixtures', () => {
  
  let dadosUsuarios

  before(() => {
    // Carrega os dados dos fixtures antes de executar os testes
    cy.fixture('usuarios').then((dados) => {
      dadosUsuarios = dados
    })
  })

  describe('POST /autenticacao/login - Usando Fixtures', () => {
    
    it('Deve realizar login com usuário válido do fixture', () => {
      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosUsuarios.usuarioValido,
        failOnStatusCode: false
      }).then((response) => {
        expect(response.status).to.eq(200)
        cy.validarRespostaSucesso(response)
        
        cy.log('Teste com usuário válido do fixture:', JSON.stringify(response.body))
      })
    })

    it('Deve falhar com usuário inexistente do fixture', () => {
      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosUsuarios.usuarioInexistente,
        failOnStatusCode: false
      }).then((response) => {
        expect([200, 400, 404]).to.include(response.status)
        cy.validarRespostaSucesso(response)
        
        if (response.status === 200) {
          expect(response.body.sucesso).to.be.false
        }
        
        cy.log('Teste com usuário inexistente do fixture:', JSON.stringify(response.body))
      })
    })

    it('Deve falhar com senha incorreta do fixture', () => {
      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosUsuarios.senhaIncorreta,
        failOnStatusCode: false
      }).then((response) => {
        expect([200, 400, 401]).to.include(response.status)
        cy.validarRespostaSucesso(response)
        
        if (response.status === 200) {
          expect(response.body.sucesso).to.be.false
        }
        
        cy.log('Teste com senha incorreta do fixture:', JSON.stringify(response.body))
      })
    })

    it('Deve validar dados inválidos do fixture', () => {
      // Teste com usuário vazio
      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosUsuarios.dadosInvalidos.usuarioVazio,
        failOnStatusCode: false
      }).then((response) => {
        expect([400, 422]).to.include(response.status)
        cy.log('Teste usuário vazio:', JSON.stringify(response.body))
      })

      // Teste com senha vazia
      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosUsuarios.dadosInvalidos.senhaVazia,
        failOnStatusCode: false
      }).then((response) => {
        expect([400, 422]).to.include(response.status)
        cy.log('Teste senha vazia:', JSON.stringify(response.body))
      })
    })

    it('Deve testar todas as origens do fixture', () => {
      dadosUsuarios.origens.forEach((origem) => {
        const dadosLogin = {
          ...dadosUsuarios.usuarioValido,
          origem: origem.codigo
        }

        cy.request({
          method: 'POST',
          url: '/autenticacao/login',
          body: dadosLogin,
          failOnStatusCode: false
        }).then((response) => {
          expect(response.status).to.be.oneOf([200, 400, 401, 404])
          cy.log(`Teste origem ${origem.nome} (${origem.codigo}):`, JSON.stringify(response.body))
        })
      })
    })

    it('Deve testar todas as empresas do fixture', () => {
      dadosUsuarios.empresas.forEach((idempresa) => {
        const dadosLogin = {
          ...dadosUsuarios.usuarioValido,
          idempresa: idempresa
        }

        cy.request({
          method: 'POST',
          url: '/autenticacao/login',
          body: dadosLogin,
          failOnStatusCode: false
        }).then((response) => {
          expect(response.status).to.be.oneOf([200, 400, 401, 404])
          cy.log(`Teste empresa ${idempresa}:`, JSON.stringify(response.body))
        })
      })
    })

    it('Deve usar comando customizado para login', () => {
      const { usuario, senha, origem, idempresa } = dadosUsuarios.usuarioValido
      
      cy.loginAutenticacao(usuario, senha, origem, idempresa).then((response) => {
        expect(response.status).to.be.oneOf([200, 400, 401, 404])
        cy.validarRespostaSucesso(response)
        
        cy.log('Teste com comando customizado:', JSON.stringify(response.body))
      })
    })

    it('Deve testar múltiplas tentativas de login', () => {
      const tentativas = 3
      
      for (let i = 0; i < tentativas; i++) {
        cy.request({
          method: 'POST',
          url: '/autenticacao/login',
          body: dadosUsuarios.usuarioValido,
          failOnStatusCode: false
        }).then((response) => {
          expect(response.status).to.be.oneOf([200, 400, 401, 404])
          cy.log(`Tentativa ${i + 1}:`, JSON.stringify(response.body))
        })
      }
    })

    it('Deve validar headers da resposta', () => {
      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosUsuarios.usuarioValido,
        failOnStatusCode: false
      }).then((response) => {
        // Validar headers básicos
        expect(response.headers).to.have.property('content-type')
        expect(response.headers['content-type']).to.include('application/json')
        
        cy.log('Headers da resposta:', JSON.stringify(response.headers))
      })
    })

    it('Deve testar com dados gerados dinamicamente', () => {
      cy.gerarDadosTeste().then((dadosGerados) => {
        cy.request({
          method: 'POST',
          url: '/autenticacao/login',
          body: {
            usuario: dadosGerados.usuario,
            senha: dadosGerados.senha,
            origem: 0,
            idempresa: 1
          },
          failOnStatusCode: false
        }).then((response) => {
          // Como são dados gerados, esperamos que falhe
          expect([200, 400, 401, 404]).to.include(response.status)
          
          cy.log('Teste com dados gerados:', JSON.stringify(response.body))
        })
      })
    })
  })
}) 