using System;
using System.ComponentModel;
using System.IO;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class InstallApkCommand : InvokableCommand
{
    private readonly string _apkPath;

    public InstallApkCommand(string apkPath)
    {
        _apkPath = apkPath;
        Name = "Install";
        Icon = new IconInfo("\uE896"); // Download
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb($"install -r \"{_apkPath}\"", out _, out string error);
            if (!string.IsNullOrEmpty(error))
                return ErrorToast($"Install failed: {error}");

            return AdbSettingsManager.Instance.SuccessToast($"Installed: {Path.GetFileName(_apkPath)}");
        }
        catch (Exception ex) when (ex is Win32Exception w32 && w32.NativeErrorCode == 2)
        {
            return ErrorToast("ADB not found. Make sure adb.exe is in your PATH.");
        }
        catch (Exception ex)
        {
            return ErrorToast($"Unexpected error: {ex.Message}");
        }
    }

    private static ICommandResult ErrorToast(string message) =>
        CommandResult.ShowToast(new ToastArgs { Message = message, Result = CommandResult.KeepOpen() });
}
