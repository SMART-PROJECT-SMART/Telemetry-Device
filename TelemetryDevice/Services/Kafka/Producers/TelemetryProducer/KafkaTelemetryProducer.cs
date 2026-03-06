using Confluent.Kafka;
using Core.Common.Enums;
using Newtonsoft.Json;
using TelemetryDevices.Common;
using TelemetryDevices.Services.Kafka.Topic_Manager;

namespace TelemetryDevices.Services.Kafka.Producers.TelemetryProducer
{
    public class KafkaTelemetryProducer : IKafkaTelemetryProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly IKafkaTopicManager _kafkaTopicManager;

        public KafkaTelemetryProducer(
            ProducerConfig producerConfig,
            IKafkaTopicManager kafkaTopicManager
        )
        {
            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetValueSerializer(Serializers.Utf8)
                .Build();
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
            var kafkaMessage = new Message<string, string>
            {
                Key = messageKey,
                Value = serializedTelemetryData,
            };

            var topicPartition = new TopicPartition(topicName, new Partition(partition));
            await _producer.ProduceAsync(topicPartition, kafkaMessage).ConfigureAwait(false);
        }
    }
}
