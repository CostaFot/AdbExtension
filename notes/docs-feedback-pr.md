# Docs Feedback: publish-extension-winget.md is misleading — EXE/MSI installers cannot be discovered by Command Palette

**Doc URL:** https://learn.microsoft.com/en-us/windows/powertoys/command-palette/publish-extension-winget

---

## Summary

The guide instructs extension developers to build EXE installers (via Inno Setup) and publish them to WinGet. This approach does not work — extensions installed via EXE or MSI are never discovered by Command Palette, regardless of how correctly the installer is configured.

## Root Cause

Command Palette discovers extensions exclusively via the Windows `AppExtensionCatalog` API:

```csharp
AppExtensionCatalog.Open("com.microsoft.commandpalette").FindAllAsync();
```

This API only resolves extensions registered as **MSIX packages**. It reads the `windows.appExtension` declaration from `Package.appxmanifest`. An EXE or MSI installer has no way to register with this catalog.

The relevant discovery logic is in:
- `PowerToys/src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Models/ExtensionService.cs`

There is no alternative discovery mechanism. Verified against the PowerToys source at `PowerToys/src/modules/cmdpal` (April 2026):

- No registry-based fallback exists anywhere in the codebase
- No file or directory scanning for unpackaged extensions
- No planned fallback code — not even a TODO

The codebase defines an `AppPackagingFlavor` enum (`Unpackaged`, `UnpackagedPortable`) in `Microsoft.CmdPal.Common/AppPackagingFlavor.cs`, but this is **never consulted during extension discovery** — it only appears in diagnostic logging. There is no code path that loads an unpackaged extension.

## Impact

Developers following this guide will:
1. Build a working EXE installer
2. Publish it to WinGet successfully
3. Have users install it — and then find the extension never appears in Command Palette

This is a silent failure with no error message. The extension installs correctly but is invisible to Command Palette.

## What Actually Works

Extensions must be distributed as **MSIX packages**. The correct distribution paths are:

- **Microsoft Store** — covered in the separate [publish-extension-store.md](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/publish-extension-store) guide
- **Sideloaded MSIX** — build and sign locally, install via `Add-AppxPackage`

## Suggested Fix

Either:
1. **Remove or deprecate this guide** until the unpackaged registry fallback is implemented in PowerToys
2. **Add a prominent warning** at the top stating that EXE/MSI distribution does not enable Command Palette discovery, and that MSIX is required

If the registry fallback is shipped in a future PowerToys release, this guide should be updated to reference the minimum PowerToys version required.
