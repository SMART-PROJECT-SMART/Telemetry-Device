using Quartz;
using TelemetryDevices.Services.Kafka.Producers;
using TelemetryDevices.Services.TelemetryDevicesManager;

namespace TelemetryDevices.Services.Quartz.Jobs
{
    public class TelemetryDeviceStatusJob : IJob
    {
        private readonly ITelemetryDeviceManager _telemetryDeviceManager;
        public Task Execute(IJobExecutionContext context)
        {
            
        }
    }
}
