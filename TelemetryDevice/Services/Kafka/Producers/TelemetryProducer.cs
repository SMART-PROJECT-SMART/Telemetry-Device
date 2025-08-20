using KafkaFlow;
using KafkaFlow.Producers;
using Shared.Common.Enums;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Kafka.Producers
{
    public class TelemetryProducer : ITelemetryProducer
    {
        private readonly IMessageProducer _messageProducer;

        public TelemetryProducer(IProducerAccessor producerAccessor)
        {
            _messageProducer = producerAccessor.GetProducer(TelemetryDeviceConstants.Kafka.PRODUCER_NAME) ?? throw new ArgumentNullException();
        }

        public async Task ProduceAsync(string topicName, string icdIdentifier, Dictionary<TelemetryFields, double> data)
        {
            await _messageProducer.ProduceAsync(
                topicName,
                icdIdentifier,
                data);
        }
    }
}
