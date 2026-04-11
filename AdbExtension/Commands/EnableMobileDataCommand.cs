using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class EnableMobileDataCommand : InvokableCommand
{
    public EnableMobileDataCommand()
    {
        Name = "Enable Mobile Data";
        Icon = new IconInfo("\uEC3B"); // NetworkTower
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb("shell svc data enable", out _, out string error);
            return string.IsNullOrEmpty(error)
                ? AdbSettingsManager.Instance.SuccessToast("Mobile data enabled")
                : ErrorToast($"Failed to enable mobile data: {error}");
        }
        catch (Exception ex) when (ex is Win32Exception w && w.NativeErrorCode == 2)
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
