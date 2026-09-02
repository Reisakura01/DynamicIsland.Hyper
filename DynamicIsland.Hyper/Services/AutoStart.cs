using System.Diagnostics;
using Microsoft.Win32;

namespace DynamicIsland.Hyper.Services;

/// <summary>开机自启管理：把本 exe 注册到当前用户 Run 键；首次运行自动开启，托盘开关联动。</summary>
internal static class AutoStart
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string PrefKeyPath = @"Software\DynamicIsland.Hyper";
    private const string PrefName = "AutoStartPref"; // 1=已开启/未关闭, 0=用户主动关闭
    public const string AppName = "DynamicIsland.Hyper";

    public static bool IsEnabled()
    {
        try { using var k = Registry.CurrentUser.OpenSubKey(RunKeyPath); return k?.GetValue(AppName) != null; }
        catch { return false; }
    }

    public static void Enable()
    {
        try { using var k = Registry.CurrentUser.CreateSubKey(RunKeyPath); k?.SetValue(AppName, "\"" + ExePath + "\""); }
        catch { }
    }

    public static void Disable()
    {
        try { using var k = Registry.CurrentUser.CreateSubKey(RunKeyPath); k?.DeleteValue(AppName, false); }
        catch { }
    }

    public static void Toggle()
    {
        if (IsEnabled()) Disable(); else Enable();
        RecordPref();
    }

    /// <summary>首次运行开启自启；若用户曾在托盘里关闭过（pref=0）则不重复开启。</summary>
    public static void EnsureOnFirstRun()
    {
        bool userDisabled = false;
        try { using var p = Registry.CurrentUser.OpenSubKey(PrefKeyPath); userDisabled = (int?)p?.GetValue(PrefName) == 0; }
        catch { }
        if (!userDisabled && !IsEnabled()) Enable();
        try { using var p = Registry.CurrentUser.CreateSubKey(PrefKeyPath); p?.SetValue(PrefName, IsEnabled() ? 1 : 0); }
        catch { }
    }

    private static void RecordPref()
    {
        try { using var p = Registry.CurrentUser.CreateSubKey(PrefKeyPath); p?.SetValue(PrefName, IsEnabled() ? 1 : 0); }
        catch { }
    }

    private static string ExePath
    {
        get { try { return Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty; } catch { return string.Empty; } }
    }
}
