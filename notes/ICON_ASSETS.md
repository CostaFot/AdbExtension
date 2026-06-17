# Icon Assets Reference

Start from a **1024×1024** source (SVG or high-res PNG) and export down.

## Square44x44Logo — App icon (taskbar, Start, shell)

| File | Size |
|---|---|
| `Square44x44Logo.scale-100.png` | 44×44 |
| `Square44x44Logo.scale-125.png` | 55×55 |
| `Square44x44Logo.scale-150.png` | 66×66 |
| `Square44x44Logo.scale-200.png` | 88×88 |
| `Square44x44Logo.scale-400.png` | 176×176 |
| `Square44x44Logo.targetsize-16.png` | 16×16 |
| `Square44x44Logo.targetsize-24.png` | 24×24 |
| `Square44x44Logo.targetsize-32.png` | 32×32 |
| `Square44x44Logo.targetsize-48.png` | 48×48 |
| `Square44x44Logo.targetsize-256.png` | 256×256 |
| `Square44x44Logo.targetsize-16_altform-unplated.png` | 16×16 |
| `Square44x44Logo.targetsize-24_altform-unplated.png` | 24×24 |
| `Square44x44Logo.targetsize-32_altform-unplated.png` | 32×32 |
| `Square44x44Logo.targetsize-48_altform-unplated.png` | 48×48 |
| `Square44x44Logo.targetsize-256_altform-unplated.png` | 256×256 |

`altform-unplated` = same artwork, transparent background, no system plate applied.

## Square150x150Logo — Medium tile

| File | Size |
|---|---|
| `Square150x150Logo.scale-100.png` | 150×150 |
| `Square150x150Logo.scale-125.png` | 188×188 |
| `Square150x150Logo.scale-150.png` | 225×225 |
| `Square150x150Logo.scale-200.png` | 300×300 |
| `Square150x150Logo.scale-400.png` | 600×600 |

## Wide310x150Logo — Wide tile

| File | Size |
|---|---|
| `Wide310x150Logo.scale-100.png` | 310×150 |
| `Wide310x150Logo.scale-125.png` | 388×188 |
| `Wide310x150Logo.scale-150.png` | 465×225 |
| `Wide310x150Logo.scale-200.png` | 620×300 |
| `Wide310x150Logo.scale-400.png` | 1240×600 |

## StoreLogo — Store listing icon

| File | Size |
|---|---|
| `StoreLogo.png` | 50×50 |
| `StoreLogo.scale-200.png` | 100×100 |

## SplashScreen — Launch screen

| File | Size |
|---|---|
| `SplashScreen.scale-100.png` | 620×300 |
| `SplashScreen.scale-125.png` | 775×375 |
| `SplashScreen.scale-150.png` | 930×450 |
| `SplashScreen.scale-200.png` | 1240×600 |
| `SplashScreen.scale-400.png` | 2480×1200 |

## LockScreenLogo

| File | Size |
|---|---|
| `LockScreenLogo.scale-200.png` | 48×48 |

## Priority for Store submission

These are the ones that matter most:

1. `StoreLogo.png` (50×50) — shown in Store search results
2. `Square44x44Logo.scale-200.png` (88×88) — shell icon
3. `Square44x44Logo.targetsize-256.png` (256×256) — high-res shell icon
4. `Square150x150Logo.scale-200.png` (300×300) — tile

> Store promotional screenshots are submitted separately and are not part of the package assets.

## Command Palette icon (in-app)

The icon shown inside Command Palette itself is set in code, not from these asset files:

- `AdbExtensionCommandsProvider.cs` — top-level palette entry
- `Pages/AdbExtensionPage.cs` — main page

Use `new IconInfo("https://...")` for a URL or `IconHelpers.FromRelativePath("Assets\\foo.png")` for a bundled asset. Target **24×24px** baseline, **48×48px** for HiDPI.
