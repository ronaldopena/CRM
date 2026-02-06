@echo off
echo ========================================
echo  Executando Testes - PoliviewCRM.Cypress
echo ========================================
echo.

echo Verificando se a API está rodando...
curl -s http://localhost:9533 >nul 2>&1
if %errorlevel% neq 0 (
    echo AVISO: API não está respondendo na porta 9533
    echo Certifique-se de que Poliview.crm.api está rodando
    echo.
)

echo Escolha como executar os testes:
echo 1. Modo interativo (abre interface do Cypress)
echo 2. Modo headless (todos os testes)
echo 3. Apenas testes básicos de autenticação
echo 4. Apenas testes com fixtures
echo 5. Apenas testes detalhados do controller
echo 6. Apenas testes com usuários reais do sistema
echo 7. Relatório completo (todos os testes com relatório)
echo.
set /p opcao="Digite sua opção (1-7): "

if "%opcao%"=="1" (
    echo Abrindo Cypress em modo interativo...
    npm run cypress:open
    
) else if "%opcao%"=="2" (
    echo Executando todos os testes em modo headless...
    npm run cypress:run
    
) else if "%opcao%"=="3" (
    echo Executando testes básicos de autenticação...
    npx cypress run --spec "cypress/e2e/autenticacao.cy.js"
    
) else if "%opcao%"=="4" (
    echo Executando testes com fixtures...
    npx cypress run --spec "cypress/e2e/autenticacao-fixtures.cy.js"
    
) else if "%opcao%"=="5" (
    echo Executando testes detalhados do controller...
    npx cypress run --spec "cypress/e2e/autenticacao-controller.cy.js"
    
) else if "%opcao%"=="6" (
    echo Executando testes com usuários reais do sistema...
    npx cypress run --spec "cypress/e2e/autenticacao-usuarios-reais.cy.js"
    
) else if "%opcao%"=="7" (
    echo Executando todos os testes com relatório...
    npx cypress run --reporter spec --reporter-options "verbose=true"
    
) else (
    echo Opção inválida!
    pause
    exit /b 1
)

echo.
echo ========================================
echo  Execução concluída!
echo ========================================
echo.

if "%opcao%"=="2" (
    echo Relatórios salvos em:
    echo   - Videos: cypress/videos/
    echo   - Screenshots: cypress/screenshots/
) else if "%opcao%"=="7" (
    echo Relatório detalhado exibido acima
)

echo.
echo Para ver os resultados detalhados:
echo 1. Videos e screenshots estão em cypress/videos/ e cypress/screenshots/
echo 2. Use modo interativo (opção 1) para debug
echo 3. Logs detalhados no console acima
echo.
pause 