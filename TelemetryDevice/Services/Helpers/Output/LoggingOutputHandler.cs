using Shared.Common.Enums;
using TelemetryDevices.Services.Builders;

namespace TelemetryDevices.Services.Helpers.Output
{
    public class LoggingOutputHandler : IOutputHandler
    {
        private readonly ILogger<LoggingOutputHandler> _logger;

        public LoggingOutputHandler(ILogger<LoggingOutputHandler> logger)
        {
            _logger = logger;
        }

        public void HandleOutput(Dictionary<TelemetryFields, double> decodedData)
        {
            _logger.LogInformation("Telemetry data processed successfully.");
        }
    }
}
