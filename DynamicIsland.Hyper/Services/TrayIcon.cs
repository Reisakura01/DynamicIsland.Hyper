using System;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicIsland.Hyper.Services;

/// <summary>系统托盘图标：由于岛窗口不进任务栏，提供托盘菜单（开机自启开关 + 退出）。</summary>
internal sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _icon;

    public TrayIcon()
    {
        var menu = new ContextMenuStrip();

        var autoStart = new ToolStripMenuItem("开机自启") { Checked = AutoStart.IsEnabled() };
        autoStart.Click += (_, _) => { AutoStart.Toggle(); autoStart.Checked = AutoStart.IsEnabled(); };
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

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
