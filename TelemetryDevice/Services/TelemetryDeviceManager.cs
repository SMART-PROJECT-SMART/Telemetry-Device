using Shared.Common.Enums;
using Shared.Models.ICDModels;
using Shared.Services.ICDsDirectory;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines.Director;
using TelemetryDevices.Services.PortsManager;

namespace TelemetryDevices.Services
{
    public class TelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId =
            new Dictionary<int, TelemetryDevice>();
        private readonly IICDDirectory _icdDirectory;
        private readonly IPipeLineDirector _telemetryPipelineDirector;
        private readonly IPortManager _portManager;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPipeLineDirector telemetryPipelineDirector,
            IPortManager portManager
        )
        {
            _icdDirectory = icdDirectory;
            _telemetryPipelineDirector = telemetryPipelineDirector;
            _portManager = portManager;
        }

        public void AddTelemetryDevice(int tailId, List<int> portNumbers, Location location)
        {
            ValidateTelemetryDeviceDoesNotExist(tailId);
            var newTelemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = newTelemetryDevice;

            var availableIcds = _icdDirectory.GetAllICDs().ToList();
            CreateTelemetryChannelsForDevice(newTelemetryDevice, portNumbers, availableIcds);
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

        private void CreateTelemetryChannelsForDevice(
            TelemetryDevice newTelemetryDevice,
            List<int> portNumbers,
            List<ICD> availableIcds
        )
        {
            for (
                int channelIndex = 0;
                channelIndex < availableIcds.Count && channelIndex < portNumbers.Count;
                channelIndex++
            )
            {
                var currentTelemetryIcd = availableIcds[channelIndex];
                var telemetryPipeline = _telemetryPipelineDirector.CreateStandardTelemetryPipeline(currentTelemetryIcd);
                newTelemetryDevice.AddChannel(
                    portNumbers[channelIndex],
                    telemetryPipeline,
                    currentTelemetryIcd
                );

                var createdTelemetryChannel = newTelemetryDevice.Channels.FirstOrDefault(c =>
                    c.PortNumber == portNumbers[channelIndex]
                );
                if (createdTelemetryChannel != null)
                {
                    _portManager.AddPort(portNumbers[channelIndex], createdTelemetryChannel);
                }
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            if (!_telemetryDevicesByTailId.TryGetValue(tailId, out var targetTelemetryDevice))
            {
                return false;
            }
            foreach (var telemetryDeviceChannel in targetTelemetryDevice.Channels)
            {
                _portManager.RemovePort(telemetryDeviceChannel.PortNumber);
            }
            return _telemetryDevicesByTailId.Remove(tailId);
        }
    }
}
