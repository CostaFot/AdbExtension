// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class ToggleTouchCoordsCommand : InvokableCommand
{
    public ToggleTouchCoordsCommand()
    {
        Name = "Toggle Touch Coordinates";
        Icon = new IconInfo("\uED5F"); // TouchPointer
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb("shell settings get system show_touches", out string current, out string readError);
            if (!string.IsNullOrEmpty(readError))
                return ErrorToast($"Failed to read touch coords state: {readError}");

            var newValue = current.Trim() == "1" ? "0" : "1";
            AdbHelper.RunAdb($"shell settings put system show_touches {newValue}", out _, out string writeError);
            if (!string.IsNullOrEmpty(writeError))
                return ErrorToast($"Failed to set touch coords: {writeError}");

            var state = newValue == "1" ? "enabled" : "disabled";
            return AdbSettingsManager.Instance.SuccessToast($"Touch coordinates {state}");
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
