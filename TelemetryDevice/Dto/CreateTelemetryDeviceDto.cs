using Core.Models;

namespace TelemetryDevices.Dto
{
    public class CreateTelemetryDeviceDto
    {
        public string SleeveName { get; set; }
        public int SleeveId { get; set; }
        public int? TailId { get; set; }
        public IEnumerable<int> PortNumbers { get; set; }
        public Location Location { get; set; }

        public CreateTelemetryDeviceDto() { }

        public CreateTelemetryDeviceDto(string sleeveName, int sleeveId, int? tailId, IEnumerable<int> portNumbers, Location location)
        {
            SleeveName = sleeveName;
            SleeveId = sleeveId;
            TailId = tailId;
            PortNumbers = portNumbers;
            Location = location;
        }
    }
}
