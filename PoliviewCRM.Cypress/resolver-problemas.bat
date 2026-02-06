@echo off
echo ========================================
echo  Resolvendo Problemas - PoliviewCRM.Cypress
echo ========================================
echo.

echo Verificando versão do Node.js...
node --version
echo.

echo Verificando versão do npm...
npm --version
echo.

echo Escolha a ação para resolver o problema:
echo 1. Limpar cache do npm e reinstalar
echo 2. Usar versão compatível do Cypress (12.x)
echo 3. Verificar compatibilidade de versões
echo 4. Reinstalação completa
echo 5. Mostrar informações do sistema
echo.
set /p opcao="Digite sua opção (1-5): "

if "%opcao%"=="1" (
    echo Limpando cache do npm...
    npm cache clean --force
    
    echo Removendo node_modules...
    rmdir /s /q node_modules 2>nul
    
    echo Removendo package-lock.json...
    del package-lock.json 2>nul
    
    echo Reinstalando dependências...
    npm install
    
) else if "%opcao%"=="2" (
    echo Instalando versão compatível do Cypress...
    npm uninstall cypress
    npm install --save-dev cypress@12.17.4
    
) else if "%opcao%"=="3" (
    echo Verificando compatibilidade...
    echo.
    echo === Informações do Sistema ===
    node --version
    npm --version
    echo.
    echo === Versões Recomendadas ===
    echo Node.js: 14.x ou superior (recomendado: 18.x)
    echo npm: 6.x ou superior
    echo Cypress: 12.x para Node.js ^lt; 14, ou 13.x+ para Node.js 14+
    echo.
    
) else if "%opcao%"=="4" (
    echo Realizando reinstalação completa...
    
    echo Removendo arquivos antigos...
    rmdir /s /q node_modules 2>nul
    del package-lock.json 2>nul
    
    echo Limpando cache...
    npm cache clean --force
    
    echo Instalando versão compatível...
    npm install --save-dev cypress@12.17.4
    
    echo Verificando instalação...
    npx cypress verify
    
) else if "%opcao%"=="5" (
    echo === Informações Detalhadas do Sistema ===
    echo.
    echo Node.js:
    node --version
    echo.
    echo npm:
    npm --version
    echo.
    echo Sistema Operacional:
    ver
    echo.
    echo Arquitetura:
    echo %PROCESSOR_ARCHITECTURE%
    echo.
    echo Variáveis de Ambiente Node:
    echo NODE_ENV: %NODE_ENV%
    echo.
    echo Cypress instalado:
    npm list cypress 2>nul || echo Cypress não encontrado
    
) else (
    echo Opção inválida!
    pause
    exit /b 1
)

echo.
echo ========================================
echo  Resolução concluída!
echo ========================================
echo.

if "%opcao%"=="1" (
    echo Tente executar os testes novamente:
    echo   npm run cypress:open
) else if "%opcao%"=="2" (
    echo Cypress 12.x instalado. Tente executar:
    echo   npm run cypress:open
) else if "%opcao%"=="4" (
    echo Reinstalação completa. Tente executar:
    echo   npm run cypress:open
)

echo.
echo Se o problema persistir:
echo 1. Atualize o Node.js para versão 14+ ou 18+
echo 2. Use: npm install --save-dev cypress@12.17.4
echo 3. Verifique: npx cypress verify
echo.
pause 