using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class InstallApksPage : DynamicListPage
{
    public InstallApksPage()
    {
        Icon = new IconInfo("\uE896"); // Download
        Title = "APK Manager";
        Name = "Open";
        PlaceholderText = "Override folder path...";
        RaiseItemsChanged(0);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
        => RaiseItemsChanged(0);

    public override IListItem[] GetItems()
    {
        var folderPath = string.IsNullOrWhiteSpace(SearchText)
            ? AdbSettingsManager.Instance.ApkFolder
            : SearchText.Trim();

        if (string.IsNullOrEmpty(folderPath))
            return [];

        if (!Directory.Exists(folderPath))
            return [new ListItem(new NoOpCommand()) { Title = $"Folder not found: {folderPath}" }];

        var apks = Directory.GetFiles(folderPath, "*.apk", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f)
            .ToArray();

        if (apks.Length == 0)
            return [
                new ListItem(new NoOpCommand()) { Title = $"No APK files found in {folderPath}" },
                new ListItem(new RefreshCommand(this)) { Title = "Refresh" },
            ];

        var items = new List<IListItem>();

        items.Add(new ListItem(new InstallAllApksPage(apks))
        {
            Title = $"Install All ({apks.Length} APK{(apks.Length == 1 ? "" : "s")})",
            Icon = new IconInfo("\uE896"),
        });

        items.AddRange(apks.Select(apk =>
            new ListItem(new InstallApkCommand(apk))
            {
                Title = Path.GetFileName(apk),
                Subtitle = apk,
            }));

        items.Add(new ListItem(new RefreshCommand(this)) { Title = "Refresh" });

        return items.ToArray();
    }

    private sealed class RefreshCommand : InvokableCommand
    {
        private readonly InstallApksPage _page;

        public RefreshCommand(InstallApksPage page)
        {
            _page = page;
            Name = "Refresh";
            Icon = new IconInfo("\uE72C"); // Refresh
        }

        public override ICommandResult Invoke()
        {
            _page.RaiseItemsChanged(0);
            return CommandResult.KeepOpen();
        }
    }
}
