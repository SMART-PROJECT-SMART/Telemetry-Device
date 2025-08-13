using Shared.Models.ICDModels;

namespace TelemetryDevices.Models
{
    public class TelemetryDevice
    {
        public Location Location { get; set; }
        public List<Channel> Channels { get; set; }
        public TelemetryDevice(Location location,List<Channel> channels)
        {
            Location = location;
            Channels = channels;
        }
        public TelemetryDevice(Location location)
        {
            Location = location;
            Channels = new List<Channel>();
        }

        public void AddChannel(int portNumber,ICD icd)
        {
            var newChannel = new Channel(portNumber, icd);
            Channels.Add(newChannel);
        }

        public bool RemoveChannel(int portNumber)
        {
            var index = Channels.FindIndex(channel => channel.PortNumber == portNumber);

            if (index == -1) return false;

            Channels.RemoveAt(index);
            return true;

        }

    }
}
