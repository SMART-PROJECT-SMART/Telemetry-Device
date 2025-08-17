using TelemetryDevices.Models;
using TelemetryDevices.Services.Helpers;
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
            _packetSniffer.PacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(byte[] payload, int destinationPort)
        {
            ProcessPacketOnPort(destinationPort, payload);
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
            ValidateSourceChannelExists(sourcePort, sourceChannel);

            var destinationChannel = GetChannelByPort(destinationPort);
            
            if (destinationChannel != null)
            {
                SwapExistingPorts(sourcePort, destinationPort, sourceChannel, destinationChannel);
            }
            else
            {
                MovePortToNewDestination(sourcePort, destinationPort, sourceChannel);
            }
        }

        private void ValidateSourceChannelExists(int sourcePort, Channel? sourceChannel)
        {
            if (sourceChannel == null)
            {
                _logger.LogWarning("Source port {SourcePort} not found", sourcePort);
                throw new InvalidOperationException($"Source port {sourcePort} not found");
            }
        }

        private void SwapExistingPorts(int sourcePort, int destinationPort, Channel sourceChannel, Channel destinationChannel)
        {
            _logger.LogInformation("Swapping ports {SourcePort} and {DestinationPort}", sourcePort, destinationPort);
            
            UpdatePortMappings(sourcePort, destinationPort, sourceChannel, destinationChannel);
            UpdateChannelPortNumbers(sourceChannel, destinationChannel, sourcePort, destinationPort);
            UpdateSnifferPorts(sourcePort, destinationPort, sourceChannel.PortNumber, destinationChannel.PortNumber);
        }

        private void MovePortToNewDestination(int sourcePort, int destinationPort, Channel sourceChannel)
        {
            _logger.LogInformation("Changing port {SourcePort} to {DestinationPort}", sourcePort, destinationPort);
            
            _portToChannel.Remove(sourcePort);
            _portToChannel[destinationPort] = sourceChannel;
            
            sourceChannel.PortNumber = destinationPort;
            
            UpdateSnifferPorts(sourcePort, destinationPort, destinationPort);
        }

        private void UpdatePortMappings(int sourcePort, int destinationPort, Channel sourceChannel, Channel destinationChannel)
        {
            _portToChannel[sourcePort] = destinationChannel;
            _portToChannel[destinationPort] = sourceChannel;
        }

        private void UpdateChannelPortNumbers(Channel sourceChannel, Channel destinationChannel, int sourcePort, int destinationPort)
        {
            sourceChannel.PortNumber = destinationPort;
            destinationChannel.PortNumber = sourcePort;
        }

        private void UpdateSnifferPorts(int sourcePort, int destinationPort, params int[] newPorts)
        {
            _packetSniffer.RemovePort(sourcePort);
            _packetSniffer.RemovePort(destinationPort);
            
            foreach (var port in newPorts)
            {
                _packetSniffer.AddPort(port);
            }
        }

        public IEnumerable<int> GetAllPorts()
        {
            return _portToChannel.Keys;
        }

        public void ProcessPacketOnPort(int portNumber, byte[] payload)
        {
            var channel = GetChannelByPort(portNumber);

            int? tailId = TailIdExtractor.GetTailIdByICD(payload, channel.ICD);
            if (!tailId.HasValue)
            {
                _logger.LogWarning("Could not extract tail ID from payload on port {Port}", portNumber);
                return;
            }

            channel.PipeLine.ProcessDataAsync(payload);
        }
    }
}
