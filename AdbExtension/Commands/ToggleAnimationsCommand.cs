// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class ToggleAnimationsCommand : InvokableCommand
{
    public ToggleAnimationsCommand()
    {
        Name = "Toggle Animations";
        Icon = new IconInfo("\uE8EB"); // Transition
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb("shell settings get global window_animation_scale", out string current, out string readError);
            if (!string.IsNullOrEmpty(readError))
                return ErrorToast($"Failed to read animation state: {readError}");

            var isEnabled = current.Trim() != "0";
            var newValue = isEnabled ? "0" : "1";

            AdbHelper.RunAdb($"shell settings put global window_animation_scale {newValue}", out _, out string e1);
            AdbHelper.RunAdb($"shell settings put global transition_animation_scale {newValue}", out _, out string e2);
            AdbHelper.RunAdb($"shell settings put global animator_duration_scale {newValue}", out _, out string e3);

            var error = new[] { e1, e2, e3 }.FirstOrDefault(e => !string.IsNullOrEmpty(e));
            if (error is not null)
                return ErrorToast($"Failed to set animations: {error}");

            var state = isEnabled ? "disabled" : "enabled";
            return AdbSettingsManager.Instance.SuccessToast($"Animations {state}");
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
