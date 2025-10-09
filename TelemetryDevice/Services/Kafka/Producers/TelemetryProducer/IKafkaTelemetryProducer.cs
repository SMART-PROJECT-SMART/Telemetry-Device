using Core.Common.Enums;

namespace TelemetryDevices.Services.Kafka.Producers.TelemetryProducer
{
    public interface IKafkaTelemetryProducer
    {
        public Task ProduceAsync(
            string topicName,
            int partition,
            string messageKey,
            int tailId,
            IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData
        );
    }
}
