using System.Text;
using Confluent.Kafka;
using Core.Common.Enums;
using Newtonsoft.Json;
using TelemetryDevices.Common;
using TelemetryDevices.Services.Kafka.Topic_Manager;

namespace TelemetryDevices.Services.Kafka.Producers.TelemetryProducer
{
    public class KafkaTelemetryProducer : IKafkaTelemetryProducer
    {
        private readonly IProducer<string, byte[]> _producer;
        private readonly IKafkaTopicManager _kafkaTopicManager;

        public KafkaTelemetryProducer(
            ProducerConfig producerConfig,
            IKafkaTopicManager kafkaTopicManager
        )
        {
            _producer = new ProducerBuilder<string, byte[]>(producerConfig).Build();
            _kafkaTopicManager = kafkaTopicManager;
        }

        public async Task ProduceAsync(
            string topicName,
            int partition,
            string messageKey,
            int tailId,
            IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData
        )
        {
            await _kafkaTopicManager.EnsureTopicExistsAsync(
                $"{TelemetryDeviceConstants.Kafka.BASE_TOPIC_NAME}{tailId}"
            );

            string serializedTelemetryData = JsonConvert.SerializeObject(telemetryData);
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
