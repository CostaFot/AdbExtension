// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class PackageActionsPage : DynamicListPage
{
    private readonly string _packageName;

    public PackageActionsPage(string packageName)
    {
        _packageName = packageName;
        Title = packageName;
        Name = "Open";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch) { }

    public override IListItem[] GetItems()
    {
        var all = BuildItems();
        var favs = all.Where(x => FavoritesStore.Instance.IsFavorite(x.Id)).Select(x => x.Item).ToArray();
        var rest = all.Where(x => !FavoritesStore.Instance.IsFavorite(x.Id)).Select(x => x.Item).ToArray();

        var result = new List<IListItem>();
        if (favs.Length > 0)
        {
            result.AddRange(new Section("Favorites", favs));
            result.AddRange(new Section("All Actions", rest));
        }
        else
        {
            result.AddRange(rest);
        }
        return result.ToArray();
    }

    private (string Id, IListItem Item)[] BuildItems() => [
        (ActionIds.Launch, new ListItem(new LaunchCommand(_packageName))
        {
            Title = "Launch",
            Subtitle = "adb shell am start -n <launcher activity>",
            MoreCommands = [StarItem(ActionIds.Launch)],
        }),
        (ActionIds.KillProcess, new ListItem(new KillCommand(_packageName))
        {
            Title = "Kill Process",
            Subtitle = "adb shell am kill / App must not be in the foreground for this to work",
            MoreCommands = [StarItem(ActionIds.KillProcess)],
        }),
        (ActionIds.ClearAppData, new ListItem(new ClearAppDataCommand(_packageName))
        {
            Title = "Clear App Data",
            Subtitle = "adb shell pm clear",
            MoreCommands = [StarItem(ActionIds.ClearAppData)],
        }),
        (ActionIds.ForceStop, new ListItem(new ForceStopCommand(_packageName))
        {
            Title = "Force Stop",
            Subtitle = "adb shell am force-stop",
            MoreCommands = [StarItem(ActionIds.ForceStop)],
        }),
        (ActionIds.OpenDeepLink, new ListItem(new OpenDeepLinkPage(_packageName))
        {
            Title = "Open Deep Link",
            Subtitle = "Enter a deep link URL to launch",
            MoreCommands = [StarItem(ActionIds.OpenDeepLink)],
        }),
        (ActionIds.Uninstall, new ListItem(new UninstallAppCommand(_packageName))
        {
            Title = "Uninstall",
            Subtitle = "adb shell pm uninstall",
            MoreCommands = [StarItem(ActionIds.Uninstall)],
        }),
        (ActionIds.GrantPermissions, new ListItem(new GrantAllPermissionsCommand(_packageName))
        {
            Title = "Grant All Permissions",
            Subtitle = "adb shell pm grant <permission>",
            MoreCommands = [StarItem(ActionIds.GrantPermissions)],
        }),
        (ActionIds.RevokePermissions, new ListItem(new RevokeAllPermissionsCommand(_packageName))
        {
            Title = "Revoke All Permissions",
            Subtitle = "adb shell pm revoke <permission>",
            MoreCommands = [StarItem(ActionIds.RevokePermissions)],
        }),
    ];

    private CommandContextItem StarItem(string id) =>
        new(new ToggleFavoriteCommand(id, () => RaiseItemsChanged(0)))
        {
            Title = FavoritesStore.Instance.IsFavorite(id) ? "Remove from Favorites" : "Add to Favorites",
        };
}
