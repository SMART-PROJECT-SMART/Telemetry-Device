using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Services.Kafka.Producers;

namespace TelemetryDevices.Services.Helpers.Output
{
    public class KafkaOutputHandler : IOutputHandler
    {
        private readonly ITelemetryProducer _producer;
        private readonly ILogger<KafkaOutputHandler> _logger;
        private  int Amount = 0;

        public KafkaOutputHandler(ITelemetryProducer producer, ILogger<KafkaOutputHandler> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        public void HandleOutput(Dictionary<TelemetryFields, double> decodedData, ICD icd)
        {
            _logger.LogInformation($"Produced to {Amount++}");
            _producer.ProduceAsync(decodedData[TelemetryFields.TailId].ToString(), icd.GetHashCode().ToString(), decodedData);
            _logger.LogInformation($"Produced to {decodedData[TelemetryFields.TailId]}");
        }
    }
}
