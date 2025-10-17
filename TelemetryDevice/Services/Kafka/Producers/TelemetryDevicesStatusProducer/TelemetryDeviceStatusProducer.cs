using Confluent.Kafka;
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

        public TelemetryDeviceStatusProducer(
            IProducer<string, byte[]> producer,
            IKafkaTopicManager kafkaTopicManager,
            IOptions<TelemetryDeviceStatusConfiguration> configuration)
        {
            _producer = producer;
            _kafkaTopicManager = kafkaTopicManager;
            _telemetryDeviceStatusConfiguration = configuration.Value;
        }

        public async Task ProduceAsync(IEnumerable<TelemetryDevice> telemetryDevices)
        {
            await _kafkaTopicManager.EnsureTopicExistsAsync(_telemetryDeviceStatusConfiguration.TopicName);

            List<TelemetryDeviceStatusDto> statusDtos = telemetryDevices
                .Select(td => new TelemetryDeviceStatusDto(td))
                .ToList();

            string jsonMessage = JsonConvert.SerializeObject(statusDtos);
            
            var kafkaMessage = new Message<string, byte[]>
            {
                Value = System.Text.Encoding.UTF8.GetBytes(jsonMessage),
            };

            await _producer.ProduceAsync(
                _telemetryDeviceStatusConfiguration.TopicName,
                kafkaMessage
            );
        }
    }
}
