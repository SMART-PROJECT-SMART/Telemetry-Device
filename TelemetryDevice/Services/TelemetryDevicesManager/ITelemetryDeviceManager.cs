using TelemetryDevices.Models;

namespace TelemetryDevices.Services.TelemetryDevicesManager
{
    public interface ITelemetryDeviceManager
    {
        public Task AddTelemetryDeviceAsync(int tailId, List<int> portNumbers, Location location);

        public bool RemoveTelemetryDevice(int tailId);

        public IEnumerable<TelemetryDevice> GetAllTelemetryDevices();
    }
}
