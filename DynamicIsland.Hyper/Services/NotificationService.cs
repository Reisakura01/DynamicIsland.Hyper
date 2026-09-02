using System;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace DynamicIsland.Hyper.Services;

/// <summary>
/// 通知实时活动：监听系统 Toast 通知，读取来源 App + 通知正文（发送人/消息），
/// 回调展示文本。首次 InitializeAsync 会弹"允许访问通知"的系统权限询问。
/// </summary>
internal sealed class NotificationService : IDisposable
{
    private UserNotificationListener? _listener;

    /// <summary>新通知（参数为展示文本，如 "📩 微信 · 张三：在吗"）。</summary>
    public event Action<string>? NotificationAdded;

    public async Task<bool> InitializeAsync()
    {
        _listener = UserNotificationListener.Current;
        var status = await _listener.RequestAccessAsync();
        if (status != UserNotificationListenerAccessStatus.Allowed)
        {
            _listener = null;
            return false;
        }

        _listener.NotificationChanged += OnNotificationChanged;
        return true;
    }

    private void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
    {
        if (args.ChangeKind != UserNotificationChangedKind.Added) return;

        // 事件参数只带通知 ID，用 GetNotification(ID) 取通知对象
        var notification = sender.GetNotification(args.UserNotificationId);
        NotificationAdded?.Invoke(BuildDisplay(notification));
    }

    private static string BuildDisplay(UserNotification? n)
    {
        var appName = n?.AppInfo?.DisplayInfo?.DisplayName ?? "通知";
        var message = ExtractText(n);
        return string.IsNullOrEmpty(message)
            ? $"📩 {appName}"
            : $"📩 {appName} · {message}";
    }

    /// <summary>从 Toast 通用模板读取正文文本（通常 [发送人, 消息]，用 "：" 连接）。失败返回空串。</summary>
    private static string ExtractText(UserNotification? n)
    {
        try
        {
            var visual = n?.Notification?.Visual;
            if (visual is null) return string.Empty;
            var binding = visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            var texts = binding?.GetTextElements();
            if (texts is null || texts.Count == 0) return string.Empty;
            return string.Join("：", texts);
        }
        catch
        {
            return string.Empty;
        }
    }

    public void Dispose()
    {
        if (_listener is not null) _listener.NotificationChanged -= OnNotificationChanged;
    }
}
