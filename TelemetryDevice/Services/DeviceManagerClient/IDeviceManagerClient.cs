using TelemetryDevices.Dto.DeviceManager;

namespace TelemetryDevices.Services.DeviceManagerClient
{
    public interface IDeviceManagerClient
    {
        Task<IEnumerable<DeviceManagerSleeveDto>> GetAllSleevesAsync(CancellationToken cancellationToken = default);
    }
}
