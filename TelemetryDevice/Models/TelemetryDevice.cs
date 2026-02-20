using Core.Models;
using Core.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Models
{
    public class TelemetryDevice
    {
        public string SleeveName { get; set; }
        public int SleeveId { get; set; }
        public Location Location { get; set; }
        public Location? TransmittingUavLocation { get; set; }
        public List<Channel> Channels { get; set; }
        public int? TailId { get; set; }

        public TelemetryDevice(string sleeveName, int sleeveId, Location location, int? tailId = null)
        {
            SleeveName = sleeveName;
            SleeveId = sleeveId;
            Location = location;
            TransmittingUavLocation = null;
            Channels = new List<Channel>();
            TailId = tailId;
        }

        public string GetStatus()
        {
            return $"Telemetry Device With Tail Id {TailId} located at Lat: {Location.Latitude}, Lon: {Location.Longitude}";
        }
    }
}
