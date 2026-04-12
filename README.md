# ADB Extension for Command Palette

A Windows 11 Command Palette extension (PowerToys) for Android developers. Exposes common ADB operations directly from the command palette.

## Requirements

- [PowerToys](https://github.com/microsoft/PowerToys) with Command Palette enabled
- [Android Platform Tools](https://developer.android.com/tools/releases/platform-tools) — `adb.exe` must be in your `PATH`
- A connected Android device or running emulator

## Features

### ADB App Commands

Browse all installed packages on the connected device, filtered by status (foreground, running, debuggable). Select a package to act on it:

![ADB App Commands](docs/app_commands.gif)

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

Actions can be starred as favorites and will appear at the top of the list for that package.

### ADB Take Screenshot

Captures the screen and saves it to Pictures (or a custom folder configured in settings).

![Take Screenshot](docs/take_screenshot.gif)

### ADB Toggle Animations

Enables/disables window, transition, and animator duration scales.

![Toggle Animations](docs/toggle_animations.gif)

### ADB Toggle Touch Coordinates

Shows/hides touch coordinate overlay.

![Toggle Touch Coordinates](docs/toggle_touch_coords.gif)

### ADB Toggle Layout Bounds

Shows/hides layout bounds overlay.

![Toggle Layout Bounds](docs/toggle_layout_bounds.gif)

### ADB Toggle Airplane Mode

Toggles airplane mode on/off.

![Toggle Airplane Mode](docs/toggle_airplane_mode.gif)

### ADB Enable / Disable Wi-Fi

Turns Wi-Fi on or off.

![Toggle Wi-Fi](docs/toggle_wifi.gif)

### ADB Enable / Disable Mobile Data

Turns mobile data on or off.

![Toggle Mobile Data](docs/toggle_mobile_data.gif)

### ADB APK Manager

Install one or more APKs from a file picker.

![APK Manager](docs/apk_manager.gif)

### ADB Launch Deep Link

Fire an arbitrary deep link without targeting a specific package.

![Launch Deep Link](docs/launch_deep_link.gif)

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
