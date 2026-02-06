describe('AutenticacaoController - Testes da API', () => {
  
  beforeEach(() => {
    // Configurações que executam antes de cada teste
    cy.log('Iniciando teste do AutenticacaoController')
  })

  describe('POST /autenticacao/login', () => {
    
    it('Deve realizar login com sucesso com dados válidos', () => {
      const dadosLogin = {
        usuario: 'usuario.teste@poliview.com.br',
        senha: 'senha123',
        origem: 0,
        idempresa: 1
      }

      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosLogin,
        failOnStatusCode: false
      }).then((response) => {
        // Validar status da resposta
        expect(response.status).to.eq(200)
        
        // Validar estrutura da resposta
        cy.validarRespostaSucesso(response)
        
        // Validar que a resposta contém um objeto
        expect(response.body).to.have.property('objeto')
        
        // Se login foi bem-sucedido, validar estrutura do usuário
        if (response.body.sucesso && response.body.objeto) {
          cy.validarUsuarioResposta(response.body.objeto)
          expect(response.body.objeto.token).to.not.be.empty
        }
        
        // Log dos dados para debug
        cy.log('Resposta do login:', JSON.stringify(response.body))
      })
    })

    it('Deve retornar erro com usuário inexistente', () => {
      const dadosLogin = {
        usuario: 'usuario.inexistente@teste.com',
        senha: 'senha123',
        origem: 0,
        idempresa: 1
      }

      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosLogin,
        failOnStatusCode: false
      }).then((response) => {
        // Pode retornar 200 com sucesso=false ou um status de erro
        expect([200, 400, 404]).to.include(response.status)
        
        // Validar estrutura da resposta
        cy.validarRespostaSucesso(response)
        
        // Se retornou 200, deve ter sucesso=false
        if (response.status === 200) {
          expect(response.body.sucesso).to.be.false
          expect(response.body.mensagem).to.contain('não encontrado')
        }
        
        cy.log('Resposta usuário inexistente:', JSON.stringify(response.body))
      })
    })

    it('Deve retornar erro com senha incorreta', () => {
      const dadosLogin = {
        usuario: 'usuario.teste@poliview.com.br',
        senha: 'senhaIncorreta123',
        origem: 0,
        idempresa: 1
      }

      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosLogin,
        failOnStatusCode: false
      }).then((response) => {
        // Pode retornar 200 com sucesso=false ou um status de erro
        expect([200, 400, 401]).to.include(response.status)
        
        // Validar estrutura da resposta
        cy.validarRespostaSucesso(response)
        
        // Se retornou 200, deve ter sucesso=false
        if (response.status === 200) {
          expect(response.body.sucesso).to.be.false
          expect(response.body.mensagem).to.match(/senha|incorreta|inválida/i)
        }
        
        cy.log('Resposta senha incorreta:', JSON.stringify(response.body))
      })
    })

    it('Deve validar campos obrigatórios - usuário vazio', () => {
      const dadosLogin = {
        usuario: '',
        senha: 'senha123',
        origem: 0,
        idempresa: 1
      }

      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosLogin,
        failOnStatusCode: false
      }).then((response) => {
        // Deve retornar erro de validação
        expect([400, 422]).to.include(response.status)
        
        cy.log('Resposta usuário vazio:', JSON.stringify(response.body))
      })
    })

    it('Deve validar campos obrigatórios - senha vazia', () => {
      const dadosLogin = {
        usuario: 'usuario.teste@poliview.com.br',
        senha: '',
        origem: 0,
        idempresa: 1
      }

      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosLogin,
        failOnStatusCode: false
      }).then((response) => {
        // Deve retornar erro de validação
        expect([400, 422]).to.include(response.status)
        
        cy.log('Resposta senha vazia:', JSON.stringify(response.body))
      })
    })

    it('Deve testar diferentes origens válidas', () => {
      const origens = [
        { codigo: 0, nome: 'PORTAL' },
        { codigo: 1, nome: 'CRM' },
        { codigo: 2, nome: 'APP' },
        { codigo: 3, nome: 'MOBUSS' }
      ]

      origens.forEach((origem) => {
        const dadosLogin = {
          usuario: 'usuario.teste@poliview.com.br',
          senha: 'senha123',
          origem: origem.codigo,
          idempresa: 1
        }

        cy.request({
          method: 'POST',
          url: '/autenticacao/login',
          body: dadosLogin,
          failOnStatusCode: false
        }).then((response) => {
          // Validar que a requisição foi processada
          expect(response.status).to.be.oneOf([200, 400, 401, 404])
          
          cy.log(`Teste origem ${origem.nome} (${origem.codigo}):`, JSON.stringify(response.body))
        })
      })
    })

    it('Deve testar com diferentes empresas', () => {
      const empresas = [1, 2, 3]

      empresas.forEach((idempresa) => {
        const dadosLogin = {
          usuario: 'usuario.teste@poliview.com.br',
          senha: 'senha123',
          origem: 0,
          idempresa: idempresa
        }

        cy.request({
          method: 'POST',
          url: '/autenticacao/login',
          body: dadosLogin,
          failOnStatusCode: false
        }).then((response) => {
          // Validar que a requisição foi processada
          expect(response.status).to.be.oneOf([200, 400, 401, 404])
          
          cy.log(`Teste empresa ${idempresa}:`, JSON.stringify(response.body))
        })
      })
    })

    it('Deve validar estrutura do JWT token quando login é bem-sucedido', () => {
      const dadosLogin = {
        usuario: 'usuario.teste@poliview.com.br',
        senha: 'senha123',
        origem: 0,
        idempresa: 1
      }

      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosLogin,
        failOnStatusCode: false
      }).then((response) => {
        // Se login foi bem-sucedido
        if (response.status === 200 && response.body.sucesso && response.body.objeto) {
          const token = response.body.objeto.token
          
          // Validar que o token existe e não está vazio
          expect(token).to.exist
          expect(token).to.not.be.empty
          
          // Validar estrutura básica do JWT (3 partes separadas por ponto)
          const tokenParts = token.split('.')
          expect(tokenParts).to.have.length(3)
          
          cy.log('Token JWT válido encontrado')
        } else {
          cy.log('Login não foi bem-sucedido, pulando validação do token')
        }
      })
    })

    it('Deve testar performance da API de login', () => {
      const dadosLogin = {
        usuario: 'usuario.teste@poliview.com.br',
        senha: 'senha123',
        origem: 0,
        idempresa: 1
      }

      const startTime = Date.now()

      cy.request({
        method: 'POST',
        url: '/autenticacao/login',
        body: dadosLogin,
        failOnStatusCode: false
      }).then((response) => {
        const endTime = Date.now()
        const responseTime = endTime - startTime

        // Validar que a resposta foi retornada em menos de 5 segundos
        expect(responseTime).to.be.lessThan(5000)
        
        cy.log(`Tempo de resposta: ${responseTime}ms`)
      })
    })
  })
}) 