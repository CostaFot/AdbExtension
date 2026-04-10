// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class LaunchCommand : InvokableCommand
{
    private readonly string _packageName;

    public LaunchCommand(string packageName)
    {
        _packageName = packageName;
        Name = "Launch";
    }

    public override ICommandResult Invoke()
    {
        try
        {
            var activity = AdbHelper.GetLauncherActivity(_packageName);
            if (activity is null)
                return ErrorToast($"Could not resolve launcher activity for {_packageName}");

            AdbHelper.RunAdb($"shell am start -n {activity}", out _, out string error);
            return string.IsNullOrEmpty(error)
                ? AdbSettingsManager.Instance.SuccessToast($"Launched {_packageName}")
                : ErrorToast($"Failed to launch: {error}");
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
