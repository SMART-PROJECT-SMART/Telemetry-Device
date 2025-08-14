using TelemetryDevices.Models;

namespace TelemetryDevices.Dto
{
    public class CreateTelemetryDeviceDto
    {
        public int TailId { get; set; }
        public List<int> PortNumbers { get; set; }
        public Location Location { get; set; }
        public CreateTelemetryDeviceDto() { }
        public CreateTelemetryDeviceDto(int tailId, List<int> portNumbers, Location location)
        {
            TailId = tailId;
            PortNumbers = portNumbers;
            Location = location;
        }
    }
}
