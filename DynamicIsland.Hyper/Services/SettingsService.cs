using System;
using System.IO;
using System.Text.Json;
using DynamicIsland.Hyper.Models;

namespace DynamicIsland.Hyper.Services;

/// <summary>设置读写：加载/保存到 %LOCALAPPDATA%\DynamicIsland.Hyper\settings.json。</summary>
internal static class SettingsService
{
    public static AppSettings Current { get; private set; } = new();

    private static string FilePath
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DynamicIsland.Hyper", "settings.json");

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var s = JsonSerializer.Deserialize<AppSettings>(json);
                if (s is not null) Current = s;
            }
        }
        catch { }
    }

    public static void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Current,
                new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
