using Confluent.Kafka;
using Newtonsoft.Json;
using TelemetryDevices.Common;
using TelemetryDevices.Dto;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Kafka.Topic_Manager;

namespace TelemetryDevices.Services.Kafka.Producers.TelemetryDevicesStatusProducer
{
    public class TelemetryDeviceStatusProducer : ITelemetryDeviceStatusProducer
    {
        private readonly IProducer<string, byte[]> _producer;
        private readonly IKafkaTopicManager _kafkaTopicManager;

        public TelemetryDeviceStatusProducer(IProducer<string, byte[]> producer,IKafkaTopicManager kafkaTopicManager)
        {
            _producer = producer;
            _kafkaTopicManager = kafkaTopicManager;
        }

        public async Task ProduceAsync(IEnumerable<TelemetryDevice> telemetryDevices)
        {
            await _kafkaTopicManager.EnsureTopicExistsAsync(TelemetryDeviceConstants.Kafka.TELEMETRY_DEVICE_STATUS_TOPIC);

            List<TelemetryDeviceStatusDto> statusDtos = telemetryDevices
                .Select(td => new TelemetryDeviceStatusDto(td))
                .ToList();

            string jsonMessage = JsonConvert.SerializeObject(statusDtos);
            
            var kafkaMessage = new Message<string, byte[]>
            {
                Value = System.Text.Encoding.UTF8.GetBytes(jsonMessage),
            };

            await _producer.ProduceAsync(
                TelemetryDeviceConstants.Kafka.TELEMETRY_DEVICE_STATUS_TOPIC,
                kafkaMessage
            );
        }
    }
}
