// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class GrantAllPermissionsCommand : InvokableCommand
{
    private readonly string _packageName;

    public GrantAllPermissionsCommand(string packageName)
    {
        _packageName = packageName;
        Name = "Grant All Permissions";
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb($"shell dumpsys package {_packageName}", out string output, out string error);
            if (!string.IsNullOrEmpty(error))
                return ErrorToast($"Failed to read package info: {error}");

            var permissions = ParseRuntimePermissions(output);
            if (permissions.Count == 0)
                return AdbSettingsManager.Instance.SuccessToast("No runtime permissions found");

            var granted = 0;
            foreach (var permission in permissions)
            {
                AdbHelper.RunAdb($"shell pm grant {_packageName} {permission}", out _, out string grantError);
                if (string.IsNullOrEmpty(grantError))
                    granted++;
            }

            return AdbSettingsManager.Instance.SuccessToast($"Granted {granted}/{permissions.Count} permissions");
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

    // Parses the "runtime permissions:" section from "dumpsys package <pkg>" output.
    private static List<string> ParseRuntimePermissions(string dumpsysOutput)
    {
        var result = new List<string>();
        var inRuntimeSection = false;

        foreach (var raw in dumpsysOutput.Split('\n'))
        {
            var line = raw.Trim();

            if (line.StartsWith("runtime permissions:", StringComparison.Ordinal))
            {
                inRuntimeSection = true;
                continue;
            }

            if (inRuntimeSection)
            {
                // Each permission line looks like: "android.permission.CAMERA: granted=false, flags=[]"
                var colonIdx = line.IndexOf(':');
                if (colonIdx > 0 && line.StartsWith("android.permission.", StringComparison.Ordinal))
                {
                    result.Add(line[..colonIdx]);
                }
                else if (!string.IsNullOrEmpty(line) && !line.StartsWith("android.", StringComparison.Ordinal))
                {
                    // Left the runtime permissions section
                    break;
                }
            }
        }

        return result;
    }

    private static ICommandResult ErrorToast(string message) =>
        CommandResult.ShowToast(new ToastArgs { Message = message, Result = CommandResult.KeepOpen() });
}
