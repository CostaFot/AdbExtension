# WinGet EXE Distribution — Dead End

## The Core Problem

PowerToys Command Palette discovers extensions exclusively via the Windows `AppExtensionCatalog` API:

```csharp
AppExtensionCatalog.Open("com.microsoft.commandpalette").FindAllAsync();
```

**This API only works with MSIX packages.** It reads the `windows.appExtension` declaration from `Package.appxmanifest`. An EXE installer has no way to register with this catalog — so PowerToys never discovers the extension, regardless of how correct the registry entries are.

There is no alternative discovery mechanism. Verified against the PowerToys source (April 2026):

- No registry-based fallback exists — not even as a TODO
- No file or directory scanning
- No unpackaged extension loading of any kind

The codebase does define an `AppPackagingFlavor` enum with `Unpackaged` and `UnpackagedPortable` values (`Microsoft.CmdPal.Common/AppPackagingFlavor.cs`), and `ApplicationInfoService.cs` has a comment noting portable support is planned. But this enum is **never consulted during extension discovery** — it only appears in diagnostic logging. There is no code path that would ever load an unpackaged extension.

---

## Bugs Found and Fixed Along the Way

These are all correct fixes — they just don't solve the discovery problem.

### 1. Registry `(Default)` value not written
Inno Setup entries were missing `ValueType: string; ValueName: ""`, so the keys were created empty. PowerToys would find the CLSID but not know where to launch the server from.

**Fix:** `setup-template.iss`
```ini
Root: HKCU; Subkey: "...\LocalServer32"; ValueType: string; ValueName: ""; ValueData: "{app}\AdbExtension.exe -RegisterProcessAsComServer"
```

### 2. UAC prompt on install
`DefaultDirName={autopf}` requires elevation. Switched to `{localappdata}` + `PrivilegesRequired=lowest`.

### 3. `PublishTrimmed=true` — unverified fix
`Shmuelie.WinRTServer` is likely not trim-safe, and disabling trimming is a reasonable precaution. However since the extension was never successfully discovered by PowerToys, this was never actually tested. The fix stays in but the root cause of any COM server crash remains unconfirmed.

### 4. Wrong WinGet package ID in workflow
`update-winget.yml` had `CostaFot.AdbExtension` but the actual merged package ID is `CostaFotiadis.ADBExtensionforCommandPalette`.

### 5. WinGet auto-trigger didn't fire
The `update-winget.yml` workflow was only on a feature branch when the v1.0.0.1 release was published. GitHub only reads workflow files from the default branch — the trigger was missed entirely.

---

## What Actually Works

The extension must be distributed as an **MSIX package**. The `Package.appxmanifest` is already set up correctly with the `com.microsoft.commandpalette` app extension declaration. Options:

- **Microsoft Store** — requires Partner Center registration and publisher identity in the manifest
- **Sideloaded MSIX** — build locally, sign with a self-signed cert, install via `Add-AppxPackage`

## Useful References

- PowerToys extension discovery: `PowerToys/src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Models/ExtensionService.cs`
- PowerToys COM activation: `PowerToys/src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Models/ExtensionWrapper.cs`
- WinGet package: `microsoft/winget-pkgs` → `manifests/c/CostaFotiadis/ADBExtensionforCommandPalette/`
