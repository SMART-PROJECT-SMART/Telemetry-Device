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

            var telemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = telemetryDevice;
            int amount = 1;
            var icds = _icdDirectory.GetAllICDs().ToList();
            for (int index = 0; index < icds.Count; index++)
            {
                telemetryDevice.AddChannel(portNumbers[index], _pipeLine, icds[index]);
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            return _telemetryDevicesByTailId.Remove(tailId);
        }

        private void OnPacketReceived(byte[] payload, int destinationPort)
        {

            int? tailId = null;
            ICD icd = _icdDirectory.GetPortsICD(destinationPort);
                tailId = TailIdExtractor.GetTailIdByICD(payload, icd);

            if (tailId.HasValue && _telemetryDevicesByTailId.TryGetValue(tailId.Value, out var value))
            {
                value.RunOnSpecificChannel(destinationPort, payload);
            }
        }
    }
}
