using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Kafka.Producers;

namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public class KafkaOutputBlock : IOutputBlock
    {
        private readonly ITelemetryProducer _producer;

        public KafkaOutputBlock(ITelemetryProducer producer)
        {
            _producer = producer;
        }

        public void HandleOutput(DecodingResult decodingResult, ICD telemetryIcd)
        {
            var tailIdValue = decodingResult.GetValue(TelemetryFields.TailId);

            var kafkaMessageKey = tailIdValue.ToString();
            var kafkaTopicName = $"{TelemetryDeviceConstants.Kafka.BASE_TOPIC_NAME}{(int)tailIdValue}";
            var kafkaPartition = telemetryIcd.Id;

            var decodedTelemetryData = decodingResult.ToDictionary();

            _ = _producer.ProduceAsync(
                kafkaTopicName,
                kafkaPartition,
                kafkaMessageKey,
                decodedTelemetryData
            );
        }
    }
}
