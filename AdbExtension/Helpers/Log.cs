using System;
using System.Diagnostics;

namespace AdbExtension;

internal static class Log
{
    public static void Info(string message) => Write("INFO", message);

    public static void Error(string message, Exception? ex = null)
    {
        Write("ERROR", ex is null ? message : $"{message} — {ex.GetType().Name}: {ex.Message}");
    }

    private static void Write(string level, string message)
        => Trace.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}");
}
