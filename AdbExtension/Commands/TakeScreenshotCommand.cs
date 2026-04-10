// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class TakeScreenshotCommand : InvokableCommand
{
    private const string DeviceTempPath = "/sdcard/cmdpal_screenshot.png";

    public TakeScreenshotCommand()
    {
        Name = "Take Screenshot";
        Icon = new IconInfo("https://github.com/favicon.ico");
    }

    public override ICommandResult Invoke()
    {
        try
        {
            RunAdb($"shell screencap -p {DeviceTempPath}", out string captureError);
            if (!string.IsNullOrEmpty(captureError))
                return ErrorToast($"Failed to capture screenshot: {captureError}");

            string localPath = BuildLocalPath();
            RunAdb($"pull {DeviceTempPath} \"{localPath}\"", out string pullError);
            if (!string.IsNullOrEmpty(pullError))
                return ErrorToast($"Failed to pull screenshot: {pullError}");

            // Cleanup is best-effort; don't fail the command if it errors
            try { RunAdb($"shell rm {DeviceTempPath}", out _); } catch { }

            return CommandResult.ShowToast($"Screenshot saved: {localPath}");
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception w32 && w32.NativeErrorCode == 2)
        {
            return ErrorToast("ADB not found. Make sure Android Platform Tools are installed and adb.exe is in your PATH.");
        }
        catch (Exception ex)
        {
            return ErrorToast($"Unexpected error: {ex.Message}");
        }
    }

    // Runs adb, reading both stdout and stderr before WaitForExit to prevent deadlocks.
    private static void RunAdb(string arguments, out string stderrOutput)
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

        stderrOutput = hasError
            ? (string.IsNullOrWhiteSpace(stderr) ? $"adb exited with code {process.ExitCode}" : stderr.Trim())
            : string.Empty;
    }

    private static string BuildLocalPath()
    {
        string pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        return Path.Combine(pictures, $"screenshot_{timestamp}.png");
    }

    private static ICommandResult ErrorToast(string message)
    {
        return CommandResult.ShowToast(new ToastArgs
        {
            Message = message,
            Result = CommandResult.KeepOpen(),
        });
    }
}
