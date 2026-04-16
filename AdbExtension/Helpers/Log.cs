using System;
using System.Diagnostics;
using System.IO;

namespace AdbExtension;

internal static class Log
{
    private static readonly string _logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AdbExtension", "logs", $"adbextension-{DateTime.Now:yyyy-MM-dd}.log");

    static Log()
    {
        try { Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!); }
        catch { /* if we can't create the log dir, file writes will just fail silently */ }
    }

    public static void Info(string message) => Write("INFO", message, writeToFile: false);

    public static void Error(string message, Exception? ex = null)
    {
        var text = ex is null ? message : $"{message} — {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
        Write("ERROR", text, writeToFile: true);
    }

    private static void Write(string level, string message, bool writeToFile)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}";
        Trace.WriteLine(line);
        if (writeToFile)
        {
            try { File.AppendAllText(_logPath, line + Environment.NewLine); }
            catch { /* don't crash the app if logging fails */ }
        }
    }
}
