// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class OpenDeepLinkPage : DynamicListPage
{
    private readonly string _packageName;

    public OpenDeepLinkPage(string packageName)
    {
        _packageName = packageName;
        Title = "Open Deep Link";
        Name = "Open";
        PlaceholderText = "Enter deep link URL (e.g. myapp://home)";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
        => RaiseItemsChanged(0);

    public override IListItem[] GetItems()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return [new ListItem(new NoOpCommand()) { Title = "Type a URL above to launch" }];

        return [
            new ListItem(new OpenDeepLinkCommand(_packageName, SearchText))
            {
                Title = $"Open: {SearchText}",
            },
        ];
    }
}
