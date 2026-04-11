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
            new CommandItem(new TakeScreenshotCommand()) { Title = "Take Screenshot" },
            new CommandItem(new ToggleAnimationsCommand()) { Title = "Toggle Animations" },
            new CommandItem(new ToggleTouchCoordsCommand()) { Title = "Toggle Touch Coordinates" },
            new CommandItem(new ToggleAirplaneModeCommand()) { Title = "Toggle Airplane Mode" },
            new CommandItem(new EnableWifiCommand()) { Title = "Enable Wi-Fi" },
            new CommandItem(new DisableWifiCommand()) { Title = "Disable Wi-Fi" },
            new CommandItem(new EnableMobileDataCommand()) { Title = "Enable Mobile Data" },
            new CommandItem(new DisableMobileDataCommand()) { Title = "Disable Mobile Data" },
            new CommandItem(new ToggleLayoutBoundsCommand()) { Title = "Toggle Layout Bounds" },
            new CommandItem(new InstallApksPage()) { Title = "APK Manager" },
            new CommandItem(new LaunchDeepLinkPage()) { Title = "Launch Deep Link" },
            new CommandItem(new AdbExtensionPage()) { Title = "ADB App Commands" },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
