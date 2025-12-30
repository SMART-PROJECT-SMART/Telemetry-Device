using Core.Models;

namespace TelemetryDevices.Dto
{
    public class CreateTelemetryDeviceDto
    {
        public int TailId { get; set; }
        public IEnumerable<int> PortNumbers { get; set; }
        public Location Location { get; set; }

        public CreateTelemetryDeviceDto() { }

        public CreateTelemetryDeviceDto(int tailId, IEnumerable<int> portNumbers, Location location)
        {
            TailId = tailId;
            PortNumbers = portNumbers;
            Location = location;
        }
    }
}
