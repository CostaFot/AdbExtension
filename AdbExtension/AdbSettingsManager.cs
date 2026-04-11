using System;
using System.IO;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed class AdbSettingsManager : JsonSettingsManager
{
    public static readonly AdbSettingsManager Instance = new();

    private readonly ToggleSetting _keepOpen = new("keepOpen", true)
    {
        Label = "Keep palette open after running a command",
        Description = "When off, the palette dismisses after each command.",
    };

    private readonly TextSetting _screenshotFolder = new("screenshotFolder", string.Empty)
    {
        Label = "Screenshot save folder",
        Description = "Where screenshots are saved. Leave empty to use My Pictures.",
        Placeholder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
    };

    private static readonly string DefaultApkFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    private readonly TextSetting _apkFolder = new("apkFolder", DefaultApkFolder)
    {
        Label = "APK Manager folder",
        Description = "Default folder the APK Manager scans for APK files.",
        Placeholder = DefaultApkFolder,
    };

    private readonly ToggleSetting _skipUninstallConfirmation = new("skipUninstallConfirmation", false)
    {
        Label = "Skip uninstall confirmation",
        Description = "When on, uninstall runs immediately without a confirmation dialog.",
    };

    public bool KeepOpen => _keepOpen.Value;
    public string ScreenshotFolder => _screenshotFolder.Value ?? string.Empty;
    public string ApkFolder => _apkFolder.Value ?? string.Empty;
    public bool SkipUninstallConfirmation => _skipUninstallConfirmation.Value;

    public ICommandResult SuccessToast(string message) =>
        KeepOpen
            ? CommandResult.ShowToast(new ToastArgs { Message = message, Result = CommandResult.KeepOpen() })
            : CommandResult.ShowToast(message);

    private AdbSettingsManager()
    {
        FilePath = System.IO.Path.Combine(Utilities.BaseSettingsPath("Microsoft.CmdPal"), "adb.settings.json");
        Settings.Add(_keepOpen);
        Settings.Add(_screenshotFolder);
        Settings.Add(_apkFolder);
        Settings.Add(_skipUninstallConfirmation);
        LoadSettings();
        Settings.SettingsChanged += (s, a) => SaveSettings();
    }
}
