using Shared.Services.ICDDirectory;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Factories.PacketHandler;
using TelemetryDevices.Services.Helpers;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Services
{
    public class TelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId = new Dictionary<int, TelemetryDevice>();
        private readonly IICDDirectory _icdDirectory;
        private readonly IPacketSniffer _packetSniffer;
        private readonly IPipeLine _pipeLine;

        public TelemetryDeviceManager(IICDDirectory icdDirectory,IPacketSniffer packetSniffer,IPipeLine pipeLine)
        {
            _icdDirectory = icdDirectory;
            _packetSniffer = packetSniffer;
            _packetSniffer.PacketReceived += OnPacketReceived;
            _pipeLine = pipeLine;
        }

        public void AddTelemetryDevice(int tailId, List<int> portNumbers, Location location)
        {
            if (_telemetryDevicesByTailId.ContainsKey(tailId))
            {
                throw new ArgumentException($"Telemetry device with tail ID {tailId} already exists.");
            }

            var telemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = telemetryDevice;

            var icds = _icdDirectory.GetAllICDs().ToList();
            for (int index = 0; index < icds.Count; index++)
            {
                telemetryDevice.AddChannel(portNumbers[index],_pipeLine, icds[index]);
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            return _telemetryDevicesByTailId.Remove(tailId);
        }

        public bool Exists(int tailId)
        {
            return _telemetryDevicesByTailId.ContainsKey(tailId);
        }
        private void OnPacketReceived(byte[] payload)
        {
            foreach (var icd in _icdDirectory.GetAllICDs())
            {
                int tailId = TailIdExtractor.GetTailIdByICD(payload, icd) ?? -1;
                if (tailId == -1)
                {
                    continue;
                }
                _telemetryDevicesByTailId[tailId].RunOnAllChannels(payload);
            }
        }
    }
}