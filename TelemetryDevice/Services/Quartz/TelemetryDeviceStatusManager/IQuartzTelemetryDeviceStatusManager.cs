namespace TelemetryDevices.Services.Quartz.TelemetryDeviceStatusManager
{
    public interface IQuartzTelemetryDeviceStatusManager
    {
        Task<bool> StartSchedular(int intervalSeconds);
        Task<bool> StopSchedular();
    }
}
