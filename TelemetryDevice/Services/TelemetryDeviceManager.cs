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
            var newTelemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = newTelemetryDevice;

            var availableIcds = _icdDirectory.GetAllICDs().ToList();
            CreateChannelsForDevice(newTelemetryDevice, portNumbers, availableIcds);
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
            TelemetryDevice newTelemetryDevice,
            List<int> portNumbers,
            List<ICD> availableIcds
        )
        {
            for (int channelIndex = 0; channelIndex < availableIcds.Count && channelIndex < portNumbers.Count; channelIndex++)
            {
                var currentIcd = availableIcds[channelIndex];
                IPipeLine telemetryPipeline = _pipeLineFactory.GetPipeLine(currentIcd);
                newTelemetryDevice.AddChannel(portNumbers[channelIndex], telemetryPipeline, currentIcd);

                var createdChannel = newTelemetryDevice.Channels.FirstOrDefault(c =>
                    c.PortNumber == portNumbers[channelIndex]
                );
                if (createdChannel != null)
                {
                    _portManager.AddPort(portNumbers[channelIndex], createdChannel);
                }
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            if (!_telemetryDevicesByTailId.TryGetValue(tailId, out var targetDevice))
            {
                return false;
            }
            foreach (var deviceChannel in targetDevice.Channels)
            {
                _portManager.RemovePort(deviceChannel.PortNumber);
            }
            return _telemetryDevicesByTailId.Remove(tailId);
        }
    }
}
