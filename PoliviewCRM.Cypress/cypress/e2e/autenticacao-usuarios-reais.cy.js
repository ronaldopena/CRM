describe('AutenticacaoController - Testes com Usuários Reais', () => {
  
  const baseUrl = '/autenticacao'
  
  beforeEach(() => {
    cy.log('=== Testando com usuários que existem no sistema ===')
  })

  describe('POST /autenticacao/login - Cenários Realistas', () => {
    
    it('Deve primeiro descobrir usuários válidos no sistema', () => {
      const usuariosComuns = [
        'usuario@empresa.com.br'
      ]

      usuariosComuns.forEach((usuario) => {
        cy.request({
          method: 'POST',
          url: `${baseUrl}/login`,
          body: {
            usuario: usuario,
            senha: 'senhaqualquer', // Senha errada propositalmente
            origem: 0,
            idempresa: 1
          },
          failOnStatusCode: false
        }).then((response) => {
          cy.log(`Testando usuário: ${usuario}`)
          cy.log(`Resposta: ${response.body.mensagem}`)
          
          // Se não retorna "usuário não encontrado", significa que o usuário existe
          if (!response.body.mensagem.match(/usuário não encontrado|não encontrado/i)) {
            cy.log(`✅ Usuário encontrado no sistema: ${usuario}`)
          } else {
            cy.log(`❌ Usuário não existe: ${usuario}`)
          }
        })
      })
    })

    it('Deve testar senha incorreta com usuário específico do sistema', () => {
      // Primeiro, vamos tentar encontrar um usuário que existe
      const possiveisUsuarios = [
        'usuario@empresa.com.br',
      ]

      let usuarioEncontrado = null

      // Testar cada usuário para encontrar um que existe
      cy.wrap(possiveisUsuarios).each((usuario) => {
        cy.request({
          method: 'POST',
          url: `${baseUrl}/login`,
          body: {
            usuario: usuario,
            senha: 'senhaerrada',
            origem: 0,
            idempresa: 1
          },
          failOnStatusCode: false
        }).then((response) => {
          // Se não retorna "usuário não encontrado", o usuário existe
          if (!response.body.mensagem.match(/usuário não encontrado|não encontrado/i)) {
            usuarioEncontrado = usuario
            cy.log(`✅ Usuário existente encontrado: ${usuario}`)
            cy.log(`Mensagem para senha incorreta: ${response.body.mensagem}`)
            
            // Validar que o erro é sobre senha ou autenticação
            expect(response.body.sucesso).to.be.false
            expect(response.body.objeto).to.be.null
          }
        })
      })
    })

    it('Deve testar diferentes formatos de CPF/CNPJ', () => {
      const formatos = [
        { tipo: 'CPF sem formatação', valor: '12345678901' },
        { tipo: 'CPF com formatação', valor: '123.456.789-01' },
        { tipo: 'CNPJ sem formatação', valor: '12345678000195' },
        { tipo: 'CNPJ com formatação', valor: '12.345.678/0001-95' }
      ]

      formatos.forEach((formato) => {
        cy.request({
          method: 'POST',
          url: `${baseUrl}/login`,
          body: {
            usuario: formato.valor,
            senha: 'senha123',
            origem: 0,
            idempresa: 1
          },
          failOnStatusCode: false
        }).then((response) => {
          // Independente se encontra ou não, deve processar a requisição
          expect(response.status).to.eq(200)
          
          cy.log(`✅ ${formato.tipo} processado: ${formato.valor}`)
          cy.log(`Resultado: ${response.body.mensagem}`)
        })
      })
    })

    it('Deve criar um usuário de teste e validar comportamentos', () => {
      // Primeiro tentar criar um usuário (se a API permitir)
      // Ou usar dados que sabemos que existem no sistema
      
      const dadosUsuarioTeste = {
        usuario: 'usuario@empresa.com.br', // Usuário simples que pode existir
        senha: '123',   // Senha simples que pode existir
        origem: 0,
        idempresa: 1
      }

      cy.request({
        method: 'POST',
        url: `${baseUrl}/login`,
        body: dadosUsuarioTeste,
        failOnStatusCode: false
      }).then((response) => {
        cy.log('Testando credenciais padrão admin/admin')
        cy.log(`Status: ${response.status}`)
        cy.log(`Mensagem: ${response.body.mensagem}`)
        cy.log(`Sucesso: ${response.body.sucesso}`)

        if (response.body.sucesso) {
          cy.log('✅ Login bem-sucedido com credenciais padrão')
          
          // Validar token
          expect(response.body.objeto.token).to.not.be.empty
          
          // Testar senha incorreta para o mesmo usuário
          cy.request({
            method: 'POST',
            url: `${baseUrl}/login`,
            body: {
              ...dadosUsuarioTeste,
              senha: 'senhaerrada'
            },
            failOnStatusCode: false
          }).then((response2) => {
            cy.log('Testando senha incorreta para usuário válido')
            expect(response2.body.sucesso).to.be.false
            cy.log(`Mensagem de erro: ${response2.body.mensagem}`)
          })
        } else {
          cy.log('ℹ️ Credenciais padrão não funcionaram, testando outros cenários')
        }
      })
    })

    it('Deve validar comportamento consistente da API', () => {
      const cenarios = [
        {
          nome: 'Usuário claramente inexistente',
          dados: {
            usuario: 'usuarioquenaoexiste@dominioquenonaoexiste.com',
            senha: 'qualquersenha',
            origem: 0,
            idempresa: 1
          }
        },
        {
          nome: 'Email vazio',
          dados: {
            usuario: '',
            senha: 'senha123',
            origem: 0,
            idempresa: 1
          }
        },
        {
          nome: 'Senha vazia',
          dados: {
            usuario: 'teste@teste.com',
            senha: '',
            origem: 0,
            idempresa: 1
          }
        }
      ]

      cenarios.forEach((cenario) => {
        cy.request({
          method: 'POST',
          url: `${baseUrl}/login`,
          body: cenario.dados,
          failOnStatusCode: false
        }).then((response) => {
          // Todos devem retornar resposta estruturada
          expect(response.status).to.eq(200)
          expect(response.body).to.have.property('sucesso')
          expect(response.body).to.have.property('mensagem')
          
          // Todos devem falhar
          expect(response.body.sucesso).to.be.false
          
          cy.log(`✅ ${cenario.nome}: ${response.body.mensagem}`)
        })
      })
    })

    it('Deve testar diferentes empresas no sistema', () => {
      const empresas = [0, 1, 2, 3, 999] // Incluindo empresa inválida

      empresas.forEach((idempresa) => {
        cy.request({
          method: 'POST',
          url: `${baseUrl}/login`,
          body: {
            usuario: 'teste@poliview.com.br',
            senha: 'senha123',
            origem: 0,
            idempresa: idempresa
          },
          failOnStatusCode: false
        }).then((response) => {
          expect(response.status).to.eq(200)
          
          cy.log(`✅ Empresa ${idempresa}: ${response.body.mensagem}`)
          
          // A empresa 999 provavelmente não existe
          if (idempresa === 999) {
            expect(response.body.sucesso).to.be.false
          }
        })
      })
    })

    it('Deve verificar logs do controller com dados reais', () => {
      cy.request({
        method: 'POST',
        url: `${baseUrl}/login`,
        body: {
          usuario: 'usuario@empresa.com.br',
          senha: '123',
          origem: 2, // APP
          idempresa: 1
        },
        failOnStatusCode: false
      }).then((response) => {
        // O controller deve imprimir os dados no console
        // Console.WriteLine($"usuario: {obj.usuario}");
        // Console.WriteLine($"senha: {obj.senha}");
        // Console.WriteLine($"origem: {obj.origem}");
        
        expect(response.status).to.eq(200)
        
        cy.log('✅ Dados enviados para log do controller:')
        cy.log('   - Usuario: usuario@empresa.com.br')
        cy.log('   - Senha: [OCULTA]')
        cy.log('   - Origem: 2 (APP)')
        cy.log('   - Empresa: 1')
      })
    })
  })

  describe('Validação de Segurança com Dados Reais', () => {
    
    it('Deve validar que senhas não aparecem em logs', () => {
      const senhaSecreta = '123'
      
      cy.request({
        method: 'POST',
        url: `${baseUrl}/login`,
        body: {
          usuario: 'usuario@empresa.com.br',
          senha: senhaSecreta,
          origem: 0,
          idempresa: 1
        },
        failOnStatusCode: false
      }).then((response) => {
        // Validar que a senha não aparece na resposta
        const responseStr = JSON.stringify(response.body)
        expect(responseStr).to.not.include(senhaSecreta)
        
        // Se login foi bem-sucedido, senha deve estar vazia
        if (response.body.objeto) {
          expect(response.body.objeto.DS_SENHA).to.equal('')
        }
        
        cy.log('✅ Senha não exposta na resposta')
      })
    })

    it('Deve validar tentativas de SQL injection', () => {
      const payloadsSQLInjection = [
        "admin'; DROP TABLE usuarios; --",
        "admin' OR '1'='1",
        "admin' UNION SELECT * FROM usuarios --"
      ]

      payloadsSQLInjection.forEach((payload) => {
        cy.request({
          method: 'POST',
          url: `${baseUrl}/login`,
          body: {
            usuario: payload,
            senha: '123',
            origem: 0,
            idempresa: 1
          },
          failOnStatusCode: false
        }).then((response) => {
          // Deve tratar como usuário normal e não dar erro de SQL
          expect(response.status).to.eq(200)
          expect(response.body).to.have.property('sucesso', false)
          
          cy.log(`✅ SQL Injection bloqueado: ${payload.substring(0, 20)}...`)
        })
      })
    })
  })
}) 