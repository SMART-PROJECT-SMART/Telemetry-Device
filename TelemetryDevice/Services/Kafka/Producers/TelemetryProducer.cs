using System.Text;
using Confluent.Kafka;
using Newtonsoft.Json;
using Core.Common.Enums;
using TelemetryDevices.Services.Kafka.Topic_Manager;

namespace TelemetryDevices.Services.Kafka.Producers
{
    public class TelemetryProducer : ITelemetryProducer
    {
        private readonly IProducer<string, byte[]> _producer;
        private readonly IKafkaTopicManager _kafkaTopicManager;

        public TelemetryProducer(ProducerConfig producerConfig,IKafkaTopicManager kafkaTopicManager)
        {
            _producer = new ProducerBuilder<string, byte[]>(producerConfig).Build();
            _kafkaTopicManager = kafkaTopicManager;
        }

        public async Task ProduceAsync(
            string topicName,
            int partition,
            string messageKey,
            Dictionary<TelemetryFields, double> telemetryData
        )
        {
            await _kafkaTopicManager.EnsureTopicExistsAsync(topicName);
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
