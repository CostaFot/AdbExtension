# ADB Extension for Command Palette — Claude Guide

## Documentation

- [Extension overview & concepts](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/extensions-overview)
- [Creating an extension (getting started guide)](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/creating-an-extension)
- [Toolkit namespace — full class list](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/microsoft-commandpalette-extensions-toolkit/microsoft-commandpalette-extensions-toolkit)
- [Command results](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/command-results)
- [Sample extensions](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/samples)

## Build & Deploy

Build via Visual Studio or:
```
dotnet build AdbExtension.sln
```
Deploy the MSIX package, then reload Command Palette to pick up changes.

## Project Conventions

- New commands → `AdbExtension/Commands/`, extend `InvokableCommand`
- New pages → `AdbExtension/Pages/`, extend `ListPage` or `DynamicListPage`
- All ADB execution goes through `AdbHelper.RunAdb()` — never use `Process` directly in command files
- Error toasts use `CommandResult.KeepOpen()` so the user can read them; success toasts use the default `Dismiss()`
- Icons: `new IconInfo("https://github.com/favicon.ico")` per project preference

## CommandPalette Toolkit — Quick Reference

### Navigation

`Page` extends `Command` (implements `ICommand`). Pass a page anywhere a command is accepted — the palette navigates into it automatically. No string-based registration needed.

```csharp
// Top-level entry point
new CommandItem(new MyPage()) { Title = "My Command" }

// List item that navigates into a sub-page
new ListItem(new MySubPage(arg)) { Title = "Go deeper" }

// List item that runs a command
new ListItem(new MyInvokableCommand()) { Title = "Do thing" }
```

### Base Classes

| Class | Use when |
|---|---|
| `InvokableCommand` | Action with no UI — runs and returns a `CommandResult` |
| `ListPage` | Static list of items |
| `DynamicListPage` | List that reacts to search input — override `UpdateSearchText()` |
| `CommandProvider` | Extension entry point — returns top-level `ICommandItem[]` |

### Page Activation Hook (On-Load Refresh)

The framework calls `FetchItems` → `GetItems()` **before** subscribing to `ItemsChanged`. This means any `RaiseItemsChanged` fired from the constructor is lost and the page shows empty on first open.

The fix: re-implement `INotifyItemsChanged` on the page class and intercept the `add` accessor. The framework subscribes via `INotifyItemsChanged.ItemsChanged +=`, so our accessor fires right after subscription — triggering a second `FetchItems` while the framework is already listening.

**Critical:** use `INotifyItemsChanged.ItemsChanged`, not `IListPage.ItemsChanged` — `ItemsChanged` lives on `INotifyItemsChanged`, and the class must re-list the interface to override base class dispatch.

```csharp
// For pages that load data async (e.g. AdbExtensionPage):
internal sealed partial class MyPage : DynamicListPage, INotifyItemsChanged
{
    private event TypedEventHandler<object, IItemsChangedEventArgs>? _itemsChanged;

    event TypedEventHandler<object, IItemsChangedEventArgs> INotifyItemsChanged.ItemsChanged
    {
        add { _itemsChanged += value; RefreshData(); } // called every time user navigates to this page
        remove => _itemsChanged -= value;
    }

    protected new void RaiseItemsChanged(int totalItems = -1)
        => _itemsChanged?.Invoke(this, new ItemsChangedEventArgs(totalItems));
}

// For pages with synchronous GetItems() (e.g. PackageActionsPage):
internal sealed partial class MyPage : ListPage, INotifyItemsChanged
{
    private event TypedEventHandler<object, IItemsChangedEventArgs>? _itemsChanged;

    event TypedEventHandler<object, IItemsChangedEventArgs> INotifyItemsChanged.ItemsChanged
    {
        add { _itemsChanged += value; _itemsChanged?.Invoke(this, new ItemsChangedEventArgs(-1)); } // fire immediately after subscribe
        remove => _itemsChanged -= value;
    }

    protected new void RaiseItemsChanged(int totalItems = -1)
        => _itemsChanged?.Invoke(this, new ItemsChangedEventArgs(totalItems));
}
```

Do **not** call `IsLoading = true` + `Task.Run(Load)` from the constructor — the event fires before the framework subscribes and the signal is lost.

### DynamicListPage Pattern

```csharp
internal sealed partial class MyPage : DynamicListPage
{
    public MyPage()
    {
        PlaceholderText = "Type to filter...";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
        => RaiseItemsChanged(0); // triggers GetItems() re-call

    public override IListItem[] GetItems()
    {
        // Filter using this.SearchText
    }
}
```

### CommandResult Options

```csharp
CommandResult.ShowToast("message")              // show toast, dismiss palette
CommandResult.ShowToast(new ToastArgs {
    Message = "message",
    Result = CommandResult.KeepOpen()           // show toast, keep palette open
})
CommandResult.KeepOpen()                        // do nothing, stay on current page
CommandResult.Dismiss()                         // close the palette
CommandResult.GoBack()                          // navigate to previous page
CommandResult.GoHome()                          // navigate to root
CommandResult.Confirm(new ConfirmationArgs())   // show confirmation dialog
```

### ListItem Properties

```csharp
new ListItem(command)
{
    Title = "Required",
    Subtitle = "Optional secondary text",
    Icon = new IconInfo("https://...") ,        // URL icon
    Section = "Section header",                 // groups items under a header
}
```

### Icon Options

```csharp
new IconInfo("https://example.com/icon.png")   // URL (light+dark same)
new IconInfo(lightIconData, darkIconData)       // separate light/dark
IconHelpers.FromRelativePath("Assets\\foo.png") // bundled asset
```

### InvokableCommand Pattern

```csharp
internal sealed partial class MyCommand : InvokableCommand
{
    public MyCommand()
    {
        Name = "Do Thing";
        Icon = new IconInfo("https://...");
    }

    public override ICommandResult Invoke()
    {
        try
        {
            // do work
            return CommandResult.ShowToast("Done");
        }
        catch (Exception ex) when (ex is Win32Exception w && w.NativeErrorCode == 2)
        {
            return ErrorToast("ADB not found. Make sure adb.exe is in your PATH.");
        }
        catch (Exception ex)
        {
            return ErrorToast($"Unexpected error: {ex.Message}");
        }
    }

    private static ICommandResult ErrorToast(string message) =>
        CommandResult.ShowToast(new ToastArgs { Message = message, Result = CommandResult.KeepOpen() });
}
```

## AdbHelper API

```csharp
// Run any adb command. Read both streams before WaitForExit (prevents pipe deadlocks).
AdbHelper.RunAdb(string arguments, out string stdout, out string stderr);

// Returns 3rd-party packages. Debuggable ones sorted first.
// Returns [] on error (no device, adb not in PATH, etc.)
PackageInfo[] AdbHelper.GetInstalledPackages();

// PackageInfo record
record PackageInfo(string Name, bool IsDebuggable);
```
