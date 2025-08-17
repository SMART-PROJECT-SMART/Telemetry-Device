using Shared.Models.ICDModels;
using Shared.Services.ICDsDirectory;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Builders;
using TelemetryDevices.Services.Factories.PacketHandler;
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
        private readonly IPipelineBuilder _pipelineBuilder;
        private readonly PipelineDirector _pipelineDirector;
        private readonly IPortManager _portManager;
        private readonly ILogger<TelemetryDeviceManager> _logger;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPacketSniffer packetSniffer,
            IPipelineBuilder pipelineBuilder,
            PipelineDirector pipelineDirector,
            IPortManager portManager,
            ILogger<TelemetryDeviceManager> logger
        )
        {
            _icdDirectory = icdDirectory;
            _packetSniffer = packetSniffer;
            _pipelineBuilder = pipelineBuilder;
            _pipelineDirector = pipelineDirector;
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

            var telemetryDevice = new TelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = telemetryDevice;
            var icds = _icdDirectory.GetAllICDs().ToList();
            
            for (int index = 0; index < icds.Count && index < portNumbers.Count; index++)
            {
                _pipelineDirector.BuildTelemetryPipeline();
                var pipeline = _pipelineBuilder.GetProduct();
                telemetryDevice.AddChannel(portNumbers[index], pipeline, icds[index]);
                
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
    }
}
