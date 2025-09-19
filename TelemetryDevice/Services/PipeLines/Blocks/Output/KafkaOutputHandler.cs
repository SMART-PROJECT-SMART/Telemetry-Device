using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Services.Kafka.Producers;

namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public class KafkaOutputHandler : IOutputHandler
    {
        private readonly ITelemetryProducer _producer;

        public KafkaOutputHandler(ITelemetryProducer producer)
        {
            _producer = producer;
        }

        public void HandleOutput(
            Dictionary<TelemetryFields, double> decodedTelemetryData,
            ICD telemetryIcd
        )
        {
            decodedTelemetryData.TryGetValue(TelemetryFields.TailId, out var tailIdValue);

            var kafkaMessageKey = tailIdValue.ToString();
            var kafkaTopicName = $"{TelemetryDeviceConstants.Kafka.BASE_TOPIC_NAME}{(int)tailIdValue}";
            var kafkaPartition = Math.Abs((telemetryIcd?.FileName ?? "unknown-icd").GetHashCode());

            var produceTask = _producer.ProduceAsync(
                kafkaTopicName,
                kafkaPartition,
                kafkaMessageKey,
                decodedTelemetryData
            );

            produceTask.Wait(
                TimeSpan.FromSeconds(TelemetryDeviceConstants.Kafka.PRODUCE_TIMEOUT_SECONDS)
            );
        }

        public ActionBlock<Dictionary<TelemetryFields, double>> GetBlock(ICD icd)
        {
            return new ActionBlock<Dictionary<TelemetryFields, double>>(decodedTelemetryData =>
            {
                HandleOutput(decodedTelemetryData, icd);
            });
        }
    }
}
