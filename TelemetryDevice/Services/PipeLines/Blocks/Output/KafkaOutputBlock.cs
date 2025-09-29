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
        private readonly ActionBlock<DecodingResult> _actionBlock;
        private readonly ICD _icd;

        public KafkaOutputBlock(ITelemetryProducer producer, ICD icd)
        {
            _producer = producer;
            _icd = icd;
            _actionBlock = new ActionBlock<DecodingResult>(decodingResult =>
            {
                HandleOutput(decodingResult, _icd);
            });
        }

        public void HandleOutput(DecodingResult decodingResult, ICD telemetryIcd)
        {
            var tailIdValue = decodingResult.GetValue(TelemetryFields.TailId) ?? 0;

            var kafkaMessageKey = tailIdValue.ToString();
            var kafkaTopicName = $"{TelemetryDeviceConstants.Kafka.BASE_TOPIC_NAME}{(int)tailIdValue}";
            var kafkaPartition = _icd.Id;

            var decodedTelemetryData = decodingResult.ToDictionary();

            var produceTask = _producer.ProduceAsync(
                kafkaTopicName,
                kafkaPartition,
                kafkaMessageKey,
                decodedTelemetryData
            );

            produceTask.Wait(
                TimeSpan.FromSeconds(TelemetryDeviceConstants.Kafka.WAIT_TIMEOUT_SECONDS)
            );
        }


        public Task Completion => _actionBlock.Completion;
        public void Complete() => _actionBlock.Complete();
        public void Fault(Exception exception) => ((IDataflowBlock)_actionBlock).Fault(exception);
        public bool Post(DecodingResult item) => _actionBlock.Post(item);
        public Task<bool> SendAsync(DecodingResult item, CancellationToken cancellationToken = default) => 
            _actionBlock.SendAsync(item, cancellationToken);
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, DecodingResult messageValue, ISourceBlock<DecodingResult> source, bool consumeToAccept) => 
            ((ITargetBlock<DecodingResult>)_actionBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
    }
}
