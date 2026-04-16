# Publishing Plan — ADB Extension for Command Palette

## Context

PowerToys Command Palette discovers extensions via the Windows `AppExtensionCatalog` API, which **only works with MSIX packages**. The EXE/WinGet approach does not work — see `winget-dead-end.md` for the full post-mortem.

The `Package.appxmanifest` is already correctly configured with the `com.microsoft.commandpalette` app extension declaration. The path forward is MSIX only.

---

## Step 0 — Bump version (every release)

Update the version number in all of these files:

| File | Field |
|---|---|
| `AdbExtension/AdbExtension.csproj` | `<AppxPackageVersion>` |
| `AdbExtension/Package.appxmanifest` | `Identity Version=` |
| `AdbExtension/app.manifest` | `assemblyIdentity version=` |
| `AdbExtension/setup-template.iss` | `#define AppVersion` |
| `AdbExtension/build-exe.ps1` | `[string]$Version` default parameter |

Quick one-liner to update all at once (replace `OLD` and `NEW`):
```powershell
$files = @("AdbExtension/AdbExtension.csproj","AdbExtension/Package.appxmanifest","AdbExtension/app.manifest","AdbExtension/setup-template.iss","AdbExtension/build-exe.ps1")
$files | ForEach-Object { (Get-Content $_) -replace 'OLD','NEW' | Set-Content $_ }
```

---

## Step 1 — Manifest fixes ✅ DONE

**`AdbExtension/Package.appxmanifest`** — updated:
- Version: `1.0.0.2`
- Display name: `ADB Extension for Command Palette`
- Description: `Run ADB commands for connected Android devices directly from Command Palette.`

> **Note:** `Identity/Name`, `Identity/Publisher`, and `Properties/PublisherDisplayName` are still scaffold defaults — fill in after Partner Center registration (Step 2).

---

## Step 2 — Replace placeholder assets

All files in `AdbExtension/Assets/` are generic Microsoft template images. Replace with custom branding.

The build target in `AdbExtension.csproj` auto-copies scale-specific files to the base filenames the Store expects — you only need to provide the `scale-200` variants and `StoreLogo.png`.

| File to create | Size | Notes |
|---|---|---|
| `Assets\Square44x44Logo.scale-200.png` | 44×44 | Also copied to `SmallTile.png` (71×71 — use same file) |
| `Assets\Square150x150Logo.scale-200.png` | 150×150 | Also copied to `LargeTile.png` (310×310 — use same file) |
| `Assets\Wide310x150Logo.scale-200.png` | 310×150 | |
| `Assets\SplashScreen.scale-200.png` | 620×300 | |
| `Assets\StoreLogo.png` | 50×50 | |

> Use Visual Studio's asset generation tool to produce all variants from a single source image: right-click `Package.appxmanifest` → Visual Assets.

---

## Step 3 — Partner Center registration & publisher identity

1. Register in **Microsoft Partner Center** → Apps and games → New Product → MSIX or PWA app.
2. Reserve product name: `ADB Extension for Command Palette`.
3. From **Product Management → Product identity**, copy and update `Package.appxmanifest`:

```xml
<Identity
  Name="<from Partner Center>"
  Publisher="<from Partner Center>"
  Version="1.0.0.2" />
<Properties>
  <PublisherDisplayName><from Partner Center></PublisherDisplayName>
  ...
</Properties>
```

4. Add signing config to `AdbExtension.csproj` first `<PropertyGroup>`:

```xml
<AppxPackageSigningEnabled>true</AppxPackageSigningEnabled>
<PackageCertificateThumbprint></PackageCertificateThumbprint>
<AppxBundle>Always</AppxBundle>
<AppxBundlePlatforms>x64|ARM64</AppxBundlePlatforms>
```

---

## Step 4 — Build MSIX bundle via GitHub Actions

Create `.github/workflows/release-msix.yml` to replace the existing EXE release workflow. It should:

1. Trigger on `workflow_dispatch` with version and release notes inputs (same pattern as `release-extension.yml`)
2. Build x64 and ARM64 with `dotnet build -p:GenerateAppxPackageOnBuild=true`
3. Bundle both into a `.msixbundle` via `makeappx`
4. Sign with `signtool` using a PFX certificate stored as a GitHub secret (`SIGNING_CERT_PFX` base64 + `SIGNING_CERT_PASSWORD`)
5. Create a GitHub Release and attach the `.msixbundle`
6. Update `update-winget.yml` to point at the `.msixbundle` URL — the current workflow constructs two separate x64/ARM64 EXE URLs which won't apply; a bundle is a single file covering both architectures

**Required GitHub secrets to add before running:**
- `SIGNING_CERT_PFX` — base64-encoded PFX certificate
- `SIGNING_CERT_PASSWORD` — certificate password

> The existing `release-extension.yml` (EXE/Inno Setup) can be kept but is no longer the primary release path.

---

## Step 5 — Distribution options

| Option | Effort | Notes |
|---|---|---|
| Microsoft Store | Medium | Best for discoverability. Requires Partner Center + signing cert from Step 3. |
| GitHub Releases (MSIX) | Low | Attach `.msixbundle` to GitHub Release. Users install via double-click or `Add-AppxPackage`. Requires self-signed cert or trusted CA cert. |

---

## Verification

- Install MSIX and reload Command Palette — extension should appear in the list
- `winget show CostaFotiadis.ADBExtensionforCommandPalette` to confirm Store listing (post-Store submission)
