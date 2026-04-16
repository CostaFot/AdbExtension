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

Quick one-liner to update all at once (replace `OLD` and `NEW`):
```powershell
$files = @("AdbExtension/AdbExtension.csproj","AdbExtension/Package.appxmanifest","AdbExtension/app.manifest")
$files | ForEach-Object { (Get-Content $_) -replace 'OLD','NEW' | Set-Content $_ }
```

---

## Step 1 — Manifest fixes ✅ DONE

**`AdbExtension/Package.appxmanifest`** — updated:
- Identity `Name`, `Publisher`, `PublisherDisplayName` filled in from Partner Center
- Version: `1.0.0.3`
- Display name: `ADB Extension for Command Palette`
- Description: `Run ADB commands for connected Android devices directly from Command Palette.`

---

## Step 2 — Replace placeholder assets ✅ DONE

All scale variants generated via Visual Studio's Visual Assets tool and committed.

---

## Step 3 — Partner Center registration & publisher identity ✅ DONE

Identity values copied from Partner Center and applied to both `Package.appxmanifest` and `AdbExtension.csproj`.

---

## Step 4 — Build MSIX bundle via GitHub Actions ✅ DONE

`.github/workflows/release-msix.yml` created. It:

1. Triggers on `workflow_dispatch` with version and release notes inputs
2. Builds x64 and ARM64 with `dotnet build -p:GenerateAppxPackageOnBuild=true`
3. Bundles both into a `.msixbundle` via `makeappx bundle /f bundle_mapping.txt`
4. Signs with `signtool` using the self-signed PFX from GitHub secrets
5. Creates a GitHub Release and attaches the `.msixbundle`

**GitHub secrets set:**
- `SIGNING_CERT_PFX` ✅
- `SIGNING_CERT_PASSWORD` ✅

**Signing cert:** `AdbExtension/signing.pfx` — gitignored, back it up outside the repo.

> The existing `release-extension.yml` (EXE/Inno Setup) can be kept but is no longer the primary release path.

---

## Step 5 — Distribution options

| Option | Effort | Notes |
|---|---|---|
| Microsoft Store | Medium | Best for discoverability. Upload `.msixbundle` to Partner Center — Store re-signs automatically. |
| GitHub Releases (MSIX) | Low | Attach `.msixbundle` to GitHub Release. Users install via double-click or `Add-AppxPackage`. Signed with self-signed cert. |

---

## Verification

- Install MSIX and reload Command Palette — extension should appear in the list
- `winget show CostaFotiadis.ADBExtensionforCommandPalette` to confirm Store listing (post-Store submission)
