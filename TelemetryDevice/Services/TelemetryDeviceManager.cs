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
        private readonly IKafkaTopicManager _kafkaTopicManager;
        private readonly IValidatorBlock _validatorBlock;
        private readonly ITelemetryDecoderBlock _telemetryDecoderBlock;
        private readonly IOutputBlock _outputBlock;
        
        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPortManager portManager,
            IKafkaTopicManager kafkaTopicManager,
            IValidatorBlock validatorBlock,
            ITelemetryDecoderBlock telemetryDecoderBlock,
            IOutputBlock outputBlock
        )
        {
            _icdDirectory = icdDirectory;
            _portManager = portManager;
            _kafkaTopicManager = kafkaTopicManager;
            _validatorBlock = validatorBlock;
            _telemetryDecoderBlock = telemetryDecoderBlock;
            _outputBlock = outputBlock;
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
                    _validatorBlock,
                    _telemetryDecoderBlock,
                    _outputBlock
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
