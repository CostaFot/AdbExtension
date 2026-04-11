// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AdbExtension;

internal sealed partial class UninstallAppCommand : InvokableCommand
{
    private readonly string _packageName;
    private readonly Action _refreshPackageList;

    public UninstallAppCommand(string packageName, Action refreshPackageList)
    {
        _packageName = packageName;
        _refreshPackageList = refreshPackageList;
        Name = "Uninstall";
    }

    public override ICommandResult Invoke()
    {
        if (AdbSettingsManager.Instance.SkipUninstallConfirmation)
            return new DoUninstallCommand(_packageName, _refreshPackageList).Invoke();

        return CommandResult.Confirm(new ConfirmationArgs
        {
            Title = $"Uninstall {_packageName}?",
            Description = "This will remove the app and all its data from the device.",
            IsPrimaryCommandCritical = true,
            PrimaryCommand = new DoUninstallCommand(_packageName, _refreshPackageList),
        });
    }

    private sealed partial class DoUninstallCommand : InvokableCommand
    {
        private readonly string _packageName;
        private readonly Action _refreshPackageList;

        public DoUninstallCommand(string packageName, Action refreshPackageList)
        {
            _packageName = packageName;
            _refreshPackageList = refreshPackageList;
            Name = "Uninstall";
        }

        public override ICommandResult Invoke()
        {
            try
            {
                AdbHelper.RunAdb($"shell pm uninstall {_packageName}", out _, out string error);
                if (string.IsNullOrEmpty(error))
                {
                    _refreshPackageList();
                    return CommandResult.GoBack();
                }
                return ErrorToast($"Failed to uninstall: {error}");
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
}
