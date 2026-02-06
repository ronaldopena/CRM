const { defineConfig } = require('cypress')

module.exports = defineConfig({
  e2e: {
    baseUrl: 'http://localhost:9533',
    supportFile: 'cypress/support/e2e.js',
    specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}',
    video: true,
    screenshotOnRunFailure: true,
    viewportWidth: 1280,
    viewportHeight: 720,
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
    responseTimeout: 10000,
    pageLoadTimeout: 10000,
    setupNodeEvents(on, config) {
      // implement node event listeners here
      
      // Configurações para melhor compatibilidade
      on('before:browser:launch', (browser = {}, launchOptions) => {
        // Configurações específicas para diferentes browsers
        if (browser.name === 'chrome') {
          launchOptions.args.push('--disable-dev-shm-usage')
          launchOptions.args.push('--no-sandbox')
        }
        
        return launchOptions
      })
      
      return config
    },
  },
  
  // Configurações adicionais para compatibilidade
  env: {
    NODE_ENV: 'test'
  },
  
  // Configurações de retry para testes mais estáveis
  retries: {
    runMode: 2,
    openMode: 0
  }
})