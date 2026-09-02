// 别名固定指向 WPF 的 Application，避免与 System.Windows.Forms.Application 歧义（CS0104）
using Application = System.Windows.Application;
using DynamicIsland.Hyper.Services;

namespace DynamicIsland.Hyper;

public partial class App : Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        // 首次运行自动开启开机自启（托盘里关闭过就不再自动开）
        AutoStart.EnsureOnFirstRun();
    }
}
