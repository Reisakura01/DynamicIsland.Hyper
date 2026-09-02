using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DynamicIsland.Hyper.Services;

/// <summary>系统托盘图标：由于岛窗口不进任务栏，提供托盘菜单（开机自启开关 + 退出）。</summary>
internal sealed class TrayIcon : IDisposable
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "DynamicIsland.Hyper";
    private readonly NotifyIcon _icon;

    public TrayIcon()
    {
        var menu = new ContextMenuStrip();

        var autoStart = new ToolStripMenuItem("开机自启") { Checked = IsAutoStartEnabled() };
        autoStart.Click += (_, _) => { ToggleAutoStart(); autoStart.Checked = IsAutoStartEnabled(); };
        menu.Items.Add(autoStart);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出Hyper 灵动岛", null, (_, _) => System.Windows.Application.Current.Shutdown());

        _icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Hyper 灵动岛",
            ContextMenuStrip = menu,
            Visible = true,
        };
    }

    private static string ExePath
    {
        get { try { return Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty; } catch { return string.Empty; } }
    }

    private static bool IsAutoStartEnabled()
    {
        try { using var k = Registry.CurrentUser.OpenSubKey(RunKeyPath); return k?.GetValue(AppName) != null; }
        catch { return false; }
    }

    private static void ToggleAutoStart()
    {
        try
        {
            using var k = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (k is null) return;
            if (IsAutoStartEnabled()) k.DeleteValue(AppName, false);
            else k.SetValue(AppName, "\"" + ExePath + "\"");
        }
        catch { }
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
