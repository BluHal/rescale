using System.IO;
using System.Runtime.CompilerServices;

namespace Rescale.Services;

/// <summary>Simple file logger that writes to rescale.log next to the executable.</summary>
public static class LogService
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rescale");

    private static readonly string LogPath = Path.Combine(LogDir, "rescale.log");

    private static readonly object Lock = new();

    public static void Info(string message, [CallerMemberName] string caller = "")
    {
        Write("INFO", caller, message);
    }

    public static void Error(string message, Exception? ex = null, [CallerMemberName] string caller = "")
    {
        Write("ERROR", caller, ex != null ? $"{message} | {ex}" : message);
    }

    private static void Write(string level, string caller, string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {caller}: {message}";
        lock (Lock)
        {
            Directory.CreateDirectory(LogDir);
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }
    }
}
