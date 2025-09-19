using System.Text;
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

        public async Task ProduceAsync(
            string topicName,
            string tailIdKey,
            Dictionary<TelemetryFields, double> telemetryData
        )
        {
            var serializedTelemetryData = JsonConvert.SerializeObject(telemetryData);
            var kafkaMessage = new Message<string, byte[]>
            {
                Key = tailIdKey,
                Value = Encoding.UTF8.GetBytes(serializedTelemetryData),
            };

            await _producer.ProduceAsync(topicName, kafkaMessage).ConfigureAwait(false);
        }

        public async Task ProduceAsync(
            string topicName,
            int partition,
            string messageKey,
            Dictionary<TelemetryFields, double> telemetryData
        )
        {
            var serializedTelemetryData = JsonConvert.SerializeObject(telemetryData);
            var kafkaMessage = new Message<string, byte[]>
            {
                Key = messageKey,
                Value = Encoding.UTF8.GetBytes(serializedTelemetryData),
            };

            var topicPartition = new TopicPartition(topicName, new Partition(partition));
            await _producer.ProduceAsync(topicPartition, kafkaMessage).ConfigureAwait(false);
        }
    }
}
