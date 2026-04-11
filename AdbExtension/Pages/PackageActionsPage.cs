using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace AdbExtension;

internal sealed partial class PackageActionsPage : ListPage, INotifyItemsChanged
{
    private readonly string _packageName;
    private readonly Action _refreshPackageList;
    private event TypedEventHandler<object, IItemsChangedEventArgs>? _itemsChanged;

    event TypedEventHandler<object, IItemsChangedEventArgs> INotifyItemsChanged.ItemsChanged
    {
        add { _itemsChanged += value; _itemsChanged?.Invoke(this, new ItemsChangedEventArgs(-1)); }
        remove => _itemsChanged -= value;
    }

    protected new void RaiseItemsChanged(int totalItems = -1)
        => _itemsChanged?.Invoke(this, new ItemsChangedEventArgs(totalItems));

    public PackageActionsPage(string packageName, Action refreshPackageList)
    {
        _packageName = packageName;
        _refreshPackageList = refreshPackageList;
        Title = packageName;
        Name = "Open";
    }

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
            Icon = new IconInfo("\uE768"), // Play
            MoreCommands = [StarItem(ActionIds.Launch)],
        }),
        (ActionIds.RestartApp, new ListItem(new RestartAppCommand(_packageName))
        {
            Title = "Restart",
            Subtitle = "adb shell am force-stop + am start",
            Icon = new IconInfo("\uE72C"), // Refresh
            MoreCommands = [StarItem(ActionIds.RestartApp)],
        }),
        (ActionIds.KillProcess, new ListItem(new KillCommand(_packageName))
        {
            Title = "Kill Process",
            Subtitle = "adb shell am kill / App must not be in the foreground for this to work",
            Icon = new IconInfo("\uE8BB"), // Stop
            MoreCommands = [StarItem(ActionIds.KillProcess)],
        }),
        (ActionIds.ClearAppData, new ListItem(new ClearAppDataCommand(_packageName))
        {
            Title = "Clear App Data",
            Subtitle = "adb shell pm clear",
            Icon = new IconInfo("\uE894"), // Clear
            MoreCommands = [StarItem(ActionIds.ClearAppData)],
        }),
        (ActionIds.ClearDataAndRestart, new ListItem(new ClearDataAndRestartCommand(_packageName))
        {
            Title = "Clear Data & Restart",
            Subtitle = "adb shell pm clear + am start",
            Icon = new IconInfo("\uE72C"), // Refresh
            MoreCommands = [StarItem(ActionIds.ClearDataAndRestart)],
        }),
        (ActionIds.ForceStop, new ListItem(new ForceStopCommand(_packageName))
        {
            Title = "Force Stop",
            Subtitle = "adb shell am force-stop",
            Icon = new IconInfo("\uE71A"), // PowerButton
            MoreCommands = [StarItem(ActionIds.ForceStop)],
        }),
        (ActionIds.OpenDeepLink, new ListItem(new OpenDeepLinkPage(_packageName))
        {
            Title = "Open Deep Link",
            Subtitle = "Enter a deep link that targets this package",
            Icon = new IconInfo("\uE71B"), // Link
            MoreCommands = [StarItem(ActionIds.OpenDeepLink)],
        }),
        (ActionIds.Uninstall, new ListItem(new UninstallAppCommand(_packageName, _refreshPackageList))
        {
            Title = "Uninstall",
            Subtitle = "adb shell pm uninstall",
            Icon = new IconInfo("\uE74D"), // Delete
            MoreCommands = [StarItem(ActionIds.Uninstall)],
        }),
        (ActionIds.GrantPermissions, new ListItem(new GrantAllPermissionsCommand(_packageName))
        {
            Title = "Grant All Permissions",
            Subtitle = "adb shell pm grant <permission>",
            Icon = new IconInfo("\uF78C"),
            MoreCommands = [StarItem(ActionIds.GrantPermissions)],
        }),
        (ActionIds.RevokePermissions, new ListItem(new RevokeAllPermissionsCommand(_packageName))
        {
            Title = "Revoke All Permissions",
            Subtitle = "adb shell pm revoke <permission>",
            Icon = new IconInfo("\uE8D0"), // Blocked
            MoreCommands = [StarItem(ActionIds.RevokePermissions)],
        }),
    ];

    private CommandContextItem StarItem(string id) =>
        new(new ToggleFavoriteCommand(id, () => RaiseItemsChanged(0)))
        {
            Title = FavoritesStore.Instance.IsFavorite(id) ? "Remove from Favorites" : "Add to Favorites",
        };
}
