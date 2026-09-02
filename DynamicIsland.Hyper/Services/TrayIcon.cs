using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicIsland.Hyper.Services;

/// <summary>系统托盘图标：设置、开机自启开关、退出。</summary>
internal sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _icon;
    private bool _updateConnected;

    /// <summary>点击"设置"菜单触发。</summary>
    public event Action? OpenSettings;

    public TrayIcon()
    {
        var menu = new ContextMenuStrip();

        var settings = new ToolStripMenuItem("设置");
        settings.Click += (_, _) => OpenSettings?.Invoke();
        menu.Items.Add(settings);
        menu.Items.Add(new ToolStripSeparator());

        var autoStart = new ToolStripMenuItem("开机自启") { Checked = AutoStart.IsEnabled() };
        autoStart.Click += (_, _) =>
        {
            AutoStart.Set(!AutoStart.IsEnabled());
            SettingsService.Current.AutoStart = AutoStart.IsEnabled();
            SettingsService.Save();
            autoStart.Checked = AutoStart.IsEnabled();
        };
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

    /// <summary>托盘气泡显示"发现新版本"，点击打开下载页。</summary>
    public void ShowUpdate(string message, string url)
    {
        _icon.ShowBalloonTip(8000, "Hyper 灵动岛", message, ToolTipIcon.Info);
        if (!_updateConnected)
        {
            _updateConnected = true;
            _icon.BalloonTipClicked += (_, _) => OpenUrl(url);
        }
    }

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { }
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
