<#
.SYNOPSIS
    Script para publicar o projeto Poliview.crm.monitor.service
.DESCRIPTION
    Compila o projeto .NET 8.0 como single-file self-contained para win-x64
.EXAMPLE
    .\publicar.ps1
#>

# Configurar codificacao para caracteres especiais
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Cores para output
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Find-DotNet {
    Write-ColorOutput "Procurando dotnet CLI..." $InfoColor

    # Verificar se dotnet esta no PATH
    try {
        $dotnetVersion = dotnet --version 2>$null
        if ($dotnetVersion) {
            Write-ColorOutput "dotnet CLI encontrado - versao: $dotnetVersion" $SuccessColor
            return "dotnet"
        }
    }
    catch {
        # Ignorar erro
    }

    # Tentar localizar dotnet em diferentes locais
    $dotnetPaths = @(
        "${env:ProgramFiles}\dotnet\dotnet.exe",
        "${env:ProgramFiles(x86)}\dotnet\dotnet.exe",
        "${env:LocalAppData}\Microsoft\dotnet\dotnet.exe"
    )

    foreach ($path in $dotnetPaths) {
        if (Test-Path $path) {
            Write-ColorOutput "dotnet CLI encontrado: $path" $SuccessColor
            return $path
        }
    }

    throw "dotnet CLI nao encontrado. Instale o .NET SDK 8.0 ou superior."
}

try {
    Write-ColorOutput "Iniciando publicacao do Poliview.crm.monitor.service..." $InfoColor

    # Localizar dotnet CLI
    $dotnetPath = Find-DotNet

    # Verificar se o arquivo de projeto existe
    $projectPath = ".\Poliview.crm.monitor.service.csproj"
    if (-not (Test-Path $projectPath)) {
        throw "Projeto nao encontrado: $projectPath"
    }

    Write-ColorOutput "Projeto: $projectPath" $InfoColor

    # Ler versao do arquivo versao.txt (na pasta pai)
    $versaoFile = "..\versao.txt"
    if (-not (Test-Path $versaoFile)) {
        throw "Arquivo versao.txt nao encontrado em: $versaoFile"
    }

    $versao = Get-Content $versaoFile -Raw
    $versao = $versao.Trim()
    Write-ColorOutput "Versao detectada: $versao" $InfoColor

    # Definir pasta de destino
    $publishDir = "C:/Deploy/CRM/$versao/monitor"
    Write-ColorOutput "Pasta de publicacao: $publishDir" $InfoColor

    # Criar pasta pai se nao existir
    $parentPath = Split-Path $publishDir -Parent
    if (-not (Test-Path $parentPath)) {
        New-Item -ItemType Directory -Path $parentPath -Force | Out-Null
        Write-ColorOutput "Pasta pai criada: $parentPath" $InfoColor
    }

    # Remover pasta de destino se existir
    if (Test-Path $publishDir) {
        Write-ColorOutput "Removendo pasta existente: $publishDir" $WarningColor
        Remove-Item -Path $publishDir -Recurse -Force
    }

    # Executar dotnet publish
    Write-ColorOutput "Executando dotnet publish..." $InfoColor
    $publishArgs = @(
        "publish",
        $projectPath,
        "-c", "Release",
        "-r", "win-x64",
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:PublishDir=$publishDir/"
    )

    Write-ColorOutput "Comando: dotnet $($publishArgs -join ' ')" $InfoColor

    & $dotnetPath $publishArgs

    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "Publicacao concluida com sucesso!" $SuccessColor

        # Verificar arquivos publicados
        if (Test-Path $publishDir) {
            $publishedFiles = Get-ChildItem -Path $publishDir -File
            Write-ColorOutput "Arquivos publicados ($($publishedFiles.Count)):" $InfoColor

            foreach ($file in $publishedFiles) {
                $size = if ($file.Length -gt 1MB) {
                    "$([math]::Round($file.Length / 1MB, 2)) MB"
                }
                else {
                    "$([math]::Round($file.Length / 1KB, 2)) KB"
                }
                Write-ColorOutput "  - $($file.Name) ($size)" $InfoColor
            }

            Write-ColorOutput "Publicacao finalizada com sucesso!" $SuccessColor
            Write-ColorOutput "Arquivos disponiveis em: $publishDir" $SuccessColor
        }
        else {
            Write-ColorOutput "Pasta de publicacao nao encontrada!" $ErrorColor
        }
    }
    else {
        throw "Falha na publicacao (codigo de saida: $LASTEXITCODE)"
    }

}
catch {
    Write-ColorOutput "Erro durante a publicacao: $($_.Exception.Message)" $ErrorColor
    exit 1
}

Write-ColorOutput "Script finalizado com sucesso!" $SuccessColor