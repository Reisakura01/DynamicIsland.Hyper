using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DynamicIsland.Hyper.Views;

/// <summary>
/// 展开态卡片（小米超级岛样式）：无媒体 → 时钟卡（时间/日期/电量）；
/// 播放中 → 大媒体面板（大封面 + 标题/歌手 + 控制按钮 + 进度条），点击收回。
/// </summary>
public partial class ExpandedCard : UserControl
{
    public event EventHandler? Clicked;
    public event EventHandler? PreviousClicked;
    public event EventHandler? PlayPauseClicked;
    public event EventHandler? NextClicked;
    public event Action<double>? SeekRequested;

    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly DispatcherTimer _notificationTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private double? _progressRatio;
    private double _currentDuration;
    private bool _liked;

    public ExpandedCard()
    {
        InitializeComponent();
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
        _notificationTimer.Tick += (_, _) => { NotificationText.Visibility = Visibility.Collapsed; _notificationTimer.Stop(); };
        ProgressTrack.SizeChanged += (_, _) => ApplyProgressRatio();
        UpdateClock();
    }

    /// <summary>设置媒体信息（null/空白表示无媒体）。</summary>
    public void SetMedia(string? title, string? artist = null, BitmapImage? cover = null, bool isPlaying = false)
    {
        bool hasMedia = !string.IsNullOrWhiteSpace(title);
        MediaArea.Visibility = hasMedia ? Visibility.Visible : Visibility.Collapsed;
        ClockView.Visibility = hasMedia ? Visibility.Collapsed : Visibility.Visible;

        if (hasMedia)
        {
            MediaTitle.Text = title;
            bool hasArtist = !string.IsNullOrWhiteSpace(artist);
            MediaArtist.Visibility = hasArtist ? Visibility.Visible : Visibility.Collapsed;
            MediaArtist.Text = artist ?? string.Empty;
            CoverImage.Source = cover;
            PlayPauseIcon.Data = (Geometry)FindResource(isPlaying ? "PausePath" : "PlayPath");
        }
        else
        {
            MediaTitle.Text = string.Empty;
            MediaArtist.Text = string.Empty;
            MediaArtist.Visibility = Visibility.Collapsed;
            CoverImage.Source = null;
            PlayPauseIcon.Data = (Geometry)FindResource("PausePath");
        }
    }

    /// <summary>设置播放进度（秒）：有总时长才显示整行进度（含时间）；无时间轴数据时整行隐藏。</summary>
    public void SetProgress(double? position, double? duration)
    {
        double dur = duration ?? 0;
        if (dur <= 0)
        {
            ProgressRow.Visibility = Visibility.Collapsed;
            _progressRatio = null;
            MediaProgressFill.Width = 0;
            return;
        }

        double pos = Math.Clamp(position ?? 0, 0, dur);
        _currentDuration = dur;
        ProgressRow.Visibility = Visibility.Visible;
        ProgressTrack.Visibility = Visibility.Visible;   // 关键：显示中间的进度线（之前漏了这条）
        _progressRatio = Math.Clamp(pos / dur, 0, 1);
        ApplyProgressRatio();
        MediaTimePos.Text = FormatTime(pos);
        MediaTimeDur.Text = FormatTime(dur);
    }

    /// <summary>点击进度条跳转：按点击位置换算成秒，触发 SeekRequested。</summary>
    private void OnProgressTrackMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true; // 不冒泡到根，避免触发"点击卡片收回"
        if (_currentDuration <= 0 || ProgressTrack.ActualWidth <= 0) return;
        var x = e.GetPosition(ProgressTrack).X;
        var ratio = Math.Clamp(x / ProgressTrack.ActualWidth, 0, 1);
        SeekRequested?.Invoke(ratio * _currentDuration);
    }

    private static string FormatTime(double sec)
    {
        if (sec < 0) sec = 0;
        var ts = TimeSpan.FromSeconds(sec);
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}"
            : $"{ts.Minutes:00}:{ts.Seconds:00}";
    }

    private void ApplyProgressRatio()
    {
        if (_progressRatio is null) return;
        var trackW = ProgressTrack.ActualWidth;
        MediaProgressFill.Width = trackW > 0 ? trackW * _progressRatio.Value : 0;
    }

    /// <summary>短暂显示一条通知（5 秒后自动消失；播放媒体时忽略通知）。</summary>
    public void ShowNotification(string text)
    {
        if (MediaArea.Visibility == Visibility.Visible) return;
        NotificationText.Text = text;
        NotificationText.Visibility = Visibility.Visible;
        _notificationTimer.Stop();
        _notificationTimer.Start();
    }

    /// <summary>设置电量显示。</summary>
    public void SetBattery(double? percent)
        => BatteryText.Text = percent is double p ? $"电量 {p:0}%" : "电量 --";

    private void UpdateClock()
    {
        var now = DateTime.Now;
        TimeText.Text = now.ToString("HH:mm:ss");
        DateText.Text = now.ToString("yyyy年M月d日 dddd");
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 点媒体控制按钮（切歌/播放暂停）不算"点击卡片收回"
        if (e.OriginalSource is DependencyObject src && IsInside(src, MediaControls))
            return;
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsInside(DependencyObject node, DependencyObject container)
    {
        for (var n = node; n is not null; n = VisualTreeHelper.GetParent(n))
        {
            if (ReferenceEquals(n, container)) return true;
        }
        return false;
    }

    private void OnPreviousClicked(object sender, RoutedEventArgs e)
        => PreviousClicked?.Invoke(this, EventArgs.Empty);

    private void OnPlayPauseClicked(object sender, RoutedEventArgs e)
        => PlayPauseClicked?.Invoke(this, EventArgs.Empty);

    private void OnNextClicked(object sender, RoutedEventArgs e)
        => NextClicked?.Invoke(this, EventArgs.Empty);

    /// <summary>喜欢/取消喜欢（仅本地视觉切换；Windows SMTC 没有"喜欢"API）。</summary>
    private void OnLikeClicked(object sender, RoutedEventArgs e)
    {
        _liked = !_liked;
        LikeIcon.Fill = _liked
            ? (Brush)FindResource("Theme.Foreground")
            : new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80));
    }
}
