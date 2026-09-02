using System.Diagnostics;
using Microsoft.Win32;

namespace DynamicIsland.Hyper.Services;

/// <summary>开机自启管理：把本 exe 注册到当前用户 Run 键。</summary>
internal static class AutoStart
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string AppName = "DynamicIsland.Hyper";

    public static bool IsEnabled()
    {
        try { using var k = Registry.CurrentUser.OpenSubKey(RunKeyPath); return k?.GetValue(AppName) != null; }
        catch { return false; }
    }

    public static void Set(bool enabled)
    {
        try
        {
            using var k = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (k is null) return;
            if (enabled) k.SetValue(AppName, "\"" + ExePath + "\"");
            else k.DeleteValue(AppName, false);
        }
        catch { }
    }

    public static void Enable() => Set(true);
    public static void Disable() => Set(false);

    private static string ExePath
    {
        get { try { return Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty; } catch { return string.Empty; } }
    }
}
