using Shared.Models.ICDModels;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Models
{
    public class TelemetryDevice
    {
        public Location Location { get; set; }
        public List<Channel> Channels { get; set; }
        private readonly IPipeLine _pipeLine;

        public TelemetryDevice(Location location, List<Channel> channels, IPipeLine pipeLine)
        {
            Location = location;
            Channels = channels;
            _pipeLine = pipeLine;
        }

        public TelemetryDevice(Location location, IPipeLine pipeLine)
        {
            Location = location;
            Channels = new List<Channel>();
            _pipeLine = pipeLine;
        }

        public void RunOnAllChannels(byte[] data)
        {
            foreach (var channel in Channels)
            {
                channel.PipeLine.ProcessDataAsync(data);
            }
        }

    
        public void AddChannel(int portNumber, ICD icd)
        {
            var newChannel = new Channel(portNumber, _pipeLine, icd);
            Channels.Add(newChannel);
        }

        public void AddChannel(int portNumber, IPipeLine pipeLine, ICD icd)
        {
            var newChannel = new Channel(portNumber, pipeLine, icd);
            Channels.Add(newChannel);
        }

        public bool RemoveChannel(int portNumber)
        {
            var index = Channels.FindIndex(channel => channel.PortNumber == portNumber);

            if (index == -1)
                return false;

            Channels.RemoveAt(index);
            return true;
        }

        public Channel? GetChannelByPort(int portNumber)
        {
            return Channels.FirstOrDefault(c => c.PortNumber == portNumber);
        }

        public void RunOnSpecificChannel(int portNumber, byte[] data)
        {
            var channel = Channels.FirstOrDefault(c => c.PortNumber == portNumber);
            if (channel != null)
            {
                channel.PipeLine.ProcessDataAsync(data);
            }
        }
    }
}
