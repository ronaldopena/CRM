// ***********************************************************
// Este arquivo é processado e carregado automaticamente antes
// dos arquivos de teste. É um ótimo lugar para colocar
// comportamentos globais e comandos customizados.
// ***********************************************************

// Import commands.js using ES2015 syntax:
import './commands'

// Alternatively you can use CommonJS syntax:
// require('./commands')

// Configurações globais
Cypress.on('uncaught:exception', (err, runnable) => {
  // returning false here prevents Cypress from
  // failing the test
  return false
}) 