// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

public partial class AdbExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public AdbExtensionCommandsProvider()
    {
        DisplayName = "ADB Quick Commands";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands = [
            new CommandItem(new TakeScreenshotCommand()) { Title = "Take Screenshot" },
            new CommandItem(new LockScreenCommand()) { Title = "Lock Screen" },
            new CommandItem(new ToggleAnimationsCommand()) { Title = "Toggle Animations" },
            new CommandItem(new ToggleTouchCoordsCommand()) { Title = "Toggle Touch Coordinates" },
            new CommandItem(new AdbExtensionPage()) { Title = "ADB App Commands" },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
