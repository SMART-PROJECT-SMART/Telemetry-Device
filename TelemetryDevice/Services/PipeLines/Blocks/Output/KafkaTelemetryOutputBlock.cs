using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Kafka.Producers;

namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public class KafkaTelemetryOutputBlock : ITelemetryOutputBlock
    {
        private readonly IKafkaTelemetryProducer _kafkaTelemetryProducer;

        public KafkaTelemetryOutputBlock(IKafkaTelemetryProducer kafkaTelemetryProducer)
        {
            _kafkaTelemetryProducer = kafkaTelemetryProducer;
        }

        public void OutputTelemetryData(DecodingResult decodingResult, ICD telemetryIcd)
        {
            double? tailIdValue = decodingResult.GetValue(TelemetryFields.TailId);

            string kafkaMessageKey = tailIdValue.ToString()!;
            string kafkaTopicName =
                $"{TelemetryDeviceConstants.Kafka.BASE_TOPIC_NAME}{(int)tailIdValue!}";
            int kafkaPartition = telemetryIcd.Id;

            Dictionary<TelemetryFields, double> decodedTelemetryData =
                decodingResult.ToDictionary();

            _ = _kafkaTelemetryProducer.ProduceAsync(
                kafkaTopicName,
                kafkaPartition,
                kafkaMessageKey,
                decodedTelemetryData
            );
        }
    }
}
