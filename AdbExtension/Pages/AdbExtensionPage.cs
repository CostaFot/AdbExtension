// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class AdbExtensionPage : ListPage
{
    public AdbExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "ADB Extension";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        return [
            new ListItem(new TakeScreenshotCommand())
            {
                Title = "Take Screenshot",
                Subtitle = "Capture a screenshot from the connected Android device",
                Icon = new IconInfo("https://github.com/favicon.ico"),
            },
        ];
    }
}
