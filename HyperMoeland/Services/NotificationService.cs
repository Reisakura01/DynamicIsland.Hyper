using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace HyperMoeland.Services;

/// <summary>
/// 通知实时活动：监听系统 Toast 通知，读取来源 App + 通知正文（发送人/消息），
/// 回调展示文本。首次 InitializeAsync 会弹"允许访问通知"的系统权限询问。
///
/// 说明：UserNotificationListener.NotificationChanged 事件在无打包 Win32 应用上
/// 订阅会抛 0x80070490 (ERROR_NOT_FOUND)，因此改用「定时轮询 GetNotificationsAsync」
/// 对比上次快照来检测新增通知（更可靠，跨版本兼容）。
/// </summary>
internal sealed class NotificationService : IDisposable
{
    private UserNotificationListener? _listener;
    private readonly HashSet<uint> _seenIds = new();

    /// <summary>新通知（参数为展示文本，如 "📩 微信 · 张三：在吗"）。</summary>
    public event Action<string>? NotificationAdded;

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _listener = UserNotificationListener.Current;
            var status = await _listener.RequestAccessAsync();
            if (status != UserNotificationListenerAccessStatus.Allowed)
            {
                _listener = null;
                return false;
            }
            // 初始化时把现有通知都记为"已见过"，避免启动时把历史通知全弹出来
            await SnapshotAsync();
            return true;
        }
        catch
        {
            _listener = null;
            return false;
        }
    }

    /// <summary>轮询一次：把新增的 Toast 通知回调出去（供外部定时器调用）。</summary>
    public async Task PollAsync()
    {
        if (_listener is null) return;
        try
        {
            var notifs = await _listener.GetNotificationsAsync(NotificationKinds.Toast);
            foreach (var n in notifs)
            {
                // 每条独立 try/catch：某些通知的 AppInfo 访问会抛 NotImplementedException，
                // 不能让它中断整轮遍历。
                try
                {
                    if (_seenIds.Add(n.Id))
                        NotificationAdded?.Invoke(BuildDisplay(n));
                }
                catch
                {
                    // 跳过无法读取的通知
                }
            }
        }
        catch
        {
            // 本轮轮询失败，下一轮重试
        }
    }

    /// <summary>仅更新快照，不触发回调（初始化时用）。</summary>
    private async Task SnapshotAsync()
    {
        if (_listener is null) return;
        try
        {
            var notifs = await _listener.GetNotificationsAsync(NotificationKinds.Toast);
            foreach (var n in notifs) _seenIds.Add(n.Id);
        }
        catch { }
    }

    private static string BuildDisplay(UserNotification n)
    {
        string appName = "通知";
        try { appName = n.AppInfo?.DisplayInfo?.DisplayName ?? "通知"; }
        catch { /* 某些通知的 AppInfo 抛 NotImplementedException，用默认名 */ }

        var message = ExtractText(n);
        return string.IsNullOrEmpty(message)
            ? $"📩 {appName}"
            : $"📩 {appName} · {message}";
    }

    /// <summary>从 Toast 通用模板读取正文文本（通常 [发送人, 消息]，用 "：" 连接）。失败返回空串。</summary>
    private static string ExtractText(UserNotification n)
    {
        try
        {
            var visual = n.Notification?.Visual;
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
        _listener = null;
    }
}
