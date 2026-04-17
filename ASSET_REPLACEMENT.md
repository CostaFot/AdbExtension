# Asset Replacement Plan — Copyright Issue

## Context
All current assets depict the Android robot head mascot (dome-shaped head with two antennae, circular eyes), which is Google's trademarked Android logo. Microsoft Store flagged this. Every image file in the project uses this same shape and must be replaced before resubmission.

---

## What needs replacing

### 1. In-app extension icons (2 files used, 1 unused)
These appear in the Command Palette as the extension's icon:

| File | Used? | Where |
|---|---|---|
| `Assets/droid_dark_1.png` | Yes | `AdbExtensionCommandsProvider.cs:13`, `AdbExtensionPage.cs:27` |
| `droid_light_2.png` | Yes | same two files (light theme pair) |
| `droid_light.png` | No | Can just delete |

### 2. Windows app tile & store assets (all contain the mascot — ~57 files)
These are generated from a single source icon at multiple sizes/scales. You only need to design **one new icon**, then regenerate all variants.

| Asset type | Base file | Purpose |
|---|---|---|
| **StoreLogo** | `StoreLogo.png` | Microsoft Store listing thumbnail — most critical |
| **Square44x44Logo** | `Square44x44Logo.png` | App icon (taskbar, start menu, alt-tab) |
| **Square150x150Logo** | `Square150x150Logo.png` | Medium tile |
| **SmallTile** | `SmallTile.png` | Small tile |
| **LargeTile** | `LargeTile.png` | Large tile |
| **Wide310x150Logo** | `Wide310x150Logo.png` | Wide tile |
| **SplashScreen** | `SplashScreen.png` | Splash on launch |
| **LockScreenLogo** | `LockScreenLogo.scale-200.png` | Lock screen badge |

Each base file has scale variants (100/125/150/200/400) and the Square44x44Logo also has target-size and altform-unplated variants.

---

## Recommended approach

**Design one new icon** — something clearly ADB/Android-debugging-themed but not the Android mascot. Good directions:
- A USB plug + terminal/shell symbol
- A phone outline with a command prompt `>_` inside
- A stylized "ADB" wordmark

**Tooling to regenerate all size variants** from one source:
- [**Windows App Icon Studio**](https://www.microsoft.com/store/productId/9NBLGGH4TXVP) — official Microsoft tool, takes one PNG and outputs all required variants
- Or use Photoshop/Figma to manually export at required sizes

**Code changes needed:** Once you have new files, update 2 lines:
- `AdbExtensionCommandsProvider.cs:13`
- `AdbExtensionPage.cs:27`

---

## Verification
1. Build the MSIX and install it
2. Confirm the new icon appears in Command Palette, Start Menu, and taskbar
3. Resubmit to Microsoft Store — no new asset files beyond what's listed above
