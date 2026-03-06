using Core.Models;
using Core.Models.ICDModels;
using Core.Services.ICDsDirectory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelemetryDevices.Config;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.PortsManager;
using TelemetryDevices.Services.Quartz.TelemetryDeviceStatusManager;

namespace TelemetryDevices.Services.TelemetryDevicesManager
{
    public class TelemetryDeviceManager : ITelemetryDeviceManager
    {
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesById;
        private readonly object _lockObject = new object();
        private readonly IICDDirectory _icdDirectory;
        private readonly IPortManager _portManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IQuartzTelemetryDeviceStatusManager _quartzTelemetryDeviceStatusManager;
        private readonly TelemetryDeviceStatusConfiguration _telemetryDeviceStatusConfiguration;
        private readonly ILogger<TelemetryDeviceManager> _logger;
        private bool _isSchedulerStarted;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPortManager portManager,
            IServiceProvider serviceProvider,
            IQuartzTelemetryDeviceStatusManager quartzTelemetryDeviceStatusManager,
            IOptions<TelemetryDeviceStatusConfiguration> configuration,
            ILogger<TelemetryDeviceManager> logger
        )
        {
            _icdDirectory = icdDirectory;
            _portManager = portManager;
            _serviceProvider = serviceProvider;
            _quartzTelemetryDeviceStatusManager = quartzTelemetryDeviceStatusManager;
            _telemetryDeviceStatusConfiguration = configuration.Value;
            _logger = logger;
            _telemetryDevicesById = new Dictionary<int, TelemetryDevice>();
            _isSchedulerStarted = false;
        }

        public async Task AddTelemetryDeviceAsync(
            string sleeveName,
            int sleeveId,
            int? tailId,
            IEnumerable<int> portNumbers,
            Location location
        )
        {
            TelemetryDevice newTelemetryDevice;
            lock (_lockObject)
            {
                ValidateTelemetryDeviceDoesNotExist(sleeveId);
                newTelemetryDevice = new TelemetryDevice(sleeveName, sleeveId, location, tailId);
                _telemetryDevicesById[sleeveId] = newTelemetryDevice;
            }

            _logger.LogInformation("Adding telemetry device for sleeve {SleeveName} (Id={SleeveId})", sleeveName, sleeveId);

            List<ICD> availableIcds = _icdDirectory.GetAllICDs();
            CreateTelemetryChannelsForDevice(newTelemetryDevice, portNumbers.ToList(), availableIcds);

            await StartSchedulerIfNeeded();
        }

        private async Task StartSchedulerIfNeeded()
        {
            if (!_isSchedulerStarted)
            {
                _logger.LogInformation("Starting Quartz scheduler for telemetry device status (interval: {IntervalSeconds}s)", _telemetryDeviceStatusConfiguration.JobInterval);
                await _quartzTelemetryDeviceStatusManager.StartSchedular(
                    _telemetryDeviceStatusConfiguration.JobInterval
                );
                _isSchedulerStarted = true;
            }
        }

        private void ValidateTelemetryDeviceDoesNotExist(int sleeveId)
        {
            if (_telemetryDevicesById.ContainsKey(sleeveId))
            {
                throw new ArgumentException(
                    $"Telemetry device for sleeve Id '{sleeveId}' already exists."
                );
            }
        }

        private void CreateTelemetryChannelsForDevice(
            TelemetryDevice newTelemetryDevice,
            List<int> portNumbers,
            List<ICD> availableIcds
        )
        {
            for (
                int channelIndex = 0;
                channelIndex < availableIcds.Count && channelIndex < portNumbers.Count;
                channelIndex++
            )
            {
                ICD currentTelemetryIcd = availableIcds[channelIndex];

                ITelemetryPipeLine telemetryPipeLine =
                    _serviceProvider.GetRequiredService<ITelemetryPipeLine>();

                Channel channel = new(portNumbers[channelIndex], telemetryPipeLine);
                channel.TelemetryPipeLine.BuildPipelineBlocks(
                    currentTelemetryIcd,
                    (decodedTailId, location) => UpdateDeviceFromTelemetry(newTelemetryDevice, decodedTailId, location)
                );
                newTelemetryDevice.Channels.Add(channel);
                _portManager.AddPort(portNumbers[channelIndex], channel);
            }
        }

        private void UpdateDeviceFromTelemetry(TelemetryDevice device, int decodedTailId, Location location)
        {
            lock (_lockObject)
            {
                foreach (TelemetryDevice otherDevice in _telemetryDevicesById.Values)
                {
                    if (otherDevice != device && otherDevice.TailId == decodedTailId)
                    {
                        otherDevice.TailId = null;
                        otherDevice.TransmittingUavLocation = null;
                    }
                }

                device.TailId = decodedTailId;
                device.TransmittingUavLocation = location;
            }
        }

        public bool RemoveTelemetryDevice(int sleeveId)
        {
            TelemetryDevice? targetTelemetryDevice;
            lock (_lockObject)
            {
                if (!_telemetryDevicesById.TryGetValue(sleeveId, out targetTelemetryDevice))
                {
                    return false;
                }

                _telemetryDevicesById.Remove(sleeveId);
            }

            foreach (Channel telemetryDeviceChannel in targetTelemetryDevice.Channels)
            {
                _portManager.RemovePort(telemetryDeviceChannel.PortNumber);
                telemetryDeviceChannel.TelemetryPipeLine.Dispose();
            }

            return true;
        }

        public void UpdatePortsForSleeve(int sleeveId, IEnumerable<int> newPorts)
        {
            TelemetryDevice device;
            lock (_lockObject)
            {
                if (!_telemetryDevicesById.TryGetValue(sleeveId, out device))
                {
                    return;
                }

                device.TailId = null;
                device.TransmittingUavLocation = null;
            }

            List<Channel> channels = device.Channels;
            List<int> portList = newPorts.ToList();

            for (int i = 0; i < channels.Count && i < portList.Count; i++)
            {
                _portManager.SwitchPorts(channels[i].PortNumber, portList[i]);
            }
        }

        public IEnumerable<TelemetryDevice> GetAllTelemetryDevices()
        {
            lock (_lockObject)
            {
                List<TelemetryDevice> devices = _telemetryDevicesById.Values.ToList();
                _logger.LogInformation("GetAllTelemetryDevices called, returning {DeviceCount} devices", devices.Count);
                return devices;
            }
        }
    }
}
