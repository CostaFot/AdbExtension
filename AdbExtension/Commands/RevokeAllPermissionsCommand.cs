using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class RevokeAllPermissionsCommand : InvokableCommand
{
    private readonly string _packageName;

    public RevokeAllPermissionsCommand(string packageName)
    {
        _packageName = packageName;
        Name = "Revoke All Permissions";
    }

    public override ICommandResult Invoke()
    {
        try
        {
            AdbHelper.RunAdb($"shell dumpsys package {_packageName}", out string output, out string error);
            if (!string.IsNullOrEmpty(error))
                return ErrorToast($"Failed to read package info: {error}");

            var permissions = ParseGrantedRuntimePermissions(output);
            if (permissions.Count == 0)
                return AdbSettingsManager.Instance.SuccessToast("No granted runtime permissions found");

            var revoked = 0;
            foreach (var permission in permissions)
            {
                AdbHelper.RunAdb($"shell pm revoke {_packageName} {permission}", out _, out string revokeError);
                if (string.IsNullOrEmpty(revokeError))
                    revoked++;
            }

            return AdbSettingsManager.Instance.SuccessToast($"Revoked {revoked}/{permissions.Count} permissions");
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

    // Parses the "runtime permissions:" section and returns only those with granted=true.
    private static List<string> ParseGrantedRuntimePermissions(string dumpsysOutput)
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
                    if (line.Contains("granted=true", StringComparison.Ordinal))
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
