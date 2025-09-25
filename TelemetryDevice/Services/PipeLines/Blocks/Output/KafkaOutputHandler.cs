using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Services.Kafka.Producers;

namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public class KafkaOutputHandler : IOutputHandler
    {
        private readonly ITelemetryProducer _producer;
        private readonly ActionBlock<Dictionary<TelemetryFields, double>> _actionBlock;
        private readonly ICD _icd;

        public KafkaOutputHandler(ITelemetryProducer producer, ICD icd)
        {
            _producer = producer;
            _icd = icd;
            _actionBlock = new ActionBlock<Dictionary<TelemetryFields, double>>(decodedTelemetryData =>
            {
                HandleOutput(decodedTelemetryData, _icd);
            });
        }

        public void HandleOutput(
            Dictionary<TelemetryFields, double> decodedTelemetryData,
            ICD telemetryIcd
        )
        {
            decodedTelemetryData.TryGetValue(TelemetryFields.TailId, out var tailIdValue);

            var kafkaMessageKey = tailIdValue.ToString();
            var kafkaTopicName = $"{TelemetryDeviceConstants.Kafka.BASE_TOPIC_NAME}{(int)tailIdValue}";
            var kafkaPartition = _icd.Id;

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

        public Task Completion => _actionBlock.Completion;
        public void Complete() => _actionBlock.Complete();
        public void Fault(Exception exception) => ((IDataflowBlock)_actionBlock).Fault(exception);
        public bool Post(Dictionary<TelemetryFields, double> item) => _actionBlock.Post(item);
        public Task<bool> SendAsync(Dictionary<TelemetryFields, double> item, CancellationToken cancellationToken = default) => 
            _actionBlock.SendAsync(item, cancellationToken);
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, Dictionary<TelemetryFields, double> messageValue, ISourceBlock<Dictionary<TelemetryFields, double>> source, bool consumeToAccept) => 
            ((ITargetBlock<Dictionary<TelemetryFields, double>>)_actionBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
    }
}
