# Executar todos os testes do AcessoService

dotnet test poliview.testes/ --filter "AcessoService" --logger "console;verbosity=normal"

# Executar apenas testes unitários (sem integração)

dotnet test poliview.testes/ --filter "AcessoService&Category!=Integration"
