using Confluent.Kafka;
using TelemetryDevices.Common;
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

            string fullStatusMessage = string.Join(
                TelemetryDeviceConstants.TextHelpers.LINE_DOWN_SEPARATOR,
                telemetryDevices.Select(td => td.GetStatus())
            );
            var kafkaMessage = new Message<string, byte[]>
            {
                Value = System.Text.Encoding.UTF8.GetBytes(fullStatusMessage),
            };

            await _producer.ProduceAsync(
                TelemetryDeviceConstants.Kafka.TELEMETRY_DEVICE_STATUS_TOPIC,
                kafkaMessage
            );
        }
    }
}
