using System;
using System.Windows.Threading;

namespace DynamicIsland.Hyper.Theme;

/// <summary>
/// 日间 / 夜间自动切换：每分钟检查一次当前小时，
/// 白天（默认 6:00–18:00）用浅色云母，其余用深色云母，切换时触发事件。
/// </summary>
internal sealed class ThemeScheduler
{
    /// <summary>日间开始小时（含）。</summary>
    public int DayStartHour { get; init; } = 6;

    /// <summary>夜间开始小时（含）。</summary>
    public int NightStartHour { get; init; } = 18;

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMinutes(1) };

    /// <summary>主题变化通知（仅在真正切换时触发）。</summary>
    public event Action<AppTheme>? ThemeChanged;

    public void Start()
    {
        _timer.Tick += (_, _) => Check();
        _timer.Start();
        Check();
    }

    public void Stop() => _timer.Stop();

    private void Check()
    {
        var now = DateTime.Now;
        var theme = now.Hour >= DayStartHour && now.Hour < NightStartHour
            ? AppTheme.Day
            : AppTheme.Night;

        if (theme != ThemeManager.Current)
        {
            ThemeManager.Apply(theme);
            ThemeChanged?.Invoke(theme);
        }
    }
}
