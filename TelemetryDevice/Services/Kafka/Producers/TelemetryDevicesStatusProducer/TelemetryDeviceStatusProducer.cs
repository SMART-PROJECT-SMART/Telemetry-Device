using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TelemetryDevices.Config;
using TelemetryDevices.Dto;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Kafka.Topic_Manager;

namespace TelemetryDevices.Services.Kafka.Producers.TelemetryDevicesStatusProducer
{
    public class TelemetryDeviceStatusProducer : ITelemetryDeviceStatusProducer
    {
        private readonly IProducer<string, byte[]> _producer;
        private readonly IKafkaTopicManager _kafkaTopicManager;
        private readonly TelemetryDeviceStatusConfiguration _telemetryDeviceStatusConfiguration;
        private readonly ILogger<TelemetryDeviceStatusProducer> _logger;

        public TelemetryDeviceStatusProducer(
            IProducer<string, byte[]> producer,
            IKafkaTopicManager kafkaTopicManager,
            IOptions<TelemetryDeviceStatusConfiguration> configuration,
            ILogger<TelemetryDeviceStatusProducer> logger)
        {
            _producer = producer;
            _kafkaTopicManager = kafkaTopicManager;
            _telemetryDeviceStatusConfiguration = configuration.Value;
            _logger = logger;
        }

        public async Task ProduceAsync(IEnumerable<TelemetryDevice> telemetryDevices)
        {
            await _kafkaTopicManager.EnsureTopicExistsAsync(_telemetryDeviceStatusConfiguration.TopicName);

            List<TelemetryDeviceStatusDto> statusDtos = telemetryDevices
                .Where(td => td.TailId.HasValue)
                .Select(td => new TelemetryDeviceStatusDto(td))
                .ToList();

            string jsonMessage = JsonConvert.SerializeObject(statusDtos);

            Message<string, byte[]> kafkaMessage = new Message<string, byte[]>
            {
                Value = System.Text.Encoding.UTF8.GetBytes(jsonMessage),
            };

            await _producer.ProduceAsync(
                _telemetryDeviceStatusConfiguration.TopicName,
                kafkaMessage
            );
            _logger.LogInformation("Produced {DeviceCount} device status(es) to topic {TopicName}", statusDtos.Count, _telemetryDeviceStatusConfiguration.TopicName);
        }
    }
}
