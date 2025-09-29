using Core.Models.ICDModels;
using Core.Services.ICDsDirectory;
using TelemetryDevices.Common;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PortsManager;
using TelemetryDevices.Services.Kafka.Producers;
using TelemetryDevices.Services.Kafka.Topic_Manager;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;

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
        private readonly IValidator _validator;
        private readonly ITelemetryDecoder _telemetryDecoder;
        private readonly IOutputHandler _outputHandler;
        
        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPortManager portManager,
            ITelemetryProducer telemetryProducer,
            IKafkaTopicManager kafkaTopicManager,
            IValidator validator,
            ITelemetryDecoder telemetryDecoder,
            IOutputHandler outputHandler
        )
        {
            _icdDirectory = icdDirectory;
            _portManager = portManager;
            _telemetryProducer = telemetryProducer;
            _kafkaTopicManager = kafkaTopicManager;
            _validator = validator;
            _telemetryDecoder = telemetryDecoder;
            _outputHandler = outputHandler;
        }

        public async Task AddTelemetryDeviceAsync(int tailId, List<int> portNumbers, Location location)
        {
            ValidateTelemetryDeviceDoesNotExist(tailId);
            TelemetryDevice newTelemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = newTelemetryDevice;

            List<ICD> availableIcds = _icdDirectory.GetAllICDs().ToList();
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
                ICD currentTelemetryIcd = availableIcds[channelIndex];
                Channel channel = new Channel(
                    portNumbers[channelIndex],
                    currentTelemetryIcd,
                    _validator,
                    _telemetryDecoder,
                    _outputHandler
                );

                newTelemetryDevice.Channels.Add(channel);
                _portManager.AddPort(portNumbers[channelIndex], channel);
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            if (!_telemetryDevicesByTailId.TryGetValue(tailId, out TelemetryDevice? targetTelemetryDevice))
            {
                return false;
            }
            foreach (Channel telemetryDeviceChannel in targetTelemetryDevice.Channels)
            {
                _portManager.RemovePort(telemetryDeviceChannel.PortNumber);
            }
            return _telemetryDevicesByTailId.Remove(tailId);
        }
    }
}
