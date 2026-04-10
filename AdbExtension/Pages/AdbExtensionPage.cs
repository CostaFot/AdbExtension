// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class AdbExtensionPage : DynamicListPage
{
    private PackageInfo[]? _packages;

    public AdbExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "ADB App Commands";
        Name = "Open";
        PlaceholderText = "Search packages...";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
        => RaiseItemsChanged(0);

    public override IListItem[] GetItems()
    {
        _packages ??= AdbHelper.GetInstalledPackages();

        var items = new List<IListItem>();

        if (string.IsNullOrEmpty(SearchText))
        {
            items.Add(new ListItem(new RefreshPackagesCommand(this))
            {
                Title = "Refresh 🔄️",
            });
        }

        var source = string.IsNullOrEmpty(SearchText)
            ? _packages
            : _packages.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToArray();

        if (source.Length == 0)
        {
            items.Add(new ListItem(new NoOpCommand()) { Title = "No packages found 😔" });
            return items.ToArray();
        }

        items.AddRange(source.Select(pkg => (IListItem)new ListItem(new PackageActionsPage(pkg.Name))
        {
            Title = pkg.Name,
            Subtitle = pkg.IsDebuggable ? "debuggable" : null,
            Section = pkg.IsDebuggable ? "Debuggable" : "Other",
        }));

        return items.ToArray();
    }

    internal void RefreshPackages()
    {
        _packages = null;
        RaiseItemsChanged(0);
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
