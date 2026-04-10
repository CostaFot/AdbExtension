// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed class AdbSettingsManager : JsonSettingsManager
{
    public static readonly AdbSettingsManager Instance = new();

    private readonly ToggleSetting _keepOpen = new("keepOpen", true)
    {
        Label = "Keep palette open after running a command",
        Description = "When off, the palette dismisses after each command.",
    };

    public bool KeepOpen => _keepOpen.Value;

    public ICommandResult SuccessToast(string message) =>
        KeepOpen
            ? CommandResult.ShowToast(new ToastArgs { Message = message, Result = CommandResult.KeepOpen() })
            : CommandResult.ShowToast(message);

    private AdbSettingsManager()
    {
        FilePath = System.IO.Path.Combine(Utilities.BaseSettingsPath("Microsoft.CmdPal"), "adb.settings.json");
        Settings.Add(_keepOpen);
        LoadSettings();
        Settings.SettingsChanged += (s, a) => SaveSettings();
    }
}
