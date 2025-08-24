using Confluent.Kafka;
using Newtonsoft.Json;
using Shared.Common.Enums;

namespace TelemetryDevices.Services.Kafka.Producers
{
    public class TelemetryProducer : ITelemetryProducer
    {
        private readonly IProducer<string, byte[]> _producer;

        public TelemetryProducer(ProducerConfig producerConfig)
        {
            _producer = new ProducerBuilder<string, byte[]>(producerConfig).Build();
        }

        public async Task ProduceAsync(string topicName, string tailIdKey, Dictionary<TelemetryFields, double> telemetryData)
        {
            var serialized = JsonConvert.SerializeObject(telemetryData);
            var message = new Message<string, byte[]>
            {
                Key = tailIdKey,
                Value = System.Text.Encoding.UTF8.GetBytes(serialized)
            };

            await _producer.ProduceAsync(topicName, message).ConfigureAwait(false);
        }
    }
}