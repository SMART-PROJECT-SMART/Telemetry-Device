using Core.Models;
using TelemetryDevices.Models;

namespace TelemetryDevices.Dto
{
    public class TelemetryDeviceStatusDto
    {
        public int SleeveId { get; set; }
        public int TailId { get; set; }
        public Location Location { get; set; }

        public TelemetryDeviceStatusDto() { }

        public TelemetryDeviceStatusDto(TelemetryDevice telemetryDevice)
        {
            SleeveId = telemetryDevice.SleeveId;
            TailId = telemetryDevice.TailId!.Value;
            Location = telemetryDevice.TransmittingUavLocation ?? telemetryDevice.Location;
        }
    }
}
