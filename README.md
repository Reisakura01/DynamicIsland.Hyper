# DynamicIsland.Hyper — Hyper 灵动岛

iOS 风格灵动岛，Windows 11 实现：顶部居中的**云母（Mica）胶囊**，点击展开成卡片，
**按时间自动切换日间（浅色云母）/ 夜间（深色云母）**。

- 技术栈：**VS2026 Community / .NET 10 / WPF（无 WinUI）**
- Windows SDK：**10.0.26100**
- 运行环境：Windows 11（云母仅 Win11 支持）

---

## 功能清单

- 🐋 **云母胶囊**：常驻顶部居中，无边框、置顶、不抢焦点、不进任务栏
- ⏰ **时钟实时活动**：无媒体时显示时间，展开卡片显示完整时间/日期
- 🎵 **媒体会话（SMTC）**：播放音乐时胶囊显示曲名 · 歌手，卡片显示"正在播放"
- 📩 **系统通知**：新通知在卡片上短暂显示（5 秒自动消失，首次需授权）
- 🔋 **电量**：卡片显示电量百分比
- 🌗 **日间/夜间自动切换**：默认 6:00–18:00 白天（浅色云母），其余夜间（深色云母）
- 🖥️ **全屏自动隐藏**：前台窗口全屏（视频/游戏）时自动隐藏，退出全屏恢复
- 🖥️ **多显示器跟随**：跟随前台窗口所在屏幕
- 📌 **系统托盘**：托盘图标右键可退出（因窗口不进任务栏）

## 设计核心

```
窗口即岛：一个透明、置顶、不抢焦点的小窗口
  紧凑态 = 云母胶囊（190×46）
  展开态 = 云母卡片（420×300）
```

- **云母**：`DwmSetWindowAttribute(DWMWA_SYSTEMBACKDROP_TYPE, DWMSBT_MAINWINDOW)` 选云母，
  再用 `SetWindowCompositionAttribute(ACCENT_ENABLE_HOSTBACKDROP)` 让透明窗口承载它；
- **日间 / 夜间**：`ThemeScheduler` 每分钟检查当前小时，通过 `DWMWA_USE_IMMERSIVE_DARK_MODE`
  切换云母深浅，同时替换前景 / 边框画刷；
- **动画**：WPF 原生 `DoubleAnimation` + `QuinticEase` 弹性缩放窗口。

## 目录结构

```
DynamicIsland.Hyper/
├── DynamicIsland.Hyper.sln
└── DynamicIsland.Hyper/
    ├── DynamicIsland.Hyper.csproj   # net10.0-windows10.0.26100.0 + UseWPF + UseWindowsForms
    ├── App.xaml(.cs)               # 入口 + 主题画刷定义
    ├── MainWindow.xaml(.cs)        # 岛窗口：透明置顶 + 动画 + 服务接线
    ├── Core/
    │   ├── IslandState.cs          # 状态枚举 + 尺寸常量
    │   └── IslandController.cs     # 状态机
    ├── Models/
    │   └── MediaSessionInfo.cs     # 媒体会话快照
    ├── Theme/
    │   ├── AppTheme.cs             # Day / Night
    │   ├── ThemeManager.cs         # 画刷资源切换
    │   └── ThemeScheduler.cs       # 按时间自动切换
    ├── Interop/
    │   ├── NativeMethods.cs        # DWM / 窗口样式 / 显示器 / 前台窗口 P/Invoke
    │   ├── MicaController.cs       # 应用云母 + 主题
    │   └── MonitorHelper.cs        # 主屏 / 指定屏工作区
    ├── Services/
    │   ├── MediaService.cs         # SMTC 媒体会话
    │   ├── NotificationService.cs  # 系统通知
    │   ├── BatteryService.cs       # 电量
    │   ├── ForegroundWatcher.cs    # 全屏检测 + 多屏跟随
    │   └── TrayIcon.cs             # 系统托盘退出
    └── Views/
        ├── CompactPill.xaml(.cs)   # 胶囊（时间/媒体）
        └── ExpandedCard.xaml(.cs)  # 卡片（时间/媒体/通知/电量）
```

## 构建与运行

1. VS2026 Community 打开 `DynamicIsland.Hyper.sln`；
2. 确认安装了 **.NET 10 SDK** 和 **Windows SDK 10.0.26100**（VS Installer 勾选）；
3. 直接 **F5**（或 Ctrl+Shift+B 编译）。

运行后：顶部中央出现云母胶囊，显示时间；点击展开成卡片，再点收回；
播放音乐胶囊显示曲名；到 18:00 后自动切深色云母，早 6:00 切回浅色。

## 云母排错（重要）

WPF 的云母是出了名的"难伺候"，如果没显示云母（看到黑底/白底/纯透明）：

1. **确认系统 Win11**（云母只支持 22000+）；
2. **确认背景类型**：`NativeMethods.SetMicaBackdrop` 里 `DWMSBT_MAINWINDOW`（=2）是云母；
3. **允许透明窗口承载**：`EnableHostBackdrop`（ACCENT_ENABLE_HOSTBACKDROP）是关键一步，
   缺了它透明窗口看不到云母；
4. 若仍是纯透明：把 `SetMicaBackdrop` 的 `DWMSBT_MAINWINDOW` 换成
   `DWMSBT_TRANSIENTWINDOW`（亚克力）验证背景机制是否正常，再切回云母；
5. 桌面主题是"纯色"而非"图片/幻灯片"时，云母会退化为几乎不透明——这是系统行为，正常。

> 最坏情况：云母不显示时，胶囊仍是半透明磨砂质感（Overlay 画刷 + 圆角），不会"黑屏"。

## 日间 / 夜间时间调整

改 `ThemeScheduler` 的 `DayStartHour` / `NightStartHour` 即可，例如 7:00–19:00：

```csharp
private readonly ThemeScheduler _themeScheduler = new()
{
    DayStartHour = 7,
    NightStartHour = 19,
};
```

## 下一步建议（按需加）

- 系统**自启**（注册表 Run 键或启动文件夹）
- 通知显示内容（标题/正文，当前只显示来源应用名）
- 媒体封面缩略图（`MediaSessionInfo.Thumbnail`）
- 播放/暂停控制按钮（SMTC `TryTogglePlayPauseAsync`）

## 与 WinUI 版（DynamicIsland.Win）的区别

| | WinUI 版 | 本版（WPF） |
|---|---|---|
| 框架 | WinUI 3 / WASDK 2.4.0 | WPF（无 WinUI） |
| 材质 | 亚克力 Acrylic | **云母 Mica** |
| 日间/夜间 | 无 | **按时间自动切换** |
| 媒体/通知/电量 | 有 | 有 |
| 全屏隐藏/多屏跟随 | 无 | **有** |
| 系统托盘 | 无 | **有** |
| 稳定性 | WASDK 版本敏感 | .NET/WPF API 十年稳定 |

---

*本项目在 Linux 环境生成，未经过 Windows 侧编译验证；首次编译若报错，把第一条 Error 贴回。*
