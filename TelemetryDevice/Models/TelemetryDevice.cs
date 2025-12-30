using Core.Models;
using Core.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Models
{
    public class TelemetryDevice
    {
        public Location Location { get; set; }
        public List<Channel> Channels { get; set; }
        public int TailId { get; set; }

        public TelemetryDevice(Location location, List<Channel> channels)
        {
            Location = location;
            Channels = channels;
        }

        public TelemetryDevice(Location location, int tailId)
        {
            Location = location;
            Channels = new List<Channel>();
            TailId = tailId;
        }

        public string GetStatus()
        {
            return $"Telemetry Device With Tail Id {TailId} located at Lat: {Location.Latitude}, Lon: {Location.Longitude}";
        }
    }
}
