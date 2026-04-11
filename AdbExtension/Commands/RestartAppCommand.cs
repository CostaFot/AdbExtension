using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class RestartAppCommand : InvokableCommand
{
    private readonly string _packageName;

    public RestartAppCommand(string packageName)
    {
        _packageName = packageName;
        Name = "Restart";
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb($"shell am force-stop {_packageName}", out _, out string stopError);
            if (!string.IsNullOrEmpty(stopError))
                return ErrorToast($"Failed to stop app: {stopError}");

            var activity = AdbHelper.GetLauncherActivity(_packageName);
            if (activity is null)
                return ErrorToast($"App stopped, but could not resolve launcher activity for {_packageName}");

            AdbHelper.RunAdb($"shell am start -n {activity}", out _, out string launchError);
            return string.IsNullOrEmpty(launchError)
                ? AdbSettingsManager.Instance.SuccessToast($"Restarted {_packageName}")
                : ErrorToast($"App stopped, but failed to launch: {launchError}");
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
