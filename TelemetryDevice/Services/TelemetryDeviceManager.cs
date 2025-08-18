using Shared.Models.ICDModels;
using Shared.Services.ICDsDirectory;
using TelemetryDevices.Models;
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
        private readonly PipeLineDirector _pipeLineDirector;
        private readonly IPortManager _portManager;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            PipeLineDirector pipeLineDirector,
            IPortManager portManager
        )
        {
            _icdDirectory = icdDirectory;
            _pipeLineDirector = pipeLineDirector;
            _portManager = portManager;
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
            return _pipeLineDirector.BuildTelemetryPipeline();
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