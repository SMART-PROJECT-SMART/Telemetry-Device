using Microsoft.Extensions.Logging;
using Quartz;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Kafka.Producers;
using TelemetryDevices.Services.Kafka.Producers.TelemetryDevicesStatusProducer;
using TelemetryDevices.Services.TelemetryDevicesManager;

namespace TelemetryDevices.Services.Quartz.Jobs
{
    public class TelemetryDeviceStatusJob : IJob
    {
        private readonly ITelemetryDeviceManager _telemetryDeviceManager;
        private readonly ITelemetryDeviceStatusProducer _telemetryDeviceStatusProducer;
        private readonly ILogger<TelemetryDeviceStatusJob> _logger;

        public TelemetryDeviceStatusJob(
            ITelemetryDeviceManager telemetryDeviceManager,
            ITelemetryDeviceStatusProducer telemetryDeviceStatusProducer,
            ILogger<TelemetryDeviceStatusJob> logger)
        {
            _telemetryDeviceManager = telemetryDeviceManager;
            _telemetryDeviceStatusProducer = telemetryDeviceStatusProducer;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            IEnumerable<TelemetryDevice> telemetryDevices = _telemetryDeviceManager.GetAllTelemetryDevices();
            int count = telemetryDevices.Count();
            _logger.LogInformation("TelemetryDeviceStatusJob executed, producing {DeviceCount} devices to Kafka", count);
            await _telemetryDeviceStatusProducer.ProduceAsync(telemetryDevices);
        }
    }
}
