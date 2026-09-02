// 别名固定指向 WPF 的 Application，避免与 System.Windows.Forms.Application 歧义（CS0104）
using Application = System.Windows.Application;
using HyperMoeland.Interop;
using HyperMoeland.Services;

namespace HyperMoeland;

public partial class App : Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        // 设置稳定的 AUMID（必须在通知/Toast 相关 WinRT 调用前），
        // 否则 UserNotificationListener 订阅会报 0x80070490 而收不到通知。
        NativeMethods.SetAppUserModelId("MoeOrigin.HyperMoeland");

        // 加载设置并应用开机自启 + 语言
        SettingsService.Load();
        AutoStart.Set(SettingsService.Current.AutoStart);
        LocalizationService.SetLanguage(SettingsService.Current.Language);
    }
}
