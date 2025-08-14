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

        public void RunOnAllChannels(byte[] data)
        {
            foreach (var channel in Channels)
            {
                channel.PipeLine.ProcessDataAsync(data);
            }
        }

    
        public void AddChannel(int portNumber, ICD icd)
        {
            var newChannel = new Channel(portNumber, icd);
            Channels.Add(newChannel);
            newChannel.PipeLine.SetICD(icd);
        }

        public void AddChannel(int portNumber, IPipeLine pipeLine, ICD icd)
        {
            var newChannel = new Channel(portNumber, pipeLine, icd);
            Channels.Add(newChannel);
            newChannel.PipeLine.SetICD(icd);
        }

        public bool RemoveChannel(int portNumber)
        {
            var index = Channels.FindIndex(channel => channel.PortNumber == portNumber);

            if (index == -1)
                return false;

            Channels.RemoveAt(index);
            return true;
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
