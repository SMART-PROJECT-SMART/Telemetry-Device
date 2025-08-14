using Shared.Models.ICDModels;
using Shared.Services.ICDsDirectory;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Factories.PacketHandler;
using TelemetryDevices.Services.Helpers;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Services
{
    public class TelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId =
            new Dictionary<int, TelemetryDevice>();
        private readonly IICDDirectory _icdDirectory;
        private readonly IPacketSniffer _packetSniffer;
        private readonly IPipeLine _pipeLine;
        private readonly ILogger<TelemetryDeviceManager> _logger;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPacketSniffer packetSniffer,
            IPipeLine pipeLine,
            ILogger<TelemetryDeviceManager> logger
        )
        {
            _icdDirectory = icdDirectory;
            _packetSniffer = packetSniffer;
            _packetSniffer.PacketReceived += OnPacketReceived;
            _pipeLine = pipeLine;
            _logger = logger;
        }

        public void AddTelemetryDevice(int tailId, List<int> portNumbers, Location location)
        {
            if (_telemetryDevicesByTailId.ContainsKey(tailId))
            {
                throw new ArgumentException(
                    $"Telemetry device with tail ID {tailId} already exists."
                );
            }

            var telemetryDevice = new TelemetryDevice(location, _pipeLine);
            _telemetryDevicesByTailId[tailId] = telemetryDevice;
            var icds = _icdDirectory.GetAllICDs().ToList();
            for (int index = 0; index < icds.Count; index++)
            {
                telemetryDevice.AddChannel(portNumbers[index], icds[index]);
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            return _telemetryDevicesByTailId.Remove(tailId);
        }

        private void OnPacketReceived(byte[] payload, int destinationPort)
        {

            int? tailId = null;
            tailId = TailIdExtractor.GetTailIdByICD(payload);

            if (tailId.HasValue && _telemetryDevicesByTailId.TryGetValue(tailId.Value, out var value))
            {
                value.RunOnSpecificChannel(destinationPort, payload);
            }
        }

        public void SwitchPorts(int sourcePort, int destinationPort)
        {
            Channel sourceChannel = null;
            Channel destinationChannel = null;
            TelemetryDevice sourceDevice = null;
            TelemetryDevice destinationDevice = null;

            foreach (var device in _telemetryDevicesByTailId.Values)
            {
                var source = device.Channels.FirstOrDefault(c => c.PortNumber == sourcePort);
                var dest = device.Channels.FirstOrDefault(c => c.PortNumber == destinationPort);

                if (source != null)
                {
                    sourceChannel = source;
                    sourceDevice = device;
                }

                if (dest != null)
                {
                    destinationChannel = dest;
                    destinationDevice = device;
                }
            }

            if (sourceChannel == null)
            {
                _logger.LogWarning($"Source port {sourcePort} not found");
                return;
            }

            if (destinationChannel != null)
            {
                _logger.LogInformation($"Swapping ports {sourcePort} and {destinationPort}");

                int tempPort = sourceChannel.PortNumber;
                sourceChannel.PortNumber = destinationChannel.PortNumber;
                destinationChannel.PortNumber = tempPort;

                _packetSniffer.RemovePort(sourcePort);
                _packetSniffer.RemovePort(destinationPort);
                _packetSniffer.AddPort(sourceChannel.PortNumber);
                _packetSniffer.AddPort(destinationChannel.PortNumber);
            }
            else
            {
                _logger.LogInformation($"Changing port {sourcePort} to {destinationPort}");

                _packetSniffer.RemovePort(sourcePort);
                _packetSniffer.AddPort(destinationPort);

                sourceChannel.PortNumber = destinationPort;
            }
        }
    }
}
