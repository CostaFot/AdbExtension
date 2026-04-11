using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class EnableWifiCommand : InvokableCommand
{
    public EnableWifiCommand()
    {
        Name = "Enable Wi-Fi";
        Icon = new IconInfo("\uE701"); // Wifi
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb("shell svc wifi enable", out _, out string error);
            return string.IsNullOrEmpty(error)
                ? AdbSettingsManager.Instance.SuccessToast("Wi-Fi enabled")
                : ErrorToast($"Failed to enable Wi-Fi: {error}");
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
