using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

public partial class AdbExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public AdbExtensionCommandsProvider()
    {
        DisplayName = "ADB Quick Commands";
        Icon = new IconInfo("\uE8EA"); // Phone
        Settings = AdbSettingsManager.Instance.Settings;

        _commands = [
            new CommandItem(new AdbExtensionPage()) { Title = "ADB App Commands" },
            new CommandItem(new TakeScreenshotCommand()) { Title = "ADB Take Screenshot" },
            new CommandItem(new ToggleAnimationsCommand()) { Title = "ADB Toggle Animations" },
            new CommandItem(new ToggleTouchCoordsCommand()) { Title = "ADB Toggle Touch Coordinates" },
            new CommandItem(new ToggleAirplaneModeCommand()) { Title = "ADB Toggle Airplane Mode" },
            new CommandItem(new EnableWifiCommand()) { Title = "ADB Enable Wi-Fi" },
            new CommandItem(new DisableWifiCommand()) { Title = "ADB Disable Wi-Fi" },
            new CommandItem(new EnableMobileDataCommand()) { Title = "ADB Enable Mobile Data" },
            new CommandItem(new DisableMobileDataCommand()) { Title = "ADB Disable Mobile Data" },
            new CommandItem(new ToggleLayoutBoundsCommand()) { Title = "ADB Toggle Layout Bounds" },
            new CommandItem(new InstallApksPage()) { Title = "ADB APK Manager" },
            new CommandItem(new LaunchDeepLinkPage()) { Title = "ADB Launch Deep Link" },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
