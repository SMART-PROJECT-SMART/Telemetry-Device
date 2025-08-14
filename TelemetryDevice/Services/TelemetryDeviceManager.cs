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
        private readonly IPortManager _portManager;
        private readonly ILogger<TelemetryDeviceManager> _logger;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPacketSniffer packetSniffer,
            IPipeLine pipeLine,
            IPortManager portManager,
            ILogger<TelemetryDeviceManager> logger
        )
        {
            _icdDirectory = icdDirectory;
            _packetSniffer = packetSniffer;
            _packetSniffer.PacketReceived += OnPacketReceived;
            _pipeLine = pipeLine;
            _portManager = portManager;
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
            
            for (int index = 0; index < icds.Count && index < portNumbers.Count; index++)
            {
                telemetryDevice.AddChannel(portNumbers[index], _pipeLine, icds[index]);
                
                var channel = telemetryDevice.Channels.FirstOrDefault(c => c.PortNumber == portNumbers[index]);
                if (channel != null)
                {
                    _portManager.AddPort(portNumbers[index], channel);
                }
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            if (!_telemetryDevicesByTailId.TryGetValue(tailId, out var device))
            {
                return false;
            }

            foreach (var channel in device.Channels)
            {
                _portManager.RemovePort(channel.PortNumber);
            }

            return _telemetryDevicesByTailId.Remove(tailId);
        }

        private void OnPacketReceived(byte[] payload, int destinationPort)
        {
            int? tailId = TailIdExtractor.GetTailIdByICD(payload);

            if (tailId.HasValue && _telemetryDevicesByTailId.TryGetValue(tailId.Value, out var device))
            {
                device.RunOnSpecificChannel(destinationPort, payload);
            }
        }

        public void SwitchPorts(int sourcePort, int destinationPort)
        {
            _portManager.SwitchPorts(sourcePort, destinationPort);
        }
    }
}
