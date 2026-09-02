using System;
using Windows.Devices.Power;

namespace DynamicIsland.Hyper.Services;

/// <summary>电量实时活动：监听系统电量变化，上报百分比。</summary>
internal sealed class BatteryService : IDisposable
{
    /// <summary>电量百分比变化（0~100）。</summary>
    public event Action<double>? ChargePercentChanged;

    public void Start()
    {
        Battery.AggregateBattery.ReportUpdated += OnReportUpdated;
        Raise();
    }

    private void OnReportUpdated(Battery sender, object args) => Raise();

    private void Raise()
    {
        var report = Battery.AggregateBattery.GetReport();
        if (report.FullChargeCapacityInMilliwattHours is int full && full > 0 &&
            report.RemainingCapacityInMilliwattHours is int remaining)
        {
            ChargePercentChanged?.Invoke(Math.Round(remaining / (double)full * 100.0, 1));
        }
    }

    public void Dispose() => Battery.AggregateBattery.ReportUpdated -= OnReportUpdated;
}
