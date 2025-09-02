using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Services.Kafka.Producers;


namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public class KafkaOutputHandler : IOutputHandler
    {
        private readonly ITelemetryProducer _producer;
        private readonly ILogger<KafkaOutputHandler> _logger;


        public KafkaOutputHandler(ITelemetryProducer producer, ILogger<KafkaOutputHandler> logger)
        {
            _producer = producer;
            _logger = logger;
        }


        public void HandleOutput(Dictionary<TelemetryFields, double> decodedTelemetryData, ICD telemetryIcd)
        {
            decodedTelemetryData.TryGetValue(TelemetryFields.TailId, out var tailIdValue);

            var kafkaMessageKey = tailIdValue.ToString();
            var kafkaTopicName = telemetryIcd?.ToString() ?? "unknown-icd";

            _ = _producer.ProduceAsync(kafkaTopicName, kafkaMessageKey, decodedTelemetryData);
        }
    }
}