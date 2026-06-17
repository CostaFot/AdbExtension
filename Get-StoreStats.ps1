<#
.SYNOPSIS
    Fetches Microsoft Store acquisition / install stats for the ADB Extension
    via the Microsoft Store analytics API.

.DESCRIPTION
    Obtains a Microsoft Entra (Azure AD) access token using client-credentials,
    then queries the analytics API for the given date range and prints a total
    (or the raw JSON with -Json).

    Credentials come from parameters or, if omitted, these environment variables:
        STORE_TENANT_ID, STORE_CLIENT_ID, STORE_CLIENT_SECRET

.EXAMPLE
    # One-off, passing values inline:
    .\Get-StoreStats.ps1 -TenantId xxx -ClientId yyy -ClientSecret zzz

.EXAMPLE
    # Using env vars, last 90 days, raw JSON:
    $env:STORE_TENANT_ID="xxx"; $env:STORE_CLIENT_ID="yyy"; $env:STORE_CLIENT_SECRET="zzz"
    .\Get-StoreStats.ps1 -StartDate (Get-Date).AddDays(-90) -Json

.EXAMPLE
    # Install events instead of acquisitions:
    .\Get-StoreStats.ps1 -Metric Installs
#>
[CmdletBinding()]
param(
    [string]   $TenantId     = $env:STORE_TENANT_ID,
    [string]   $ClientId     = $env:STORE_CLIENT_ID,
    [string]   $ClientSecret = $env:STORE_CLIENT_SECRET,

    # Store ID for "ADB Extension for Command Palette"
    [string]   $AppId        = "9NHDX4XWCNGS",

    [ValidateSet("Acquisitions", "Installs")]
    [string]   $Metric       = "Acquisitions",

    [datetime] $StartDate    = (Get-Date).AddDays(-30),
    [datetime] $EndDate      = (Get-Date),

    # Print the raw JSON rows instead of just the total
    [switch]   $Json
)

$ErrorActionPreference = "Stop"

foreach ($p in @("TenantId", "ClientId", "ClientSecret")) {
    if ([string]::IsNullOrWhiteSpace((Get-Variable $p -ValueOnly))) {
        throw "Missing $p. Pass -$p or set the matching STORE_* environment variable."
    }
}

# --- 1. Get an access token (valid 60 min) ---------------------------------
$token = (Invoke-RestMethod -Method Post `
    -Uri "https://login.microsoftonline.com/$TenantId/oauth2/token" `
    -ContentType "application/x-www-form-urlencoded" `
    -Body @{
        grant_type    = "client_credentials"
        client_id     = $ClientId
        client_secret = $ClientSecret
        resource      = "https://manage.devcenter.microsoft.com"
    }).access_token

# --- 2. Query the analytics API (follow paging) ----------------------------
$endpoint = if ($Metric -eq "Installs") { "installs" } else { "appacquisitions" }
$start    = $StartDate.ToString("yyyy-MM-dd")
$end      = $EndDate.ToString("yyyy-MM-dd")

$uri = "https://manage.devcenter.microsoft.com/v1.0/my/analytics/$endpoint" +
       "?applicationId=$AppId&startDate=$start&endDate=$end&top=10000"

$headers = @{ Authorization = "Bearer $token" }
$rows = [System.Collections.Generic.List[object]]::new()

while ($uri) {
    $resp = Invoke-RestMethod -Method Get -Uri $uri -Headers $headers
    if ($resp.Value) { $rows.AddRange($resp.Value) }
    $uri = $resp.'@nextLink'
}

# --- 3. Report -------------------------------------------------------------
if ($Json) {
    $rows | ConvertTo-Json -Depth 6
    return
}

$qtyField = if ($Metric -eq "Installs") { "installCount" } else { "acquisitionQuantity" }
$total    = ($rows | Measure-Object -Property $qtyField -Sum).Sum

Write-Host ""
Write-Host "Store $Metric for $AppId" -ForegroundColor Cyan
Write-Host "  Window : $start -> $end"
Write-Host "  Rows   : $($rows.Count)"
Write-Host "  Total  : $([int]$total)" -ForegroundColor Green
Write-Host ""

# Emit the number so it can be captured: $n = .\Get-StoreStats.ps1
[int]$total
