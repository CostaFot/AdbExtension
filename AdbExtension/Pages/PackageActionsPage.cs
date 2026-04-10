// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class PackageActionsPage : ListPage
{
    private readonly string _packageName;

    public PackageActionsPage(string packageName)
    {
        _packageName = packageName;
        Title = packageName;
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new LaunchCommand(_packageName))
            {
                Title = "Launch",
                Subtitle = "adb shell am start -n <launcher activity>",
            },
            new ListItem(new KillCommand(_packageName))
            {
                Title = "Kill Process",
                Subtitle = $"adb shell am kill {_packageName}",
            },
            new ListItem(new ClearAppDataCommand(_packageName))
            {
                Title = "Clear App Data",
                Subtitle = $"adb shell pm clear {_packageName}",
            },
            new ListItem(new ForceStopCommand(_packageName))
            {
                Title = "Force Stop",
                Subtitle = $"adb shell am force-stop {_packageName}",
            },
            new ListItem(new OpenDeepLinkPage(_packageName))
            {
                Title = "Open Deep Link",
                Subtitle = "Enter a deep link URL to launch",
            },
            new ListItem(new UninstallAppCommand(_packageName))
            {
                Title = "Uninstall",
                Subtitle = $"adb shell pm uninstall {_packageName}",
            },
        ];
    }
}
