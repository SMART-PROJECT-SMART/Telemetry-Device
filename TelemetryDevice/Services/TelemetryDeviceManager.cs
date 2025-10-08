using Core.Models.ICDModels;
using Core.Services.ICDsDirectory;
using TelemetryDevices.Common;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Kafka.Topic_Manager;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.PortsManager;

namespace TelemetryDevices.Services
{
    public class TelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId;
        private readonly IICDDirectory _icdDirectory;
        private readonly IPortManager _portManager;
        private readonly IKafkaTopicManager _kafkaTopicManager;
        private readonly IServiceProvider _serviceProvider;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPortManager portManager,
            IKafkaTopicManager kafkaTopicManager,
            IServiceProvider serviceProvider
        )
        {
            _icdDirectory = icdDirectory;
            _portManager = portManager;
            _kafkaTopicManager = kafkaTopicManager;
            _serviceProvider = serviceProvider;
            _telemetryDevicesByTailId = new Dictionary<int, TelemetryDevice>();
        }

        public async Task AddTelemetryDeviceAsync(
            int tailId,
            List<int> portNumbers,
            Location location
        )
        {
            ValidateTelemetryDeviceDoesNotExist(tailId);
            TelemetryDevice newTelemetryDevice = new TelemetryDevice(location,tailId);
            _telemetryDevicesByTailId[tailId] = newTelemetryDevice;

            List<ICD> availableIcds = _icdDirectory.GetAllICDs();
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
                ICD currentTelemetryIcd = availableIcds[channelIndex];

                ITelemetryPipeLine telemetryPipeLine =
                    _serviceProvider.GetRequiredService<ITelemetryPipeLine>();

                Channel channel = new(portNumbers[channelIndex], telemetryPipeLine);
                channel.TelemetryPipeLine.BuildPipelineBlocks(currentTelemetryIcd);
                newTelemetryDevice.Channels.Add(channel);
                _portManager.AddPort(portNumbers[channelIndex], channel);
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            if (
                !_telemetryDevicesByTailId.TryGetValue(
                    tailId,
                    out TelemetryDevice? targetTelemetryDevice
                )
            )
            {
                return false;
            }

            foreach (Channel telemetryDeviceChannel in targetTelemetryDevice.Channels)
            {
                _portManager.RemovePort(telemetryDeviceChannel.PortNumber);
                telemetryDeviceChannel.TelemetryPipeLine.Dispose();
            }

            return _telemetryDevicesByTailId.Remove(tailId);
        }
    }
}
