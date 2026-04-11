using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class ToggleFavoriteCommand : InvokableCommand
{
    private readonly string _id;
    private readonly Action _refresh;

    public ToggleFavoriteCommand(string id, Action refresh)
    {
        _id = id;
        _refresh = refresh;
        Name = FavoritesStore.Instance.IsFavorite(id) ? "Remove from Favorites" : "Add to Favorites";
        Icon = new IconInfo(FavoritesStore.Instance.IsFavorite(id) ? "\uE735" : "\uE734"); // FavoriteStarFill / FavoriteStar
    }

    public override ICommandResult Invoke()
    {
        FavoritesStore.Instance.Toggle(_id);
        _refresh();
        return CommandResult.KeepOpen();
    }
}
