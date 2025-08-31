using Shared.Common.Enums;
using Shared.Models.ICDModels;
using Shared.Services.ICDsDirectory;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Factories.PipeLineFactory;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.PortsManager;

namespace TelemetryDevices.Services
{
    public class TelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId =
            new Dictionary<int, TelemetryDevice>();
        private readonly IICDDirectory _icdDirectory;
        private readonly IPipeLineFactory _pipeLineFactory;
        private readonly IPortManager _portManager;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPipeLineFactory pipeLineFactory,
            IPortManager portManager
        )
        {
            _icdDirectory = icdDirectory;
            _pipeLineFactory = pipeLineFactory;
            _portManager = portManager;
        }

        public void AddTelemetryDevice(int tailId, List<int> portNumbers, Location location)
        {
            ValidateTelemetryDeviceDoesNotExist(tailId);
            var telemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = telemetryDevice;

            var icds = _icdDirectory.GetAllICDs().ToList();
            CreateChannelsForDevice(telemetryDevice, portNumbers, icds);
        }

        private void ValidateTelemetryDeviceDoesNotExist(int tailId)
        {
            if (_telemetryDevicesByTailId.ContainsKey(tailId))
            {
                throw new ArgumentException(
                    $"Telemetry device with tail ID {tailId} already exists."
                );
            }
        }

        private void CreateChannelsForDevice(
            TelemetryDevice telemetryDevice,
            List<int> portNumbers,
            List<ICD> icds
        )
        {
            for (int index = 0; index < icds.Count && index < portNumbers.Count; index++)
            {
                var icd = icds[index];
                IPipeLine pipeline = _pipeLineFactory.GetPipeLine(icd);
                telemetryDevice.AddChannel(portNumbers[index], pipeline, icd);

                var channel = telemetryDevice.Channels.FirstOrDefault(c =>
                    c.PortNumber == portNumbers[index]
                );
                if (channel != null)
                {
                    _portManager.AddPort(portNumbers[index], channel);
                }
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            if (!_telemetryDevicesByTailId.TryGetValue(tailId, out var device))
            {
                return false;
            }
            foreach (var channel in device.Channels)
            {
                _portManager.RemovePort(channel.PortNumber);
            }
            return _telemetryDevicesByTailId.Remove(tailId);
        }
    }
}
