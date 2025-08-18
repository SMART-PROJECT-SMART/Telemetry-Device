using Shared.Models.ICDModels;
using Shared.Services.ICDsDirectory;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Builders;
using TelemetryDevices.Services.Factories.PacketHandler;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.PortsManager;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Services
{
    public class TelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId =
            new Dictionary<int, TelemetryDevice>();
        private readonly IICDDirectory _icdDirectory;
        private readonly IPacketSniffer _packetSniffer;
        private readonly IPipeLineBuilder _pipeLineBuilder;
        private readonly PipeLineDirector _pipeLineDirector;
        private readonly IPortManager _portManager;
        private readonly ILogger<TelemetryDeviceManager> _logger;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPacketSniffer packetSniffer,
            IPipeLineBuilder pipeLineBuilder,
            PipeLineDirector pipeLineDirector,
            IPortManager portManager,
            ILogger<TelemetryDeviceManager> logger
        )
        {
            _icdDirectory = icdDirectory;
            _packetSniffer = packetSniffer;
            _pipeLineBuilder = pipeLineBuilder;
            _pipeLineDirector = pipeLineDirector;
            _portManager = portManager;
            _logger = logger;
        }

        public void AddTelemetryDevice(int tailId, List<int> portNumbers, Location location)
        {
            ValidateTelemetryDeviceDoesNotExist(tailId);
            var telemetryDevice = CreateTelemetryDevice(location);
            _telemetryDevicesByTailId[tailId] = telemetryDevice;
            
            var icds = _icdDirectory.GetAllICDs().ToList();
            CreateChannelsForDevice(telemetryDevice, portNumbers, icds);
        }

        private void ValidateTelemetryDeviceDoesNotExist(int tailId)
        {
            if (_telemetryDevicesByTailId.ContainsKey(tailId))
            {
                throw new ArgumentException(
                    $"Telemetry device with tail ID {tailId} already exists."
                );
            }
        }

        private TelemetryDevice CreateTelemetryDevice(Location location)
        {
            return new TelemetryDevice(location);
        }

        private void CreateChannelsForDevice(TelemetryDevice telemetryDevice, List<int> portNumbers, List<ICD> icds)
        {
            for (int index = 0; index < icds.Count && index < portNumbers.Count; index++)
            {
                var pipeline = CreatePipeline();
                telemetryDevice.AddChannel(portNumbers[index], pipeline, icds[index]);
                
                var channel = telemetryDevice.Channels.FirstOrDefault(c => c.PortNumber == portNumbers[index]);
                if (channel != null)
                {
                    _portManager.AddPort(portNumbers[index], channel);
                }
            }
        }

        private IPipeLine CreatePipeline()
        {
            _pipeLineDirector.BuildTelemetryPipeline();
            return _pipeLineBuilder.GetProduct();
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
