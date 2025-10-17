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

        public TelemetryDeviceStatusJob(ITelemetryDeviceManager telemetryDeviceManager,ITelemetryDeviceStatusProducer telemetryDeviceStatusProducer)
        {
            _telemetryDeviceManager = telemetryDeviceManager;
            _telemetryDeviceStatusProducer = telemetryDeviceStatusProducer;
        }

        public Task Execute(IJobExecutionContext context)
        {
            IEnumerable<TelemetryDevice> telemetryDevices = _telemetryDeviceManager.GetAllTelemetryDevices();
            _telemetryDeviceStatusProducer.ProduceAsync(telemetryDevices);
            return Task.CompletedTask;
        }
    }
}
