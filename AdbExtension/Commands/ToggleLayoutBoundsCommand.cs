// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class ToggleLayoutBoundsCommand : InvokableCommand
{
    public ToggleLayoutBoundsCommand()
    {
        Name = "Toggle Layout Bounds";
        Icon = new IconInfo("\uE773"); // GridView/Layout
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb("shell getprop debug.layout", out string current, out string readError);
            if (!string.IsNullOrEmpty(readError))
                return ErrorToast($"Failed to read layout bounds state: {readError}");

            var enable = current.Trim() != "true";
            var newValue = enable ? "true" : "false";
            AdbHelper.RunAdb($"shell setprop debug.layout {newValue}", out _, out string writeError);
            if (!string.IsNullOrEmpty(writeError))
                return ErrorToast($"Failed to set layout bounds: {writeError}");

            AdbHelper.RunAdb("shell service call activity 1599295570", out _, out string notifyError);
            if (!string.IsNullOrEmpty(notifyError))
                return ErrorToast($"Failed to notify system: {notifyError}");

            var state = enable ? "enabled" : "disabled";
            return AdbSettingsManager.Instance.SuccessToast($"Layout bounds {state}");
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
