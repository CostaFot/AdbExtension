using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AdbExtension;

public class Program
{
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
            Console.WriteLine("Not being launched as a Extension... exiting.");
        }
    }
}
