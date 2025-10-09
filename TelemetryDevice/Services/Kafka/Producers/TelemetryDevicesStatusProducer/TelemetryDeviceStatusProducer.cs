using Confluent.Kafka;
using TelemetryDevices.Common;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.Kafka.Producers.TelemetryDevicesStatusProducer
{
    public class TelemetryDeviceStatusProducer : ITelemetryDeviceStatusProducer
    {
        private readonly IProducer<string, byte[]> _producer;

        public TelemetryDeviceStatusProducer(IProducer<string, byte[]> producer)
        {
            _producer = producer;
        }

        public Task ProduceAsync(IEnumerable<TelemetryDevice> telemetryDevices)
        {
            string fullStatusMessage = string.Join("\n", telemetryDevices.Select(td => td.GetStatus()));
            var kafkaMessage = new Message<string, byte[]>
            {
                Key = Guid.NewGuid().ToString(),
                Value = System.Text.Encoding.UTF8.GetBytes(fullStatusMessage),
            };
            return _producer.ProduceAsync(TelemetryDeviceConstants.Kafka.TELEMETRY_DEVICE_STATUS_TOPIC, kafkaMessage);
        }
    }
}
