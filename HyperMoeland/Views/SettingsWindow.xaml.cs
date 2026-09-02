using System;
using System.Globalization;
using System.Windows;
using HyperMoeland.Models;
using HyperMoeland.Services;
using HyperMoeland.Theme;

namespace HyperMoeland.Views;

/// <summary>设置窗口：主题模式 / 日夜间 / 自启 / 自动更新。</summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Populate();
    }

    private void Populate()
    {
        var s = SettingsService.Current;

        ThemeModeBox.SelectedIndex = s.ThemeMode switch
        {
            ThemePreference.Light => 1,
            ThemePreference.Dark => 2,
            _ => 0,
        };

        for (int i = 0; i < 24; i++)
        {
            DayStartBox.Items.Add($"{i:00}:00");
            NightStartBox.Items.Add($"{i:00}:00");
        }
        DayStartBox.SelectedIndex = Math.Clamp(s.DayStartHour, 0, 23);
        NightStartBox.SelectedIndex = Math.Clamp(s.NightStartHour, 0, 23);

        AutoStartCheck.IsChecked = s.AutoStart;
        AutoUpdateCheck.IsChecked = s.AutoUpdate;
        NeonSpeedSlider.Value = Math.Clamp(s.NeonSpeedMs, 400, 2000);
        NeonSpeedLabel.Text = s.NeonSpeedMs + "ms";
    }

    private void OnNeonSpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        => NeonSpeedLabel.Text = ((int)e.NewValue) + "ms";

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var s = SettingsService.Current;
        s.ThemeMode = ThemeModeBox.SelectedIndex switch
        {
            1 => ThemePreference.Light,
            2 => ThemePreference.Dark,
            _ => ThemePreference.Auto,
        };
        s.DayStartHour = Math.Clamp(DayStartBox.SelectedIndex, 0, 23);
        s.NightStartHour = Math.Clamp(NightStartBox.SelectedIndex, 0, 23);
        s.AutoStart = AutoStartCheck.IsChecked == true;
        s.AutoUpdate = AutoUpdateCheck.IsChecked == true;
        s.NeonSpeedMs = (int)NeonSpeedSlider.Value;

        SettingsService.Save();
        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
