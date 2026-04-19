---
title: I built a PowerToys Command Palette extension so I'd stop typing `adb shell pm clear` like an animal
tags: [android, adb, powertoys, windows, csharp]
draft: true
---

<!-- HOUSEKEEPING -->
Quick one this week. No Kotlin. No Compose. Just Windows, C#, and a shell command I've typed approximately eight thousand times in my career.

<!-- HOOK -->
Every Android engineer has a muscle memory for this:

```
adb shell pm clear com.my.very.long.package.name.debug
```

Then you go hunting through `adb shell pm list packages -3` because you forgot the exact package name. Then you do it again. And again. Productivity™.

I finally snapped and built a thing.

<!-- MEME PLACEHOLDER: "I've had enough of this grandpa" / old man yells at cloud, captioned "me @ my own terminal history" -->

## TL;DR

I built an [**ADB extension for the PowerToys Command Palette**](https://github.com/CostaFot/AdbExtension). `Win` + `Alt` + `Space`, type `adb`, pick an app, pick an action — clear data, force stop, grant all permissions, toggle animations, take a screenshot, install an APK, launch a deep link, the usual suspects. No terminal. No package-name-autocomplete-regret.

Open-source, [on GitHub](https://github.com/CostaFot/AdbExtension), also [in the Microsoft Store](<!-- TODO: store link -->) if you're the kind of person who likes signed MSIX bundles (you should be).

---

## "What is PowerToys Command Palette"

Fair question. If you're on macOS you already have Raycast/Alfred/Spotlight and you're probably smug about it. On Windows we had... the Start menu. And PowerToys Run, which was fine.

[**PowerToys Command Palette**](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/) is Microsoft's newer, nicer take — `Win` + `Alt` + `Space`, a searchable palette, and — crucially for this post — **an extension model**. You can ship your own commands, pages, and list items as an MSIX package. The palette discovers them, indexes them, and runs them.

It's basically VS Code's Command Palette, except it's your whole operating system. Debatable whether the world needed this. I needed this.

<!-- MEME PLACEHOLDER: the "they don't know" party guy, captioned "they don't know you can write your own PowerToys extensions" -->

## Why not just... use the terminal? / a GUI tool? / Android Studio?

- **Terminal**: that's the whole problem. Typing package names is the bit I wanted to delete.
- **Android Studio**: fires up a JVM to clear app data. No.
- **scrcpy / existing GUIs**: great for mirroring, overkill for "clear data on my debug build."
- **Just memorise the commands, you baby**: this is the correct take. I rejected it.

In this house we still use `adb`, we just don't want to *type* `adb`.

## The extension model, in ~30 seconds

An extension is a WinUI/C# class library that exposes a `CommandProvider`. The provider returns top-level items. Items can either **run an `InvokableCommand`** or **navigate into a `Page`**. Pages are lists. Lists contain items. Items contain... you see where this is going.

```csharp
internal sealed partial class ClearAppDataCommand : InvokableCommand
{
    private readonly string _packageName;

    public ClearAppDataCommand(string packageName)
    {
        _packageName = packageName;
        Name = "Clear App Data";
    }

    public override ICommandResult Invoke()
    {
        AdbHelper.RunAdb($"shell pm clear {_packageName}", out _, out string error);
        return string.IsNullOrEmpty(error)
            ? CommandResult.ShowToast($"Cleared data for {_packageName}")
            : ErrorToast($"Failed to clear data: {error}");
    }
}
```

That's it. That's the whole pattern, 18 times, with different `shell` invocations. Caveman solution.

<!-- MEME PLACEHOLDER: spongebob "caveman brain" / primitive sponge, captioned "shell out to adb.exe and parse stdout" -->

## Caveman solution #1: shelling out to `adb.exe`

There are proper ADB client libraries for .NET. I did not use them. I spawn `adb.exe` as a subprocess and read stdout/stderr like it's 1998.

```csharp
using var process = new Process();
process.StartInfo = new ProcessStartInfo
{
    FileName = "adb",
    Arguments = arguments,
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    CreateNoWindow = true,
};

process.Start();
string stdout = process.StandardOutput.ReadToEnd();
string stderr = process.StandardError.ReadToEnd();
process.WaitForExit();
```

The one trick: **read both streams before calling `WaitForExit`**, otherwise one of the pipes fills up, the child process blocks trying to write to it, and your extension hangs forever. Classic. Everyone rediscovers this the hard way exactly once.

Why not a library? Because `adb` on the user's PATH already works, already handles transport, already has the right protocol version for whatever Android build they're on. My job is to type `shell pm clear` into it. Just works™.

## The one bug that ate my weekend: `ItemsChanged` vs. the constructor

This one's worth the price of admission if you're ever writing a Command Palette extension yourself.

The setup: my package list page loads asynchronously. You don't want to block the palette while `adb shell pm list packages -3` comes back. So the obvious thing is: kick off the load, return an empty list, raise `ItemsChanged` when the data arrives. Standard.

Except the framework subscribes to `ItemsChanged` **after** it calls `GetItems` for the first time. Which means: if you fire `RaiseItemsChanged` from your constructor, or from a background task that finishes before subscription, **nobody is listening**. Your page loads, shows nothing, and sits there smugly empty.

<!-- MEME PLACEHOLDER: the "disappointed cricket" / an empty room with a single tumbleweed, captioned "my ItemsChanged event handler, in production" -->

The fix is slightly evil. `INotifyItemsChanged` is an interface. You re-implement it on the page, intercept the `add` accessor, and trigger your refresh *right there* — the moment the framework subscribes:

```csharp
internal sealed partial class AdbExtensionPage : DynamicListPage, INotifyItemsChanged
{
    private event TypedEventHandler<object, IItemsChangedEventArgs>? _itemsChanged;

    event TypedEventHandler<object, IItemsChangedEventArgs> INotifyItemsChanged.ItemsChanged
    {
        add    { _itemsChanged += value; RefreshPackages(); }  // <-- the whole point
        remove => _itemsChanged -= value;
    }

    protected new void RaiseItemsChanged(int totalItems = -1)
        => _itemsChanged?.Invoke(this, new ItemsChangedEventArgs(totalItems));
}
```

Two things that bit me:

1. You *must* use `INotifyItemsChanged.ItemsChanged`, not `IListPage.ItemsChanged`. Same name, different interface, and the framework subscribes via the first one. Very fun to debug.
2. The bonus: every time the user navigates *back* into the page, `add` fires again and we refresh. Which is actually what I want. Free feature. Oh well.

Is this the "intended" extension lifecycle? <!-- TODO: I still don't know. Ask the PowerToys folks. --> Debatable. But it works.

## What's in the box

Stuff the extension does, in no particular order:

- **Clear app data** / clear data + restart
- **Force stop**, **kill**, **uninstall**
- **Grant all / revoke all runtime permissions** (the real reason I built this)
- **Toggle animations**, **layout bounds**, **touch coordinates**
- **Toggle airplane mode**, **Wi-Fi**, **mobile data**
- **Take screenshot** (pulls straight into your clipboard, roughly)
- **Install APK** from a folder
- **Launch deep link** / open arbitrary URI
- **Favorites** for the three packages you actually use every day

The package list sorts foreground → running → debuggable → everything else, because that's the order I want them in 95% of the time. The other 5% there's a search box. Sorted.

## Shipping it

MSIX + GitHub Actions + Partner Center. `gh workflow run release-msix.yml` builds x64 + ARM64, bundles, signs, publishes a release. I wave the `.msixbundle` at the Microsoft Store and then wait for certification to either pass or tell me my privacy policy URL is wrong again.

<!-- MEME PLACEHOLDER: distracted boyfriend, labelled "me", "submitting a new MSIX build", "fixing the actual bug someone reported" -->

## Anyways

If you're on Windows and you do Android dev, try it:

- Repo: <https://github.com/CostaFot/AdbExtension>
- Store: <!-- TODO: link -->
- PRs welcome, issues more welcome, "you should have used [library X]" takes also welcome but I will file them under caveman solution #2.

<!-- TODO: consider a short gif of the palette flow here -->

Hope you found this somewhat useful. @ markasduplicate Later.
