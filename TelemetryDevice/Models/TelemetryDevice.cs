using Shared.Models.ICDModels;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Models
{
    public class TelemetryDevice
    {
        public Location Location { get; set; }
        public List<Channel> Channels { get; set; }

        public TelemetryDevice(Location location, List<Channel> channels)
        {
            Location = location;
            Channels = channels;
        }

        public TelemetryDevice(Location location)
        {
            Location = location;
            Channels = new List<Channel>();
        }

        public void AddChannel(int portNumber, IPipeLine telemetryPipeline, ICD channelIcd)
        {
            var newTelemetryChannel = new Channel(portNumber, telemetryPipeline, channelIcd);
            Channels.Add(newTelemetryChannel);
        }

        public bool RemoveChannel(int portNumber)
        {
            var channelIndex = Channels.FindIndex(telemetryChannel =>
                telemetryChannel.PortNumber == portNumber
            );

            if (channelIndex == -1)
                return false;

            Channels.RemoveAt(channelIndex);
            return true;
        }

        public Channel? GetChannelByPort(int portNumber)
        {
            return Channels.FirstOrDefault(telemetryChannel =>
                telemetryChannel.PortNumber == portNumber
            );
        }
    }
}
