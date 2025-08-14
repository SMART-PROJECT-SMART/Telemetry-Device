using TelemetryDevices.Models;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Services
{
    public class PortManager : IPortManager
    {
        private readonly Dictionary<int, Channel> _portToChannel = new();
        private readonly IPacketSniffer _packetSniffer;
        private readonly ILogger<PortManager> _logger;

        public PortManager(IPacketSniffer packetSniffer, ILogger<PortManager> logger)
        {
            _packetSniffer = packetSniffer;
            _logger = logger;
        }

        public void AddPort(int portNumber, Channel channel)
        {
            if (_portToChannel.ContainsKey(portNumber))
            {
                _logger.LogWarning("Port {PortNumber} already exists", portNumber);
                return;
            }

            _portToChannel[portNumber] = channel;
            _packetSniffer.AddPort(portNumber);
            _logger.LogInformation("Added port {PortNumber}", portNumber);
        }

        public void RemovePort(int portNumber)
        {
            if (_portToChannel.Remove(portNumber))
            {
                _packetSniffer.RemovePort(portNumber);
                _logger.LogInformation("Removed port {PortNumber}", portNumber);
            }
            else
            {
                _logger.LogWarning("Port {PortNumber} not found for removal", portNumber);
            }
        }

        public Channel? GetChannelByPort(int portNumber)
        {
            return _portToChannel.GetValueOrDefault(portNumber);
        }

        public void SwitchPorts(int sourcePort, int destinationPort)
        {
            var sourceChannel = GetChannelByPort(sourcePort);
            var destinationChannel = GetChannelByPort(destinationPort);

            if (sourceChannel == null)
            {
                _logger.LogWarning("Source port {SourcePort} not found", sourcePort);
                return;
            }

            if (destinationChannel != null)
            {
                _logger.LogInformation("Swapping ports {SourcePort} and {DestinationPort}", sourcePort, destinationPort);
                
                _portToChannel[sourcePort] = destinationChannel;
                _portToChannel[destinationPort] = sourceChannel;
                
                sourceChannel.PortNumber = destinationPort;
                destinationChannel.PortNumber = sourcePort;
                
                _packetSniffer.RemovePort(sourcePort);
                _packetSniffer.RemovePort(destinationPort);
                _packetSniffer.AddPort(sourceChannel.PortNumber);
                _packetSniffer.AddPort(destinationChannel.PortNumber);
            }
            else
            {
                _logger.LogInformation("Changing port {SourcePort} to {DestinationPort}", sourcePort, destinationPort);
                
                _portToChannel.Remove(sourcePort);
                _portToChannel[destinationPort] = sourceChannel;
                
                sourceChannel.PortNumber = destinationPort;
                
                _packetSniffer.RemovePort(sourcePort);
                _packetSniffer.AddPort(destinationPort);
            }
        }

        public IEnumerable<int> GetAllPorts()
        {
            return _portToChannel.Keys;
        }

    }
}
