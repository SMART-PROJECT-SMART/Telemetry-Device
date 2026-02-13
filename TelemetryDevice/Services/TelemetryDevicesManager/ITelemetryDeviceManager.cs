using Core.Models;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.TelemetryDevicesManager
{
    public interface ITelemetryDeviceManager
    {
        Task AddTelemetryDeviceAsync(string sleeveName, int? tailId, IEnumerable<int> portNumbers, Location location);

        bool RemoveTelemetryDevice(string sleeveName);

        void UpdatePortsForSleeve(string sleeveName, IEnumerable<int> newPorts);

        IEnumerable<TelemetryDevice> GetAllTelemetryDevices();
    }
}
