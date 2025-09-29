using Core.Models.ICDModels;
using Core.Services.ICDsDirectory;
using TelemetryDevices.Common;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PortsManager;
using TelemetryDevices.Services.Kafka.Producers;
using TelemetryDevices.Services.Kafka.Topic_Manager;

namespace TelemetryDevices.Services
{
    public class TelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId =
            new Dictionary<int, TelemetryDevice>();
        private readonly IICDDirectory _icdDirectory;
        private readonly IPortManager _portManager;
        private readonly ITelemetryProducer _telemetryProducer;
        private readonly IKafkaTopicManager _kafkaTopicManager; 
        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPortManager portManager,
            ITelemetryProducer telemetryProducer,
            IKafkaTopicManager kafkaTopicManager
        )
        {
            _icdDirectory = icdDirectory;
            _portManager = portManager;
            _telemetryProducer = telemetryProducer;
            _kafkaTopicManager = kafkaTopicManager;
        }

        public async Task AddTelemetryDeviceAsync(int tailId, List<int> portNumbers, Location location)
        {
            ValidateTelemetryDeviceDoesNotExist(tailId);
            var newTelemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = newTelemetryDevice;

            var availableIcds = _icdDirectory.GetAllICDs().ToList();
            CreateTelemetryChannelsForDevice(newTelemetryDevice, portNumbers, availableIcds);
            await _kafkaTopicManager.EnsureTopicExistsAsync(
                $"{TelemetryDeviceConstants.Kafka.BASE_TOPIC_NAME}{tailId}"
            );
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
                var channel = new Channel(
                    portNumbers[channelIndex],
                    currentTelemetryIcd,
                    _telemetryProducer
                );

                newTelemetryDevice.Channels.Add(channel);
                _portManager.AddPort(portNumbers[channelIndex], channel);
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
