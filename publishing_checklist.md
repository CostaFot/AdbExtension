# Publishing Prep Plan — ADB Extension for Command Palette

## Context

The extension was scaffolded from Microsoft's template, which leaves several placeholder/default values in the manifest and csproj. Before submitting to Partner Center or WinGet, these need to be cleaned up. Some changes can be made immediately; others require first registering in Partner Center to get your Publisher identity.

---

## Track A: Microsoft Store (MSIX bundle)

### ~~Step 1 — Fixes you can do RIGHT NOW~~ ✅ DONE

**`AdbExtension/Package.appxmanifest`** — all updated:
- Version: `1.0.0.2`
- All display names: `ADB Extension for Command Palette`
- All descriptions: `Run ADB commands for connected Android devices directly from Command Palette.`

> **Note:** `Identity/Name`, `Identity/Publisher`, and `Properties/PublisherDisplayName` are left as scaffold defaults — fill in after Partner Center registration (Step 2).

**Assets (`AdbExtension/Assets/`)** — all 7 files are generic Microsoft template images. Replace with custom branding. Required files:
- `StoreLogo.png`
- `Square44x44Logo.scale-200.png`
- `Square44x44Logo.targetsize-24_altform-unplated.png`
- `Square150x150Logo.scale-200.png`
- `Wide310x150Logo.scale-200.png`
- `SplashScreen.scale-200.png`
- `LockScreenLogo.scale-200.png`

---

### Step 2 — After Partner Center registration

1. Register in **Microsoft Partner Center** → Apps and games → New Product → MSIX or PWA app.
2. Reserve product name (e.g. `ADB Extension for Command Palette`).
3. From **Product Management → Product identity**, copy:
   - `Package/Identity/Name`
   - `Package/Identity/Publisher`
   - `Package/Properties/PublisherDisplayName`

**Update `Package.appxmanifest`** with copied values:
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

**Update `AdbExtension.csproj`** — add to the first unconditional `<PropertyGroup>`:
```xml
<AppxPackageSigningEnabled>true</AppxPackageSigningEnabled>
<PackageCertificateThumbprint></PackageCertificateThumbprint>
<AppxBundle>Always</AppxBundle>
<AppxBundlePlatforms>x64|ARM64</AppxBundlePlatforms>
```

---

### Step 3 — Build MSIX bundle

```
cd AdbExtension\AdbExtension
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64 -p:AppxPackageDir="AppPackages\x64\"
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=ARM64 -p:AppxPackageDir="AppPackages\ARM64\"
makeappx bundle /v /d bin\Release\ /p AdbExtensionForCommandPalette_1.0.0.2_Bundle.msixbundle
```

Upload `.msixbundle` to Partner Center. In the Store description, note that reviewer needs PowerToys + Command Palette installed.

---

## Track B: WinGet (EXE installer via Inno Setup + GitHub Actions)

### Step 4 — Modify csproj for WinGet publishing

In `AdbExtension/AdbExtension.csproj`, in the first `<PropertyGroup>`:
- Remove: `<PublishProfile>win-$(Platform).pubxml</PublishProfile>`
- Add: `<WindowsPackageType>None</WindowsPackageType>`

### Step 5 — Create build scripts

Create in `AdbExtension/AdbExtension/`:
- `setup-template.iss` — Inno Setup installer config (use template from MS docs, fill in CLSID `d857a76b-60ad-4db5-a14c-22f1d4f7bfaa` and app metadata)
- `build-exe.ps1` — PowerShell build script (setup .NET + Inno Setup → dotnet publish → create installer → upload)

### Step 6 — Create GitHub Actions workflow

Create `.github/workflows/release-extension.yml` — automates: build → package → create GitHub Release with x64 + ARM64 `.exe` assets.

### Step 7 — WinGet first submission

After the GitHub Release is created:
```
wingetcreate new "<x64 .exe URL>" "<arm64 .exe URL>"
```
Follow prompts, add tag `windows-commandpalette-extension` to locale YAML files before submitting.

---

## Critical files to modify

| File | Changes |
|---|---|
| `AdbExtension/Package.appxmanifest` | DisplayName, Description, Version, Publisher identity (post-Partner Center) |
| `AdbExtension/AdbExtension.csproj` | AppxPackage properties (post-Partner Center), WindowsPackageType swap for WinGet |
| `AdbExtension/Assets/*.png` | Replace all 7 with custom branding |
| `AdbExtension/AdbExtension/setup-template.iss` | Create new |
| `AdbExtension/AdbExtension/build-exe.ps1` | Create new |
| `.github/workflows/release-extension.yml` | Create new |

## Verification

- After Step 1: Deploy the MSIX and reload Command Palette — confirm display names are updated throughout
- After Step 3: `dir *.msixbundle` confirms bundle was created; upload to Partner Center submission
- After Step 6: Trigger `gh workflow run release-extension.yml` and confirm both `.exe` assets appear in the GitHub Release
