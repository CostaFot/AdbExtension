// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class ToggleAirplaneModeCommand : InvokableCommand
{
    public ToggleAirplaneModeCommand()
    {
        Name = "Toggle Airplane Mode";
        Icon = new IconInfo("\uEB4A"); // Airplane
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb("shell settings get global airplane_mode_on", out string current, out string readError);
            if (!string.IsNullOrEmpty(readError))
                return ErrorToast($"Failed to read airplane mode state: {readError}");

            var enable = current.Trim() != "1";
            var state = enable ? "enabled" : "disabled";

            AdbHelper.RunAdb("shell getprop ro.build.version.sdk", out string sdkOutput, out _);
            var isApi30OrHigher = int.TryParse(sdkOutput.Trim(), out int sdk) && sdk >= 30;

            string? writeError;
            if (isApi30OrHigher)
            {
                var stateStr = enable ? "enable" : "disable";
                AdbHelper.RunAdb($"shell cmd connectivity airplane-mode {stateStr}", out _, out writeError);
            }
            else
            {
                var newValue = enable ? "1" : "0";
                var boolStr = enable ? "true" : "false";
                AdbHelper.RunAdb($"shell settings put global airplane_mode_on {newValue}", out _, out writeError);
                if (string.IsNullOrEmpty(writeError))
                    AdbHelper.RunAdb($"shell am broadcast -a android.intent.action.AIRPLANE_MODE --ez state {boolStr}", out _, out writeError);
            }

            if (!string.IsNullOrEmpty(writeError))
                return ErrorToast($"Failed to set airplane mode: {writeError}");
            return CommandResult.ShowToast(new ToastArgs { Message = $"Airplane mode {state}", Result = CommandResult.KeepOpen() });
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
