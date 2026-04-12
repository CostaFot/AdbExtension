# ADB Extension for Command Palette

A Windows 11 Command Palette extension (PowerToys) for Android developers. Exposes common ADB operations directly from the command palette.

![Top-level commands](top_level_1.png)

## Requirements

- [PowerToys](https://github.com/microsoft/PowerToys) with Command Palette enabled
- [Android Platform Tools](https://developer.android.com/tools/releases/platform-tools) — `adb.exe` must be in your `PATH`
- A connected Android device or running emulator

## Features

### ADB App Commands

Browse all installed packages on the connected device, filtered by status (foreground, running, debuggable). Select a package to act on it:

![Package list](adb_packages_1.png)

![Package actions](adb_packages_commands.png)

| Action | ADB equivalent |
|---|---|
| Launch | `am start -n <launcher activity>` |
| Restart | `am force-stop` + `am start` |
| Kill Process | `am kill` |
| Force Stop | `am force-stop` |
| Clear App Data | `pm clear` |
| Clear Data & Restart | `pm clear` + `am start` |
| Open Deep Link | `am start -a android.intent.action.VIEW -d <url>` |
| Grant All Permissions | `pm grant <permission>` for each declared permission |
| Revoke All Permissions | `pm revoke <permission>` for each declared permission |
| Uninstall | `pm uninstall` |

### ADB APK Manager

Install one or more APKs from a file picker.

![APK Manager](apk_manager.png)

Actions can be starred as favorites and will appear at the top of the list for that package.

### ADB Take Screenshot

Captures the screen and saves it to Pictures (or a custom folder configured in settings).

### ADB Toggle Animations

Enables/disables window, transition, and animator duration scales.

### ADB Toggle Touch Coordinates

Shows/hides touch coordinate overlay.

### ADB Toggle Layout Bounds

Shows/hides layout bounds overlay.

### ADB Toggle Airplane Mode

Toggles airplane mode on/off.

### ADB Enable / Disable Wi-Fi

Turns Wi-Fi on or off.

### ADB Enable / Disable Mobile Data

Turns mobile data on or off.C:\Users\jarla\Downloads\apks

### ADB Launch Deep Link

Fire an arbitrary deep link without targeting a specific package.

## Installation

ADB Extension is available via command palette

![Command Palette search](search_extension_1.png)
![Command Palette results](search_extension_2.png)

## Wishlist

### App targeting
- [ ] Pull a specific shared pref file
- [ ] Dump app's database to desktop

### Device state
- [ ] Set screen timeout
- [ ] Set font size / display size
- [ ] Change locale

### Media / files
- [ ] Pull latest screenshot to clipboard
- [ ] Record screen (start/stop)

### Simulation
- [ ] Send a broadcast intent
- [ ] Simulate low battery / charging state
- [ ] Trigger doze mode
- [ ] Fake a GPS location

## License

[MIT](LICENSE)
