#!/usr/bin/env powershell

<#
.SYNOPSIS
    Script para compilar o projeto Poliview.crm.servicos no modo Release
.DESCRIPTION
    Este script localiza o MSBuild, restaura pacotes NuGet e compila o projeto no modo Release
.EXAMPLE
    .\compilar-servicos.ps1
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

function Find-MSBuild {
    Write-ColorOutput "Procurando MSBuild..." $InfoColor

    # Tentar localizar MSBuild em diferentes locais
    $msbuildPaths = @(
        # Visual Studio 2022
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",

        # Visual Studio 2019
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",

        # Visual Studio 2017
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe",

        # MSBuild standalone
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe"
    )

    foreach ($path in $msbuildPaths) {
        if (Test-Path $path) {
            Write-ColorOutput "MSBuild encontrado: $path" $SuccessColor
            return $path
        }
    }

    # Tentar usando where.exe
    try {
        $whereMSBuild = where.exe msbuild 2>$null
        if ($whereMSBuild -and (Test-Path $whereMSBuild[0])) {
            Write-ColorOutput "MSBuild encontrado via PATH: $($whereMSBuild[0])" $SuccessColor
            return $whereMSBuild[0]
        }
    }
    catch {
        # Ignorar erro do where.exe
    }

    throw "MSBuild nao encontrado. Instale o Visual Studio ou Build Tools for Visual Studio."
}

function Find-NuGet {
    Write-ColorOutput "Procurando NuGet..." $InfoColor

    # Tentar localizar nuget.exe
    $nugetPaths = @(
        ".\nuget.exe",
        "${env:ProgramFiles(x86)}\NuGet\nuget.exe",
        "${env:LocalAppData}\NuGet\nuget.exe"
    )

    foreach ($path in $nugetPaths) {
        if (Test-Path $path) {
            Write-ColorOutput "NuGet encontrado: $path" $SuccessColor
            return $path
        }
    }

    # Tentar baixar nuget.exe se nao encontrar
    try {
        Write-ColorOutput "Baixando NuGet.exe..." $InfoColor
        $nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
        $nugetPath = ".\nuget.exe"
        Invoke-WebRequest -Uri $nugetUrl -OutFile $nugetPath
        Write-ColorOutput "NuGet baixado com sucesso" $SuccessColor
        return $nugetPath
    }
    catch {
        Write-ColorOutput "Nao foi possivel baixar o NuGet.exe: $($_.Exception.Message)" $WarningColor
        return $null
    }
}

# Configurar codificacao para caracteres especiais
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Verificar se estamos na pasta correta
$projectPath = ".\Poliview.crm.servicos\Poliview.crm.servicos.csproj"
if (-not (Test-Path $projectPath)) {
    Write-ColorOutput "Projeto nao encontrado: $projectPath" $ErrorColor
    Write-ColorOutput "Certifique-se de executar o script na pasta raiz do projeto" $ErrorColor
    exit 1
}

try {
    Write-ColorOutput "Iniciando compilacao do Poliview.crm.servicos..." $InfoColor
    Write-ColorOutput "Projeto: $projectPath" $InfoColor

    # Localizar MSBuild
    $msbuildPath = Find-MSBuild

    # Localizar NuGet
    $nugetPath = Find-NuGet

    # Restaurar pacotes NuGet se disponível
    if ($nugetPath) {
        Write-ColorOutput "Restaurando pacotes NuGet..." $InfoColor

        # Verificar se existe arquivo de solucao
        $solutionPath = ".\Poliview.crm.sln"
        $packagesPath = ".\packages"

        if (Test-Path $solutionPath) {
            # Usar arquivo de solucao para restaurar pacotes
            $nugetArgs = @("restore", $solutionPath, "-PackagesDirectory", $packagesPath)
        }
        else {
            # Usar projeto diretamente especificando pasta de pacotes
            $nugetArgs = @("restore", $projectPath, "-PackagesDirectory", $packagesPath)
        }

        if ($Verbose) { $nugetArgs += "-Verbosity", "detailed" }

        & $nugetPath $nugetArgs
        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "Falha na restauracao de pacotes NuGet, mas continuando..." $WarningColor
        }
        else {
            Write-ColorOutput "Pacotes NuGet restaurados com sucesso" $SuccessColor
        }
    }

    # Limpar se solicitado
    if ($Clean) {
        Write-ColorOutput "Limpando projeto..." $InfoColor
        $cleanArgs = @(
            $projectPath,
            "/p:Configuration=Release",
            "/p:Platform=AnyCPU",
            "/target:Clean",
            "/nologo"
        )
        if ($Verbose) { $cleanArgs += "/verbosity:detailed" }

        & $msbuildPath $cleanArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Falha na limpeza do projeto"
        }
        Write-ColorOutput "Projeto limpo com sucesso" $SuccessColor
    }

    # Compilar projeto
    Write-ColorOutput "Compilando projeto no modo Release..." $InfoColor
    $buildArgs = @(
        $projectPath,
        "/p:Configuration=Release",
        "/p:Platform=AnyCPU",
        "/p:OutputPath=bin\Release\",
        "/target:Build",
        "/nologo",
        "/maxcpucount"
    )

    if ($Verbose) {
        $buildArgs += "/verbosity:detailed"
    }
    else {
        $buildArgs += "/verbosity:minimal"
    }

    & $msbuildPath $buildArgs

    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "Compilacao concluida com sucesso!" $SuccessColor

        # Verificar se o executavel foi gerado
        $outputPath = ".\Poliview.crm.servicos\bin\Release\Poliview.crm.servicos.exe"
        if (Test-Path $outputPath) {
            Write-ColorOutput "Executavel gerado: $outputPath" $SuccessColor

            # Mostrar informacoes do arquivo
            $fileInfo = Get-Item $outputPath
            Write-ColorOutput "Tamanho: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" $InfoColor
            Write-ColorOutput "Data: $($fileInfo.LastWriteTime)" $InfoColor

            # Deploy automatico para pasta de deploy
            try {
                Write-ColorOutput "Iniciando deploy automatico..." $InfoColor

                # Ler versao do arquivo versao.txt
                $versaoFile = ".\versao.txt"
                if (Test-Path $versaoFile) {
                    $versao = Get-Content $versaoFile -Raw
                    $versao = $versao.Trim()
                    Write-ColorOutput "Versao detectada: $versao" $InfoColor

                    # Definir pastas de origem e destino
                    $sourcePath = ".\Poliview.crm.servicos\bin\Release"
                    $deployPath = "C:\Deploy\CRM\$versao\servicos"

                    Write-ColorOutput "Pasta origem: $sourcePath" $InfoColor
                    Write-ColorOutput "Pasta destino: $deployPath" $InfoColor

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
                    Copy-Item -Path $sourcePath -Destination $deployPath -Recurse -Force

                    Write-ColorOutput "Deploy concluido com sucesso!" $SuccessColor
                    Write-ColorOutput "Arquivos copiados para: $deployPath" $SuccessColor

                    # Mostrar arquivos copiados
                    $copiedFiles = Get-ChildItem -Path $deployPath -File
                    Write-ColorOutput "Arquivos no deploy ($($copiedFiles.Count)):" $InfoColor
                    foreach ($file in $copiedFiles) {
                        Write-ColorOutput "  - $($file.Name) ($([math]::Round($file.Length / 1KB, 2)) KB)" $InfoColor
                    }

                }
                else {
                    Write-ColorOutput "Arquivo versao.txt nao encontrado. Deploy ignorado." $WarningColor
                }

            }
            catch {
                Write-ColorOutput "Erro durante o deploy: $($_.Exception.Message)" $ErrorColor
                Write-ColorOutput "Compilacao foi bem-sucedida, mas deploy falhou." $WarningColor
            }
        }
    }
    else {
        throw "Falha na compilacao (codigo de saida: $LASTEXITCODE)"
    }

}
catch {
    Write-ColorOutput "Erro durante a compilacao: $($_.Exception.Message)" $ErrorColor
    exit 1
}

Write-ColorOutput "Script finalizado com sucesso!" $SuccessColor