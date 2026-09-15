# ==============================================================================
# 🎮 GameLog — Script de Provisionamento e Deploy Automatizado na Microsoft Azure
# ==============================================================================
param (
    [string]$ResourceGroupName = "rg-gamelog-prod",
    [string]$Location = "eastus2",
    [string]$DbAdminUser = "gamelogadmin",
    [string]$DbPassword,
    [string]$JwtSecret
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "🚀 Iniciando Implantação do GameLog na Microsoft Azure" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Checar se o Azure CLI está logado
try {
    $currentAccount = az account show --query "name" -o tsv
    Write-Host "[1/7] Conectado à conta Azure: $currentAccount" -ForegroundColor Green
} catch {
    Write-Error "Você não está logado na Azure CLI. Execute 'az login' antes de rodar este script."
    exit 1
}

# 2. Validar senhas
if (-not $DbPassword) {
    $DbPassword = Read-Host -Prompt "Digite a senha para o banco PostgreSQL (mínimo 8 caracteres com letras e números)" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($DbPassword)
    $DbPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

if (-not $JwtSecret) {
    $randomBytes = New-Object byte[] 32
    (New-Object System.Security.Cryptography.RNGCryptoServiceProvider).GetBytes($randomBytes)
    $JwtSecret = [Convert]::ToBase64String($randomBytes)
    Write-Host "[2/7] JWT_SECRET seguro de 256 bits gerado automaticamente." -ForegroundColor Yellow
}

$suffix = Get-Random -Minimum 1000 -Maximum 9999
$AcrName = "acrgamelog$suffix"
$PgServerName = "pg-gamelog-$suffix"

# 3. Criar Resource Group
Write-Host "[3/7] Criando Grupo de Recursos '$ResourceGroupName' em '$Location'..." -ForegroundColor Cyan
az group create --name $ResourceGroupName --location $Location | Out-Null

# 4. Criar PostgreSQL Flexible Server
Write-Host "[4/7] Criando Banco PostgreSQL Flexible Server ($PgServerName)..." -ForegroundColor Cyan
az postgres flexible-server create `
    --resource-group $ResourceGroupName `
    --name $PgServerName `
    --location $Location `
    --admin-user $DbAdminUser `
    --admin-password $DbPassword `
    --sku-name Standard_B1ms `
    --tier Burstable `
    --storage-size 32 `
    --version 16 `
    --database-name gamelog `
    --yes | Out-Null

az postgres flexible-server firewall-rule create `
    --resource-group $ResourceGroupName `
    --name $PgServerName `
    --rule-name AllowAllAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0 | Out-Null

# 5. Criar ACR e Build da Imagem
Write-Host "[5/7] Criando Container Registry ($AcrName) e construindo imagem da API..." -ForegroundColor Cyan
az acr create --resource-group $ResourceGroupName --name $AcrName --sku Basic --admin-enabled true | Out-Null
$AcrPassword = az acr credential show --name $AcrName --query "passwords[0].value" -o tsv
$AcrServer = az acr show --name $AcrName --query "loginServer" -o tsv

az acr build --registry $AcrName --image gamelog-backend:latest ./backend | Out-Null

# 6. Criar Azure Container App
Write-Host "[6/7] Criando Container App Environment e API Backend..." -ForegroundColor Cyan
az containerapp env create --name "env-gamelog" --resource-group $ResourceGroupName --location $Location | Out-Null

az containerapp create `
    --name "api-gamelog" `
    --resource-group $ResourceGroupName `
    --environment "env-gamelog" `
    --image "$AcrServer/gamelog-backend:latest" `
    --target-port 8080 `
    --ingress external `
    --registry-server $AcrServer `
    --registry-username $AcrName `
    --registry-password $AcrPassword `
    --cpu 0.5 `
    --memory 1.0Gi `
    --min-replicas 1 `
    --max-replicas 5 `
    --secrets "db-pass=$DbPassword" "jwt-secret=$JwtSecret" `
    --env-vars `
        "ASPNETCORE_ENVIRONMENT=Production" `
        "DB_SERVER=$PgServerName.postgres.database.azure.com" `
        "DB_PORT=5432" `
        "DB_NAME=gamelog" `
        "DB_USER=$DbAdminUser" `
        "DB_PASSWORD=secretref:db-pass" `
        "JWT_SECRET=secretref:jwt-secret" `
        "FORCE_RESEED=false" `
        "CORS_ALLOWED_ORIGINS=http://localhost:3000" | Out-Null

$ApiUrl = az containerapp show --name "api-gamelog" --resource-group $ResourceGroupName --query "properties.configuration.ingress.fqdn" -o tsv

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "🎉 Implantação Concluída com Sucesso!" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "API Backend: https://$ApiUrl" -ForegroundColor Yellow
Write-Host "Health Check: https://$ApiUrl/health" -ForegroundColor Yellow
Write-Host "Metrics OTel: https://$ApiUrl/metrics" -ForegroundColor Yellow
Write-Host "Banco Host:  $PgServerName.postgres.database.azure.com" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Green
