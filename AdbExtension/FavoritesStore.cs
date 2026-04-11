using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal static class ActionIds
{
    public const string Launch           = "launch";
    public const string KillProcess      = "kill";
    public const string ClearAppData          = "clear";
    public const string ClearDataAndRestart   = "clear-restart";
    public const string RestartApp            = "restart";
    public const string ForceStop        = "force-stop";
    public const string OpenDeepLink     = "deep-link";
    public const string Uninstall        = "uninstall";
    public const string GrantPermissions = "grant-perms";
    public const string RevokePermissions = "revoke-perms";
}

internal sealed class FavoritesStore
{
    public static readonly FavoritesStore Instance = new();

    private readonly Lock _lock = new();
    private HashSet<string> _favorites = [];

    private static string JsonPath
    {
        get
        {
            var directory = Utilities.BaseSettingsPath("Microsoft.CmdPal");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "adb_favorites.json");
        }
    }

    private FavoritesStore() => Load();

    public bool IsFavorite(string id)
    {
        lock (_lock) return _favorites.Contains(id);
    }

    public void Toggle(string id)
    {
        lock (_lock)
        {
            if (!_favorites.Remove(id))
                _favorites.Add(id);
        }

        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(JsonPath))
                return;

            var json = File.ReadAllText(JsonPath);
            var ids = JsonSerializer.Deserialize<List<string>>(json);
            if (ids != null)
                _favorites = [.. ids];
        }
        catch
        {
            _favorites = [];
        }
    }

    private void Save()
    {
        try
        {
            List<string> snapshot;
            lock (_lock) snapshot = [.. _favorites];
            File.WriteAllText(JsonPath, JsonSerializer.Serialize(snapshot));
        }
        catch
        {
            // best-effort
        }
    }
}
