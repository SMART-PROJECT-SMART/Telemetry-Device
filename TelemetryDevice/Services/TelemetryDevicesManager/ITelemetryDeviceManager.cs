using Core.Models;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.TelemetryDevicesManager
{
    public interface ITelemetryDeviceManager
    {
        Task AddTelemetryDeviceAsync(string sleeveName, int sleeveId, int? tailId, IEnumerable<int> portNumbers, Location location);

        bool RemoveTelemetryDevice(int sleeveId);

        void UpdatePortsForSleeve(int sleeveId, IEnumerable<int> newPorts);

        IEnumerable<TelemetryDevice> GetAllTelemetryDevices();
    }
}
