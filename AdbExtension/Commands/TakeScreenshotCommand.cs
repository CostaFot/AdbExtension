// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
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
            AdbHelper.RunAdb($"shell screencap -p {DeviceTempPath}", out _, out string captureError);
            if (!string.IsNullOrEmpty(captureError))
                return ErrorToast($"Failed to capture screenshot: {captureError}");

            string localPath = BuildLocalPath();
            AdbHelper.RunAdb($"pull {DeviceTempPath} \"{localPath}\"", out _, out string pullError);
            if (!string.IsNullOrEmpty(pullError))
                return ErrorToast($"Failed to pull screenshot: {pullError}");

            // Cleanup is best-effort; don't fail the command if it errors
            try { AdbHelper.RunAdb($"shell rm {DeviceTempPath}", out _, out _); } catch { }

            return CommandResult.ShowToast(new ToastArgs { Message = $"Screenshot saved: {localPath}", Result = CommandResult.KeepOpen() });
        }
        catch (Exception ex) when (ex is Win32Exception w32 && w32.NativeErrorCode == 2)
        {
            return ErrorToast("ADB not found. Make sure Android Platform Tools are installed and adb.exe is in your PATH.");
        }
        catch (Exception ex)
        {
            return ErrorToast($"Unexpected error: {ex.Message}");
        }
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
