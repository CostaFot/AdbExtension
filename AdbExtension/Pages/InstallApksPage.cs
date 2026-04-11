using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace AdbExtension;

internal sealed partial class InstallApksPage : DynamicListPage, INotifyItemsChanged
{
    private event TypedEventHandler<object, IItemsChangedEventArgs>? _itemsChanged;

    event TypedEventHandler<object, IItemsChangedEventArgs> INotifyItemsChanged.ItemsChanged
    {
        add { _itemsChanged += value; _itemsChanged?.Invoke(this, new ItemsChangedEventArgs(-1)); }
        remove => _itemsChanged -= value;
    }

    protected new void RaiseItemsChanged(int totalItems = -1)
        => _itemsChanged?.Invoke(this, new ItemsChangedEventArgs(totalItems));

    private readonly object _lock = new();
    private bool _installing;
    private string? _currentApk;
    private readonly Dictionary<string, (bool Success, string Error)> _results = [];

    public InstallApksPage()
    {
        Icon = new IconInfo("\uE896"); // Download
        Title = "APK Manager";
        Name = "Open";
        PlaceholderText = "Enter folder path...";
        SetSearchNoUpdate(AdbSettingsManager.Instance.ApkFolder);
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        lock (_lock) { _results.Clear(); _installing = false; _currentApk = null; }
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems()
    {
        var folderPath = SearchText.Trim();

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

        bool installing;
        string? currentApk;
        Dictionary<string, (bool Success, string Error)> results;
        lock (_lock)
        {
            installing = _installing;
            currentApk = _currentApk;
            results = new Dictionary<string, (bool, string)>(_results);
        }

        var items = new List<IListItem>();

        if (installing)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = $"Installing... ({results.Count}/{apks.Length})",
                Icon = new IconInfo("\uE896"), // Download
            });
        }
        else if (results.Count > 0)
        {
            int succeeded = results.Count(r => r.Value.Success);
            items.Add(new ListItem(new InstallAllCommand(this, apks))
            {
                Title = $"Install All — {succeeded}/{results.Count} succeeded. Run again?",
                Icon = new IconInfo("\uE896"),
            });
        }
        else
        {
            items.Add(new ListItem(new InstallAllCommand(this, apks))
            {
                Title = $"Install All ({apks.Length} APK{(apks.Length == 1 ? "" : "s")})",
                Icon = new IconInfo("\uE896"),
            });
        }

        items.AddRange(apks.Select(apk =>
        {
            IconInfo icon;
            string subtitle;

            if (apk == currentApk)
            {
                icon = new IconInfo("\uE916"); // Clock
                subtitle = "Installing...";
            }
            else if (results.TryGetValue(apk, out var result))
            {
                icon = new IconInfo(result.Success ? "\uE73E" : "\uE711"); // Checkmark / Cancel
                subtitle = result.Success ? "Installed" : result.Error;
            }
            else
            {
                icon = new IconInfo("\uE896"); // Download
                subtitle = apk;
            }

            return (IListItem)new ListItem(new InstallApkCommand(apk))
            {
                Title = Path.GetFileName(apk),
                Subtitle = subtitle,
                Icon = icon,
            };
        }));

        if (!installing)
            items.Add(new ListItem(new RefreshCommand(this)) { Title = "Refresh" });

        return items.ToArray();
    }

    internal void StartInstallAll(string[] apks)
    {
        lock (_lock)
        {
            if (_installing) return;
            _installing = true;
            _results.Clear();
            _currentApk = null;
        }
        RaiseItemsChanged(0);
        Task.Run(() => InstallAll(apks));
    }

    private void InstallAll(string[] apks)
    {
        foreach (var apk in apks)
        {
            lock (_lock) _currentApk = apk;
            RaiseItemsChanged(0);

            AdbHelper.RunAdb($"install -r \"{apk}\"", out _, out string error);

            lock (_lock) _results[apk] = (string.IsNullOrEmpty(error), error);
            RaiseItemsChanged(0);
        }

        lock (_lock) { _currentApk = null; _installing = false; }
        RaiseItemsChanged(0);
    }

    private sealed class InstallAllCommand : InvokableCommand
    {
        private readonly InstallApksPage _page;
        private readonly string[] _apks;

        public InstallAllCommand(InstallApksPage page, string[] apks)
        {
            _page = page;
            _apks = apks;
            Name = "Install All";
        }

        public override ICommandResult Invoke()
        {
            _page.StartInstallAll(_apks);
            return CommandResult.KeepOpen();
        }
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
            lock (_page._lock) { _page._results.Clear(); _page._currentApk = null; }
            _page.RaiseItemsChanged(0);
            return CommandResult.KeepOpen();
        }
    }
}
