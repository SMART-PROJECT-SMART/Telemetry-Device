using Shared.Services.ICDDirectory;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services
{
    public class TelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId = new Dictionary<int, TelemetryDevice>();
        private readonly IICDDirectory _icdDirectory;

        public TelemetryDeviceManager(IICDDirectory icdDirectory)
        {
            _icdDirectory = icdDirectory;
        }

        public void AddTelemetryDevice(int tailId, List<int> portNumbers, Location location)
        {
            if (_telemetryDevicesByTailId.ContainsKey(tailId))
            {
                throw new ArgumentException($"Telemetry device with tail ID {tailId} already exists.");
            }

            var telemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = telemetryDevice;

            var icds = _icdDirectory.GetAllICDs().ToList();
            for (int index = 0; index < icds.Count; index++)
            {
                telemetryDevice.AddChannel(portNumbers[index], icds[index]);
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            return _telemetryDevicesByTailId.Remove(tailId);
        }

        public bool Exists(int tailId)
        {
            return _telemetryDevicesByTailId.ContainsKey(tailId);
        }
    }
}