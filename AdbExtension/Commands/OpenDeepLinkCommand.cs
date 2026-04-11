using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class OpenDeepLinkCommand : InvokableCommand
{
    private readonly string _packageName;
    private readonly string _url;

    public OpenDeepLinkCommand(string packageName, string url)
    {
        _packageName = packageName;
        _url = url;
        Name = "Open Deep Link";
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb(
                $"shell am start -p {_packageName} -a android.intent.action.VIEW -d \"{_url}\"",
                out _,
                out string error);

            return string.IsNullOrEmpty(error)
                ? AdbSettingsManager.Instance.SuccessToast($"Opened: {_url}")
                : ErrorToast($"Failed to open deep link: {error}");
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
