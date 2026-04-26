using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AdbExtension;

public class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [MTAThread]
    public static void Main(string[] args)
    {
        Log.Info("AdbExtension starting");

        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
        {
            Log.Info("RegisterProcessAsComServer mode detected.");
            try
            {
                global::Shmuelie.WinRTServer.ComServer server = new();
                ManualResetEvent extensionDisposedEvent = new(false);
                AdbExtension extensionInstance = new(extensionDisposedEvent);
                server.RegisterClass<AdbExtension, IExtension>(() => extensionInstance);
                Log.Info("COM server registered. Starting...");
                server.Start();
                Log.Info("COM server started. Waiting for disposal signal.");
                extensionDisposedEvent.WaitOne();
                Log.Info("Disposal signal received. Stopping server.");
                server.Stop();
                server.UnsafeDispose();
            }
            catch (Exception ex)
            {
                Log.Error("COM server failed", ex);
            }
        }
        else
        {
            MessageBox(
                IntPtr.Zero,
                "ADB Extension for Command Palette is a background extension.\n\nTo use it, open PowerToys Command Palette and search for \"ADB\".",
                "ADB Extension for Command Palette",
                0x40 /* MB_ICONINFORMATION */);
        }
    }
}
