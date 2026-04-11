// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class KillCommand : InvokableCommand
{
    private readonly string _packageName;

    public KillCommand(string packageName)
    {
        _packageName = packageName;
        Name = "Kill Process";
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb($"shell am kill {_packageName}", out _, out string error);
            return string.IsNullOrEmpty(error)
                ? AdbSettingsManager.Instance.SuccessToast($"Killed process: {_packageName}")
                : ErrorToast($"Failed to kill process: {error}");
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
