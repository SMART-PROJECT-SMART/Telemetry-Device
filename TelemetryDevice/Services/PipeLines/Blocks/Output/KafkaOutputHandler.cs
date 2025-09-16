using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Services.Kafka.Producers;
using TelemetryDevices.Common;

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
                var kafkaTopicName = telemetryIcd?.ToString() ?? "unknown-icd";
                
                var produceTask = _producer.ProduceAsync(kafkaTopicName, kafkaMessageKey, decodedTelemetryData);
                
                produceTask.Wait(TimeSpan.FromSeconds(TelemetryDeviceConstants.Kafka.PRODUCE_TIMEOUT_SECONDS));
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
