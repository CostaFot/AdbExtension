// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdbExtension;

internal record PackageInfo(string Name, bool IsDebuggable);

internal static class AdbHelper
{
    // Runs adb, reading both stdout and stderr before WaitForExit to prevent deadlocks.
    public static void RunAdb(string arguments, out string stdoutOutput, out string stderrOutput)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "adb",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        process.Start();
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        bool hasError = process.ExitCode != 0
            || stderr.Contains("error:", StringComparison.OrdinalIgnoreCase);

        stdoutOutput = stdout;
        stderrOutput = hasError
            ? (string.IsNullOrWhiteSpace(stderr) ? $"adb exited with code {process.ExitCode}" : stderr.Trim())
            : string.Empty;
    }

    // Returns 3rd-party installed packages, debuggable ones sorted first then alphabetically.
    // Returns empty array on any error (no device, ADB not found, etc.)
    public static PackageInfo[] GetInstalledPackages()
    {
        try
        {
            RunAdb("shell pm list packages -3", out string pmOutput, out string pmError);
            if (!string.IsNullOrEmpty(pmError))
                return [];

            var thirdParty = pmOutput
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.StartsWith("package:", StringComparison.Ordinal))
                .Select(line => line["package:".Length..])
                .ToHashSet(StringComparer.Ordinal);

            if (thirdParty.Count == 0)
                return [];

            RunAdb("shell dumpsys package packages", out string dumpsysOutput, out _);
            var debuggable = ParseDebuggablePackages(dumpsysOutput);

            return thirdParty
                .Select(pkg => new PackageInfo(pkg, debuggable.Contains(pkg)))
                .OrderByDescending(p => p.IsDebuggable)
                .ThenBy(p => p.Name)
                .ToArray();
        }
        catch
        {
            return [];
        }
    }

    // Parses "dumpsys package packages" output and returns the set of debuggable package names.
    // Handles both "flags=" (older Android) and "pkgFlags=" (newer Android) field names.
    private static HashSet<string> ParseDebuggablePackages(string dumpsysOutput)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        string? current = null;

        foreach (var raw in dumpsysOutput.Split('\n'))
        {
            var line = raw.Trim();

            if (line.StartsWith("Package [", StringComparison.Ordinal))
            {
                var start = line.IndexOf('[') + 1;
                var end = line.IndexOf(']', start);
                if (start > 0 && end > start)
                    current = line[start..end];
            }
            else if (current != null
                && (line.StartsWith("flags=", StringComparison.Ordinal)
                    || line.StartsWith("pkgFlags=", StringComparison.Ordinal))
                && line.Contains("DEBUGGABLE", StringComparison.Ordinal))
            {
                result.Add(current);
            }
        }

        return result;
    }
}
