using Shared.Common.Enums;

namespace TelemetryDevices.Services.Kafka.Producers
{
    public interface ITelemetryProducer
    {
        public Task ProduceAsync(
            string topicName,
            string icdIdentifier,
            Dictionary<TelemetryFields, double> data
        );

        public Task ProduceAsync(
            string topicName,
            int partition,
            string messageKey,
            Dictionary<TelemetryFields, double> data
        );
    }
}
