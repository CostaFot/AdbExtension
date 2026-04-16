#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Generates a self-signed code signing certificate for MSIX sideload releases.

.DESCRIPTION
    Creates a certificate matching the Publisher in Package.appxmanifest,
    exports it as a PFX, and copies the base64-encoded value to the clipboard
    ready to paste as the SIGNING_CERT_PFX GitHub secret.

.PARAMETER Password
    Password to protect the PFX file. Also goes into the SIGNING_CERT_PASSWORD secret.

.PARAMETER OutFile
    Path to write the PFX file. Defaults to signing.pfx next to this script.

.EXAMPLE
    .\create-signing-cert.ps1 -Password "hunter2"
#>
param(
    [Parameter(Mandatory)]
    [string]$Password,

    [string]$OutFile = "$PSScriptRoot\signing.pfx"
)

# Must match Identity/Publisher in Package.appxmanifest
$publisher = "CN=3D57AA92-97A9-42D2-8CB0-4207D9145514"

Write-Host "Creating certificate for: $publisher"

$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject $publisher `
    -KeyUsage DigitalSignature `
    -FriendlyName "ADB Extension Signing" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

$securePassword = ConvertTo-SecureString -String $Password -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $OutFile -Password $securePassword | Out-Null

Write-Host "PFX written to: $OutFile"
Write-Host ""
Write-Host "GitHub secrets to set:"
Write-Host "  SIGNING_CERT_PASSWORD = $Password"
Write-Host ""

$base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($OutFile))
$base64 | Set-Clipboard
Write-Host "  SIGNING_CERT_PFX = (copied to clipboard)"
Write-Host ""
Write-Host "Keep $OutFile safe - you need it to re-sign future releases with the same identity."
