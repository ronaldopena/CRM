<#
.SYNOPSIS
    Script master para publicar todos os projetos do Poliview CRM
.DESCRIPTION
    Executa todos os scripts de publicacao individuais e gera relatorio de sucessos/falhas
.EXAMPLE
    .\publicar-todos.ps1
#>

# Configurar codificacao para caracteres especiais
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Cores para output
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"
$HeaderColor = "Magenta"

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Header {
    param([string]$Title)
    Write-Host ""
    Write-ColorOutput ("=" * 80) $HeaderColor
    Write-ColorOutput "  $Title" $HeaderColor
    Write-ColorOutput ("=" * 80) $HeaderColor
    Write-Host ""
}

function Execute-ProjectScript {
    param(
        [string]$ProjectName,
        [string]$ScriptPath,
        [string]$WorkingDirectory
    )

    Write-ColorOutput "Executando publicacao: $ProjectName" $InfoColor
    Write-ColorOutput "Diretorio: $WorkingDirectory" $InfoColor
    Write-ColorOutput "Script: $ScriptPath" $InfoColor

    try {
        # Salvar diretorio atual
        $originalLocation = Get-Location

        # Mudar para diretorio do projeto
        Set-Location $WorkingDirectory

        # Executar script
        $startTime = Get-Date
        & powershell -ExecutionPolicy Bypass -File $ScriptPath
        $endTime = Get-Date
        $duration = $endTime - $startTime

        # Restaurar diretorio original
        Set-Location $originalLocation

        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "[OK] $ProjectName - SUCESSO (Tempo: $($duration.ToString('mm\:ss')))" $SuccessColor
            return @{
                Project  = $ProjectName
                Status   = "SUCESSO"
                Duration = $duration
                Error    = $null
            }
        }
        else {
            Write-ColorOutput "[ERRO] $ProjectName - FALHA (Codigo: $LASTEXITCODE)" $ErrorColor
            return @{
                Project  = $ProjectName
                Status   = "FALHA"
                Duration = $duration
                Error    = "Codigo de saida: $LASTEXITCODE"
            }
        }
    }
    catch {
        # Restaurar diretorio original em caso de erro
        Set-Location $originalLocation

        Write-ColorOutput "[ERRO] $ProjectName - ERRO: $($_.Exception.Message)" $ErrorColor
        return @{
            Project  = $ProjectName
            Status   = "ERRO"
            Duration = $null
            Error    = $_.Exception.Message
        }
    }
}

# Inicio do script principal
Write-Header "PUBLICACAO AUTOMATIZADA - POLIVIEW CRM"

$startTimeTotal = Get-Date
Write-ColorOutput "Iniciando publicacao automatizada de todos os projetos..." $InfoColor
Write-ColorOutput "Data/Hora: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" $InfoColor

# Verificar se arquivo versao.txt existe
$versaoFile = ".\versao.txt"
if (Test-Path $versaoFile) {
    $versao = Get-Content $versaoFile -Raw
    $versao = $versao.Trim()
    Write-ColorOutput "Versao detectada: $versao" $InfoColor
}
else {
    Write-ColorOutput "[ERRO] Arquivo versao.txt nao encontrado!" $ErrorColor
    exit 1
}

# Lista de projetos para publicar
$projetos = @(
    @{
        Name             = "Poliview.crm.servicos (.NET Framework)"
        ScriptPath       = ".\compilar-servicos.ps1"
        WorkingDirectory = "."
    },
    @{
        Name             = "Poliview.crm.integracao"
        ScriptPath       = ".\publicar.ps1"
        WorkingDirectory = ".\Poliview.crm.integracao"
    },
    @{
        Name             = "Poliview.crm.api"
        ScriptPath       = ".\publicar.ps1"
        WorkingDirectory = ".\Poliview.crm.api"
    },
    @{
        Name             = "poliview.crm.service.email"
        ScriptPath       = ".\publicar.ps1"
        WorkingDirectory = ".\poliview.crm.service.email"
    },
    @{
        Name             = "poliview.crm.sla"
        ScriptPath       = ".\publicar.ps1"
        WorkingDirectory = ".\poliview.crm.sla"
    },
    @{
        Name             = "Poliview.crm.monitor.service"
        ScriptPath       = ".\publicar.ps1"
        WorkingDirectory = ".\Poliview.crm.monitor.service"
    },
    @{
        Name             = "poliview.crm.instalador"
        ScriptPath       = ".\publicar.ps1"
        WorkingDirectory = ".\poliview.crm.instalador"
    }
)

# Verificar se todos os scripts existem antes de executar
Write-ColorOutput "Verificando scripts..." $InfoColor
$missingScripts = @()

foreach ($projeto in $projetos) {
    $fullScriptPath = Join-Path $projeto.WorkingDirectory $projeto.ScriptPath
    if (-not (Test-Path $fullScriptPath)) {
        $missingScripts += "$($projeto.Name): $fullScriptPath"
    }
}

if ($missingScripts.Count -gt 0) {
    Write-ColorOutput "[ERRO] Scripts nao encontrados:" $ErrorColor
    foreach ($missing in $missingScripts) {
        Write-ColorOutput "   - $missing" $ErrorColor
    }
    exit 1
}

Write-ColorOutput "[OK] Todos os scripts encontrados!" $SuccessColor

# Array para armazenar resultados
$resultados = @()

# Executar cada projeto
foreach ($projeto in $projetos) {
    Write-Header "PROJETO: $($projeto.Name)"

    $resultado = Execute-ProjectScript -ProjectName $projeto.Name -ScriptPath $projeto.ScriptPath -WorkingDirectory $projeto.WorkingDirectory
    $resultados += $resultado

    # Pausa entre execucoes para melhor visualizacao
    Start-Sleep -Seconds 2
}

# Relatorio final
$endTimeTotal = Get-Date
$durationTotal = $endTimeTotal - $startTimeTotal

Write-Header "RELATORIO FINAL"

Write-ColorOutput "Tempo total de execucao: $($durationTotal.ToString('hh\:mm\:ss'))" $InfoColor
Write-ColorOutput "Data/Hora final: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" $InfoColor

# Contar sucessos e falhas
$sucessos = $resultados | Where-Object { $_.Status -eq "SUCESSO" }
$falhas = $resultados | Where-Object { $_.Status -ne "SUCESSO" }

Write-Host ""
Write-ColorOutput "RESUMO:" $HeaderColor
Write-ColorOutput "[OK] Sucessos: $($sucessos.Count)" $SuccessColor
Write-ColorOutput "[ERRO] Falhas: $($falhas.Count)" $ErrorColor
Write-ColorOutput "[INFO] Total: $($resultados.Count)" $InfoColor

# Detalhar sucessos
if ($sucessos.Count -gt 0) {
    Write-Host ""
    Write-ColorOutput "PROJETOS PUBLICADOS COM SUCESSO:" $SuccessColor
    foreach ($sucesso in $sucessos) {
        $tempo = if ($sucesso.Duration) { " (Tempo: $($sucesso.Duration.ToString('mm\:ss')))" } else { "" }
        Write-ColorOutput "[OK] $($sucesso.Project)$tempo" $SuccessColor
    }
}

# Detalhar falhas
if ($falhas.Count -gt 0) {
    Write-Host ""
    Write-ColorOutput "PROJETOS COM PROBLEMAS:" $ErrorColor
    foreach ($falha in $falhas) {
        Write-ColorOutput "[ERRO] $($falha.Project)" $ErrorColor
        if ($falha.Error) {
            Write-ColorOutput "   Erro: $($falha.Error)" $WarningColor
        }
    }

    Write-Host ""
    Write-ColorOutput "ACAO RECOMENDADA:" $WarningColor
    Write-ColorOutput "- Verificar logs de erro dos projetos com falha" $WarningColor
    Write-ColorOutput "- Executar scripts individuais para debug" $WarningColor
    Write-ColorOutput "- Verificar dependencias e configuracoes" $WarningColor
}

# Informacoes sobre deploy
if ($sucessos.Count -gt 0) {
    Write-Host ""
    Write-ColorOutput "LOCALIZACAO DOS ARQUIVOS PUBLICADOS:" $InfoColor
    Write-ColorOutput "Pasta base: C:\Deploy\CRM\$versao\" $InfoColor
    Write-ColorOutput "Subpastas por projeto:" $InfoColor
    Write-ColorOutput "  - servicos\           (Poliview.crm.servicos)" $InfoColor
    Write-ColorOutput "  - integracao\         (Poliview.crm.integracao)" $InfoColor
    Write-ColorOutput "  - api\                (Poliview.crm.api)" $InfoColor
    Write-ColorOutput "  - email\              (poliview.crm.service.email)" $InfoColor
    Write-ColorOutput "  - sla\                (poliview.crm.sla)" $InfoColor
    Write-ColorOutput "  - monitor\            (Poliview.crm.monitor.service)" $InfoColor
    Write-ColorOutput "  - instalador\         (poliview.crm.instalador)" $InfoColor
}

Write-Header "PUBLICACAO FINALIZADA"

# Codigo de saida baseado em falhas
if ($falhas.Count -gt 0) {
    Write-ColorOutput "[ERRO] Publicacao concluida com $($falhas.Count) erro(s)" $ErrorColor
    exit 1
}
else {
    Write-ColorOutput "[OK] Todos os projetos publicados com sucesso!" $SuccessColor
    exit 0
}