<#
.SYNOPSIS
    Script para compilar o projeto poliview.crm.api no modo Release (self-contained, single-file)
.DESCRIPTION
    Este script compila o projeto .NET 8.0 como single-file self-contained para win-x64 e faz deploy automatico
.EXAMPLE
    .\compilar-api.ps1
#>

param(
    [switch]$Clean = $false,
    [switch]$Verbose = $false
)

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

# Configurar codificacao para caracteres especiais
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Verificar se estamos na pasta correta
$projectPath = ".\poliview.crm.api\poliview.crm.api.csproj"
if (-not (Test-Path $projectPath)) {
    Write-ColorOutput "Projeto nao encontrado: $projectPath" $ErrorColor
    Write-ColorOutput "Certifique-se de executar o script na pasta raiz do projeto" $ErrorColor
    exit 1
}

try {
    Write-ColorOutput "Iniciando compilacao do poliview.crm.api..." $InfoColor
    Write-ColorOutput "Projeto: $projectPath" $InfoColor

    # Localizar dotnet CLI
    $dotnetPath = Find-DotNet

    # Ler versao do arquivo versao.txt
    $versaoFile = ".\versao.txt"
    if (-not (Test-Path $versaoFile)) {
        throw "Arquivo versao.txt nao encontrado"
    }

    $versao = Get-Content $versaoFile -Raw
    $versao = $versao.Trim()
    Write-ColorOutput "Versao detectada: $versao" $InfoColor

    # Definir pasta de output temporario e final
    $tempOutputPath = ".\temp-publish-api"
    $deployPath = "C:\Deploy\CRM\$versao\api"

    Write-ColorOutput "Pasta output temporario: $tempOutputPath" $InfoColor
    Write-ColorOutput "Pasta deploy final: $deployPath" $InfoColor

    # Limpar pasta temporaria se existir
    if (Test-Path $tempOutputPath) {
        Write-ColorOutput "Removendo pasta temporaria existente..." $WarningColor
        Remove-Item -Path $tempOutputPath -Recurse -Force
    }

    # Restaurar dependencias
    Write-ColorOutput "Restaurando dependencias..." $InfoColor
    $restoreArgs = @("restore", $projectPath)
    if ($Verbose) { $restoreArgs += "--verbosity", "detailed" } else { $restoreArgs += "--verbosity", "minimal" }

    & $dotnetPath $restoreArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Falha na restauracao de dependencias"
    }
    Write-ColorOutput "Dependencias restauradas com sucesso" $SuccessColor

    # Limpar se solicitado
    if ($Clean) {
        Write-ColorOutput "Limpando projeto..." $InfoColor
        $cleanArgs = @("clean", $projectPath, "--configuration", "Release")
        if ($Verbose) { $cleanArgs += "--verbosity", "detailed" } else { $cleanArgs += "--verbosity", "minimal" }

        & $dotnetPath $cleanArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Falha na limpeza do projeto"
        }
        Write-ColorOutput "Projeto limpo com sucesso" $SuccessColor
    }

    # Publicar projeto (self-contained, single-file, win-x64)
    Write-ColorOutput "Publicando projeto (self-contained, single-file, win-x64)..." $InfoColor
    $publishArgs = @(
        "publish",
        $projectPath,
        "--configuration", "Release",
        "--runtime", "win-x64",
        "--self-contained", "true",
        "--output", $tempOutputPath,
        "-p:PublishSingleFile=true",
        "-p:PublishTrimmed=false",
        "-p:DebugType=None",
        "-p:DebugSymbols=false"
    )

    if ($Verbose) {
        $publishArgs += "--verbosity", "detailed"
    }
    else {
        $publishArgs += "--verbosity", "minimal"
    }

    & $dotnetPath $publishArgs

    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "Publicacao concluida com sucesso!" $SuccessColor

        # Verificar se o executavel foi gerado
        $exeFiles = Get-ChildItem -Path $tempOutputPath -Filter "*.exe" -File
        if ($exeFiles.Count -gt 0) {
            $mainExe = $exeFiles[0]
            Write-ColorOutput "Executavel gerado: $($mainExe.Name)" $SuccessColor
            Write-ColorOutput "Tamanho: $([math]::Round($mainExe.Length / 1MB, 2)) MB" $InfoColor
            Write-ColorOutput "Data: $($mainExe.LastWriteTime)" $InfoColor

            # Deploy automatico para pasta de deploy
            try {
                Write-ColorOutput "Iniciando deploy automatico..." $InfoColor

                # Criar pasta pai se nao existir
                $parentPath = Split-Path $deployPath -Parent
                if (-not (Test-Path $parentPath)) {
                    New-Item -ItemType Directory -Path $parentPath -Force | Out-Null
                    Write-ColorOutput "Pasta pai criada: $parentPath" $InfoColor
                }

                # Apagar pasta de destino se existir
                if (Test-Path $deployPath) {
                    Write-ColorOutput "Removendo pasta existente: $deployPath" $WarningColor
                    Remove-Item -Path $deployPath -Recurse -Force
                }

                # Copiar arquivos para pasta de deploy
                Write-ColorOutput "Copiando arquivos para deploy..." $InfoColor
                Copy-Item -Path $tempOutputPath -Destination $deployPath -Recurse -Force

                Write-ColorOutput "Deploy concluido com sucesso!" $SuccessColor
                Write-ColorOutput "Arquivos copiados para: $deployPath" $SuccessColor

                # Mostrar arquivos copiados
                $copiedFiles = Get-ChildItem -Path $deployPath -File
                Write-ColorOutput "Arquivos no deploy ($($copiedFiles.Count)):" $InfoColor
                foreach ($file in $copiedFiles) {
                    $size = if ($file.Length -gt 1MB) {
                        "$([math]::Round($file.Length / 1MB, 2)) MB"
                    }
                    else {
                        "$([math]::Round($file.Length / 1KB, 2)) KB"
                    }
                    Write-ColorOutput "  - $($file.Name) ($size)" $InfoColor
                }

                # Limpar pasta temporaria
                Write-ColorOutput "Limpando pasta temporaria..." $InfoColor
                Remove-Item -Path $tempOutputPath -Recurse -Force

            }
            catch {
                Write-ColorOutput "Erro durante o deploy: $($_.Exception.Message)" $ErrorColor
                Write-ColorOutput "Publicacao foi bem-sucedida, mas deploy falhou." $WarningColor
            }
        }
        else {
            throw "Executavel nao encontrado na pasta de output"
        }
    }
    else {
        throw "Falha na publicacao (codigo de saida: $LASTEXITCODE)"
    }

}
catch {
    Write-ColorOutput "Erro durante a compilacao: $($_.Exception.Message)" $ErrorColor

    # Limpar pasta temporaria em caso de erro
    if (Test-Path $tempOutputPath) {
        Remove-Item -Path $tempOutputPath -Recurse -Force -ErrorAction SilentlyContinue
    }

    exit 1
}

Write-ColorOutput "Script finalizado com sucesso!" $SuccessColor