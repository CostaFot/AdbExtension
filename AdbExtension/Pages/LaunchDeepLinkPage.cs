// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class LaunchDeepLinkPage : DynamicListPage
{
    public LaunchDeepLinkPage()
    {
        Title = "Launch Deep Link";
        Name = "Open";
        PlaceholderText = "Enter URL or deep link (e.g. https://example.com or myapp://home)";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
        => RaiseItemsChanged(0);

    public override IListItem[] GetItems()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return [new ListItem(new NoOpCommand()) { Title = "Type a URL or deep link above to launch" }];

        return [
            new ListItem(new LaunchDeepLinkCommand(SearchText))
            {
                Title = $"Launch: {SearchText}",
                Subtitle = "adb shell am start -a android.intent.action.VIEW",
            },
        ];
    }
}
