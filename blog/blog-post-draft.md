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

Global hotkey that works from anywhere, no IDE plugin, no remembering `adb` incantations, and I can extend it with whatever I end up doing the most. In this house we still use `adb`, we just don't want to *type* `adb`.

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

## Shipping it, or: the EXE path is a dead end

This is the part the official docs actively send you the wrong way on, so buckle in.

The [extension publishing docs](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/creating-an-extension) show you how to package your extension as an `.exe` with an Inno Setup installer, and drop some registry keys under `HKCU\Software\Classes\CLSID\{...}\LocalServer32`. Follow along, build, install — the palette does not see your extension. No error. No log. Nothing.

<!-- MEME PLACEHOLDER: "It's simple, just follow the docs" — confidently-wrong / galaxy brain guy -->

**First rabbit hole: the registry entries as written do nothing.** The Inno Setup snippet in the docs is missing `ValueType: string` and `ValueName: ""`. The key gets created, but the default value is empty. PowerToys finds the CLSID, has no idea where the exe lives. You fix it:

```ini
Root: HKCU; Subkey: "...\LocalServer32"; ValueType: string; ValueName: ""; \
    ValueData: "{app}\AdbExtension.exe -RegisterProcessAsComServer"
```

Classic case of docs written by someone who knows the mechanism but not the installer tooling.

**Second rabbit hole: a UAC prompt on install.** `{autopf}` resolves to Program Files, which needs elevation. Nobody wants to click through UAC for a Command Palette extension. Swap to `{localappdata}\AdbExtension` and set `PrivilegesRequired=lowest`. Done.

**Third rabbit hole: none of it matters anyway.**

Registry correct. No UAC prompt. Installs cleanly. Still does not show up in Command Palette. Anywhere.

PowerToys discovers extensions exclusively via the Windows `AppExtensionCatalog` API:

```csharp
AppExtensionCatalog.Open("com.microsoft.commandpalette").FindAllAsync();
```

That API only reads `windows.appExtension` declarations from `Package.appxmanifest` on **MSIX-packaged** apps. An EXE installer has no package identity, so it's completely invisible to this API regardless of how pristine your COM registry entries are.

I went looking for a fallback. There isn't one. Checked against the PowerToys source in `PowerToys/src/modules/cmdpal` — no registry-based discovery path, no directory scan for unpackaged extensions, not even a TODO. There *is* an `AppPackagingFlavor` enum in the codebase with values like `Unpackaged` and `UnpackagedPortable` which looks promising for about thirty seconds, then you realise it's never consulted during extension discovery. It only shows up in diagnostic logging. In this house we still use enums for decoration.

<!-- MEME PLACEHOLDER: "Spent all day fixing the car. Realised it needed a boat." — wait-it-was-never-going-to-work energy -->

**Fourth rabbit hole: Visual Studio was lying to me (kindly).** The whole reason any of this worked during development: the VS `AdbExtension (Package)` profile deploys as a proper MSIX with a dev certificate. `AppExtensionCatalog` finds it, everything works. The unpackaged EXE profile has no package identity at all. Entirely different mechanism, nothing in the docs mentions the gap, and your dev loop happily masks the production failure.

So just ship an MSIX. Easy. Right?

## Signing your MSIX. Oh well.

The goal: a proper `winget install` experience with a signed MSIX — same as every other extension in the Command Palette gallery. Turns out that's the only way the gallery install button actually works. So the signed MSIX isn't optional, it's the whole point.

Your signing options, as far as I can tell:

- **Buy a code signing cert** — DigiCert, Sectigo, the usual suspects. ~$100–300/year. For a free open source tool. Debatable.
- **Azure Trusted Signing** — Microsoft's own service, ~$10/month. Requires identity verification that takes a few business days. Still costs money.
- **SignPath.io** — free tier for genuine open source projects. Apply, wait, maybe get approved.
- **Microsoft Store** — they sign it for you. Free. The catch is you now have to deal with the Microsoft Store.

I went with the Store. Submit the MSIX, wait for certification, Microsoft signs it. Then you upload the Store-signed MSIX to your GitHub release and point your WinGet manifest at it — `InstallerType: msix`, proper `SignatureSha256`, done. `winget install` works, gallery install works, everything works.

One detail nobody writes down: the Command Palette's built-in gallery is populated from WinGet by **tag**. Add `windows-commandpalette-extension` to your manifest's `Tags` list and your extension appears in the gallery. Omit it and it doesn't. That's the whole mechanism. Of course when I first added the tag I was still shipping the broken EXE — so it was discoverable *and* broken, which is the worst of both worlds. Pulled the tag until the signed MSIX went live.

Getting the Store-signed MSIX *back out* of the Store was its own adventure. `winget download` requires an organisational Microsoft account, apparently — personal accounts need not apply. `store.rg-adguard.net` redirects to HTTP and my browser very politely refused. In the end `msft-store.tplant.com.au` worked fine.

The signed bundle it hands you contains individual per-architecture MSIXes inside — it's just a zip with a fancy extension. Rename it, extract, grab the x64 one, upload to GitHub Releases, run `winget hash --msix` on it, paste the hash into the manifest. Submit PR to winget-pkgs. Done.

<!-- MEME PLACEHOLDER: Thanos "finally. something worked." captioned exactly that -->

## Anyways

Building the extension was fine. Publishing it was a completely different beast, and the official docs will cheerfully send you down a path that cannot work. The irony of building a free tool for developers and hitting a cert paywall to distribute it is not lost on me.

Filed a docs PR on `MicrosoftDocs/windows-dev-docs` <!-- TODO: link --> and opened an issue on `microsoft/PowerToys` <!-- TODO: link --> so the next person googling "command palette extension not showing up" finds something. You're welcome, future googlers.

If you're on Windows and you do Android dev, try it:

- Repo: <https://github.com/CostaFot/AdbExtension>
- Store: <!-- TODO: link -->
- PRs welcome, issues more welcome, "you should have used [library X]" takes also welcome but I will file them under caveman solution #2.

<!-- TODO: consider a short gif of the palette flow here -->

Hope you found this somewhat useful. @ markasduplicate Later.
