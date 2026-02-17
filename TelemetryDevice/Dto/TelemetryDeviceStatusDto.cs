using Core.Models;
using TelemetryDevices.Models;

namespace TelemetryDevices.Dto
{
    public class TelemetryDeviceStatusDto
    {
        public int? TailId { get; set; }
        public Location? Location { get; set; }

        public TelemetryDeviceStatusDto() { }

        public TelemetryDeviceStatusDto(TelemetryDevice telemetryDevice)
        {
            TailId = telemetryDevice.TailId;
            Location = telemetryDevice.TransmittingUavLocation;
        }
    }
}
