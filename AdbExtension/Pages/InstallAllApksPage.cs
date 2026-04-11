using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class InstallAllApksPage : ListPage
{
    private readonly string[] _apkPaths;
    private readonly List<(string Name, bool Success, string Error)> _results = [];

    public InstallAllApksPage(string[] apkPaths)
    {
        _apkPaths = apkPaths;
        Icon = new IconInfo("\uE896"); // Download
        Title = $"Installing {apkPaths.Length} APKs...";
        Name = "Install All";
        IsLoading = true;
        Task.Run(InstallAll);
    }

    private void InstallAll()
    {
        foreach (var apk in _apkPaths)
        {
            AdbHelper.RunAdb($"install -r \"{apk}\"", out _, out string error);
            lock (_results)
            {
                _results.Add((Path.GetFileName(apk), string.IsNullOrEmpty(error), error));
            }
            RaiseItemsChanged(0);
        }

        int succeeded = _results.Count(r => r.Success);
        Title = succeeded == _apkPaths.Length
            ? $"All {_apkPaths.Length} APKs installed"
            : $"{succeeded}/{_apkPaths.Length} APKs installed";
        IsLoading = false;
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems()
    {
        lock (_results)
        {
            return _results.Select(r => (IListItem)new ListItem(new NoOpCommand())
            {
                Title = r.Name,
                Subtitle = r.Success ? "Installed" : r.Error,
                Icon = new IconInfo(r.Success ? "\uE73E" : "\uE711"), // Checkmark / Cancel
            }).ToArray();
        }
    }
}
