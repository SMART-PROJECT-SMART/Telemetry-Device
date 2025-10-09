using Core.Common.Enums;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.Kafka.Producers.TelemetryDevicesStatusProducer
{
    public interface ITelemetryDeviceStatusProducer
    {
        public Task ProduceAsync(
            IEnumerable<TelemetryDevice> telemetryDevices);
    }
}
