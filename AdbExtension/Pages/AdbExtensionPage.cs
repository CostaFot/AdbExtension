// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class AdbExtensionPage : DynamicListPage
{
    private PackageInfo[]? _packages;

    public AdbExtensionPage()
    {
        Icon = new IconInfo("\uE8EA"); // Phone
        Title = "ADB App Commands";
        Name = "Open";
        PlaceholderText = "Search packages...";
        IsLoading = true;
        Task.Run(LoadPackages);
    }

    private void LoadPackages()
    {
        Log.Info("LoadPackages: start");
        _packages = AdbHelper.GetInstalledPackages();
        Log.Info($"LoadPackages: done, {_packages.Length} packages");
        IsLoading = false;
        RaiseItemsChanged(0);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
        => RaiseItemsChanged(0);

    public override IListItem[] GetItems()
    {
        if (_packages is null)
            return [];

        var items = new List<IListItem>();

        var source = string.IsNullOrEmpty(SearchText)
            ? _packages
            : _packages.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToArray();

        if (source.Length == 0)
        {
            items.Add(new ListItem(new NoOpCommand()) { Title = "No packages found 😔" });
        }
        else
        {
            items.AddRange(source.Select(pkg =>
            {
                var tags = new List<string>();
                if (pkg.IsForeground) tags.Add("foreground");
                else if (pkg.IsRunning) tags.Add("running");
                if (pkg.IsDebuggable) tags.Add("debuggable");

                var section = pkg.IsForeground ? "Foreground"
                    : pkg.IsRunning ? "Running"
                    : pkg.IsDebuggable ? "Debuggable"
                    : "Other";

                return (IListItem)new ListItem(new PackageActionsPage(pkg.Name, RefreshPackages))
                {
                    Title = pkg.Name,
                    Subtitle = tags.Count > 0 ? string.Join(" · ", tags) : null,
                    Section = section,
                };
            }));
        }

        items.Add(new ListItem(new RefreshPackagesCommand(this)) { Title = "Refresh 🔄️" });

        return items.ToArray();
    }

    internal void RefreshPackages()
    {
        Log.Info("RefreshPackages: triggered");
        _packages = null;
        IsLoading = true;
        RaiseItemsChanged(0);
        Task.Run(LoadPackages);
    }

    private sealed class RefreshPackagesCommand : InvokableCommand
    {
        private readonly AdbExtensionPage _page;

        public RefreshPackagesCommand(AdbExtensionPage page)
        {
            _page = page;
            Name = "Refresh";
        }

        public override ICommandResult Invoke()
        {
            _page.RefreshPackages();
            return CommandResult.KeepOpen();
        }
    }
}
