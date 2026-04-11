using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class ClearDataAndRestartCommand : InvokableCommand
{
    private readonly string _packageName;

    public ClearDataAndRestartCommand(string packageName)
    {
        _packageName = packageName;
        Name = "Clear Data & Restart";
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb($"shell pm clear {_packageName}", out _, out string clearError);
            if (!string.IsNullOrEmpty(clearError))
                return ErrorToast($"Failed to clear data: {clearError}");

            var activity = AdbHelper.GetLauncherActivity(_packageName);
            if (activity is null)
                return ErrorToast($"Data cleared, but could not resolve launcher activity for {_packageName}");

            AdbHelper.RunAdb($"shell am start -n {activity}", out _, out string launchError);
            return string.IsNullOrEmpty(launchError)
                ? AdbSettingsManager.Instance.SuccessToast($"Cleared data and restarted {_packageName}")
                : ErrorToast($"Data cleared, but failed to launch: {launchError}");
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
