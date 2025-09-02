using TelemetryDevices.Models;
using TelemetryDevices.Services.Helpers;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Services.PortsManager
{
    public class PortManager : IPortManager
    {
        private readonly Dictionary<int, Channel> _portToChannel = new();
        private readonly IPacketSniffer _packetSniffer;

        public PortManager(IPacketSniffer packetSniffer)
        {
            _packetSniffer = packetSniffer;
            _packetSniffer.PacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(byte[] packetPayload, int destinationPort)
        {
            ProcessPacketOnPort(destinationPort, packetPayload);
        }

        public void AddPort(int portNumber, Channel assignedChannel)
        {
            if (_portToChannel.ContainsKey(portNumber))
            {
                return;
            }

            _portToChannel[portNumber] = assignedChannel;
            _packetSniffer.AddPort(portNumber);
        }

        public void RemovePort(int portNumber)
        {
            if (_portToChannel.Remove(portNumber))
            {
                _packetSniffer.RemovePort(portNumber);
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
                throw new InvalidOperationException($"Source port {sourcePort} not found");
            }
        }

        private void SwapExistingPorts(
            int sourcePort,
            int destinationPort,
            Channel sourceChannel,
            Channel destinationChannel
        )
        {
            UpdatePortMappings(sourcePort, destinationPort, sourceChannel, destinationChannel);
            UpdateChannelPortNumbers(
                sourceChannel,
                destinationChannel,
                sourcePort,
                destinationPort
            );
            UpdateSnifferPorts(
                sourcePort,
                destinationPort,
                sourceChannel.PortNumber,
                destinationChannel.PortNumber
            );
        }

        private void MovePortToNewDestination(
            int sourcePort,
            int destinationPort,
            Channel sourceChannel
        )
        {
            _portToChannel.Remove(sourcePort);
            _portToChannel[destinationPort] = sourceChannel;

            sourceChannel.PortNumber = destinationPort;

            UpdateSnifferPorts(sourcePort, destinationPort, destinationPort);
        }

        private void UpdatePortMappings(
            int sourcePort,
            int destinationPort,
            Channel sourceChannel,
            Channel destinationChannel
        )
        {
            _portToChannel[sourcePort] = destinationChannel;
            _portToChannel[destinationPort] = sourceChannel;
        }

        private void UpdateChannelPortNumbers(
            Channel sourceChannel,
            Channel destinationChannel,
            int sourcePort,
            int destinationPort
        )
        {
            sourceChannel.PortNumber = destinationPort;
            destinationChannel.PortNumber = sourcePort;
        }

        private void UpdateSnifferPorts(
            int sourcePort,
            int destinationPort,
            params int[] assignedPortNumbers
        )
        {
            _packetSniffer.RemovePort(sourcePort);
            _packetSniffer.RemovePort(destinationPort);

            foreach (var portNumber in assignedPortNumbers)
            {
                _packetSniffer.AddPort(portNumber);
            }
        }

        public IEnumerable<int> GetAllPorts()
        {
            return _portToChannel.Keys;
        }

        public void ProcessPacketOnPort(int portNumber, byte[] packetPayload)
        {
            var assignedChannel = GetChannelByPort(portNumber);
            
            if (assignedChannel == null)
            {
                return;
            }

            int? extractedTailId = TailIdExtractor.GetTailIdByICD(
                packetPayload,
                assignedChannel.ICD
            );
            if (!extractedTailId.HasValue)
            {
                return;
            }

            assignedChannel.ProcessTelemetryData(packetPayload);
        }
    }
}
