@echo off
echo ========================================
echo  Instalando PoliviewCRM.Cypress
echo ========================================
echo.

echo Verificando se Node.js está instalado...
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERRO: Node.js não está instalado!
    echo Por favor, instale o Node.js antes de continuar.
    echo Download: https://nodejs.org/
    pause
    exit /b 1
)

echo Node.js encontrado!
echo Versão: 
node --version
echo.

echo Verificando versão do Node.js...
for /f "tokens=1 delims=v" %%a in ('node --version') do set NODE_VERSION=%%a
for /f "tokens=1,2 delims=." %%a in ("%NODE_VERSION%") do (
    set MAJOR_VERSION=%%a
    set MINOR_VERSION=%%b
)

if %MAJOR_VERSION% LSS 12 (
    echo AVISO: Sua versão do Node.js (%NODE_VERSION%) é muito antiga!
    echo Recomendamos Node.js 14.x ou superior para melhor compatibilidade.
    echo Download: https://nodejs.org/
    echo.
    echo Continuando com versão compatível do Cypress...
    echo.
)

echo Limpando instalações anteriores...
npm cache clean --force >nul 2>&1

echo Removendo dependências antigas...
rmdir /s /q node_modules >nul 2>&1
del package-lock.json >nul 2>&1

echo Instalando dependências do npm...
npm install

if %errorlevel% neq 0 (
    echo ERRO: Falha na instalação das dependências!
    echo.
    echo Tentando com versão compatível do Cypress...
    npm install --save-dev cypress@12.17.4
    
    if %errorlevel% neq 0 (
        echo ERRO: Falha na instalação mesmo com versão compatível!
        echo.
        echo Soluções possíveis:
        echo 1. Atualize o Node.js para versão 14+ ou 18+
        echo 2. Execute: resolver-problemas.bat
        echo 3. Instale manualmente: npm install --save-dev cypress@12.17.4
        pause
        exit /b 1
    )
)

echo.
echo Verificando instalação do Cypress...
npx cypress verify >nul 2>&1
if %errorlevel% neq 0 (
    echo AVISO: Cypress não foi verificado corretamente.
    echo Isso pode causar problemas na execução dos testes.
    echo.
    echo Execute: resolver-problemas.bat para corrigir
    echo.
)

echo.
echo ========================================
echo  Instalação concluída com sucesso!
echo ========================================
echo.
echo Versões instaladas:
echo Node.js: 
node --version
echo npm: 
npm --version
echo Cypress: 
npm list cypress --depth=0 2>nul | findstr cypress
echo.
echo Para executar os testes:
echo   - Modo interativo: npm run cypress:open
echo   - Modo headless:   npm run cypress:run
echo.
echo Se houver problemas, execute: resolver-problemas.bat
echo.
echo Certifique-se de que a API está rodando na porta 9533
echo.
pause 