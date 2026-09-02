// 别名固定指向 WPF 的 Application，避免与 System.Windows.Forms.Application 歧义（CS0104）
using Application = System.Windows.Application;
using HyperMoeland.Services;

namespace HyperMoeland;

public partial class App : Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        // 加载设置并应用开机自启
        SettingsService.Load();
        AutoStart.Set(SettingsService.Current.AutoStart);
    }
}
