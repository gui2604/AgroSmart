# Executa testes unitarios e grava logs legiveis em logs/
# Uso: powershell -File .\scripts\run-unit-tests.ps1

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

$logDir = Join-Path $RepoRoot "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$fullLog = Join-Path $logDir "unit-test-full.log"
$scenarioLog = Join-Path $logDir "unit-test-scenarios.log"

if (Test-Path $scenarioLog) { Remove-Item $scenarioLog -Force }
$env:ORBITAL_TEST_LOG_DIR = $logDir

Write-Host ""
Write-Host "AgroSmart - testes unitarios (CT-01 .. CT-06)" -ForegroundColor Cyan
Write-Host "Pasta de logs: $logDir" -ForegroundColor Yellow
Write-Host ""

$testOutput = dotnet test AgroSmart.sln --verbosity minimal --nologo 2>&1
$testOutput | Out-File -FilePath $fullLog -Encoding utf8
$testOutput | ForEach-Object { Write-Host $_ }

if (-not (Test-Path $scenarioLog)) {
    Write-Host "Aviso: arquivo de cenarios nao gerado. Rebuild e tente de novo." -ForegroundColor Red
} else {
    Write-Host ""
    Write-Host "----------------------------------------" -ForegroundColor Green
    Write-Host "Cenarios CT-01 .. CT-06:" -ForegroundColor Green
    Write-Host "----------------------------------------" -ForegroundColor Green
    Get-Content $scenarioLog | ForEach-Object { Write-Host $_ }
}
Write-Host ""
Write-Host "Abra no Cursor:" -ForegroundColor Yellow
Write-Host "  logs/unit-test-scenarios.log  (so cenarios)" -ForegroundColor White
Write-Host "  logs/unit-test-full.log       (saida completa)" -ForegroundColor White
Write-Host ""

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
