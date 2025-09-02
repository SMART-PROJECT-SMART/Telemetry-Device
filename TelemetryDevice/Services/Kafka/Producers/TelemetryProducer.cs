using Confluent.Kafka;
using Newtonsoft.Json;
using Shared.Common.Enums;
using System.Text;

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
            var serializedTelemetryData = JsonConvert.SerializeObject(telemetryData);
            var kafkaMessage = new Message<string, byte[]>
            {
                Key = tailIdKey,
                Value = Encoding.UTF8.GetBytes(serializedTelemetryData)
            };

            await _producer.ProduceAsync(topicName, kafkaMessage).ConfigureAwait(false);
        }
    }
}