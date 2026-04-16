# Publishing Plan — ADB Extension for Command Palette

## Context

PowerToys Command Palette discovers extensions via the Windows `AppExtensionCatalog` API, which **only works with MSIX packages**. The EXE/WinGet approach does not work — see `winget-dead-end.md` for the full post-mortem.

The `Package.appxmanifest` is already correctly configured with the `com.microsoft.commandpalette` app extension declaration. The path forward is MSIX only.

---

## Step 1 — Manifest fixes ✅ DONE

**`AdbExtension/Package.appxmanifest`** — updated:
- Version: `1.0.0.2`
- Display name: `ADB Extension for Command Palette`
- Description: `Run ADB commands for connected Android devices directly from Command Palette.`

> **Note:** `Identity/Name`, `Identity/Publisher`, and `Properties/PublisherDisplayName` are still scaffold defaults — fill in after Partner Center registration (Step 2).

---

## Step 2 — Replace placeholder assets

All 7 files in `AdbExtension/Assets/` are generic Microsoft template images. Replace with custom branding:
- `StoreLogo.png`
- `Square44x44Logo.scale-200.png`
- `Square44x44Logo.targetsize-24_altform-unplated.png`
- `Square150x150Logo.scale-200.png`
- `Wide310x150Logo.scale-200.png`
- `SplashScreen.scale-200.png`
- `LockScreenLogo.scale-200.png`

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

## Step 4 — Build MSIX bundle

```
cd AdbExtension\AdbExtension
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64 -p:AppxPackageDir="AppPackages\x64\"
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=ARM64 -p:AppxPackageDir="AppPackages\ARM64\"
makeappx bundle /v /d bin\Release\ /p AdbExtensionForCommandPalette_1.0.0.2_Bundle.msixbundle
```

Upload `.msixbundle` to Partner Center. Note in the Store description that reviewers need PowerToys + Command Palette installed to test it.

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
