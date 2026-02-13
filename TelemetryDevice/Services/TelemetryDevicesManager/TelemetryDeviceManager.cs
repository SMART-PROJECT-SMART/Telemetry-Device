using Core.Models;
using Core.Models.ICDModels;
using Core.Services.ICDsDirectory;
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
        private readonly Dictionary<string, TelemetryDevice> _telemetryDevicesBySleeveName;
        private readonly object _lockObject = new object();
        private readonly IICDDirectory _icdDirectory;
        private readonly IPortManager _portManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IQuartzTelemetryDeviceStatusManager _quartzTelemetryDeviceStatusManager;
        private readonly TelemetryDeviceStatusConfiguration _telemetryDeviceStatusConfiguration;
        private bool _isSchedulerStarted;

        public TelemetryDeviceManager(
            IICDDirectory icdDirectory,
            IPortManager portManager,
            IServiceProvider serviceProvider,
            IQuartzTelemetryDeviceStatusManager quartzTelemetryDeviceStatusManager,
            IOptions<TelemetryDeviceStatusConfiguration> configuration
        )
        {
            _icdDirectory = icdDirectory;
            _portManager = portManager;
            _serviceProvider = serviceProvider;
            _quartzTelemetryDeviceStatusManager = quartzTelemetryDeviceStatusManager;
            _telemetryDeviceStatusConfiguration = configuration.Value;
            _telemetryDevicesBySleeveName = new Dictionary<string, TelemetryDevice>();
            _isSchedulerStarted = false;
        }

        public async Task AddTelemetryDeviceAsync(
            string sleeveName,
            int? tailId,
            IEnumerable<int> portNumbers,
            Location location
        )
        {
            TelemetryDevice newTelemetryDevice;
            lock (_lockObject)
            {
                ValidateTelemetryDeviceDoesNotExist(sleeveName);
                newTelemetryDevice = new TelemetryDevice(sleeveName, location, tailId);
                _telemetryDevicesBySleeveName[sleeveName] = newTelemetryDevice;
            }

            List<ICD> availableIcds = _icdDirectory.GetAllICDs();
            CreateTelemetryChannelsForDevice(newTelemetryDevice, portNumbers.ToList(), availableIcds);

            await StartSchedulerIfNeeded();
        }

        private async Task StartSchedulerIfNeeded()
        {
            if (!_isSchedulerStarted)
            {
                await _quartzTelemetryDeviceStatusManager.StartSchedular(
                    _telemetryDeviceStatusConfiguration.JobInterval
                );
                _isSchedulerStarted = true;
            }
        }

        private void ValidateTelemetryDeviceDoesNotExist(string sleeveName)
        {
            if (_telemetryDevicesBySleeveName.ContainsKey(sleeveName))
            {
                throw new ArgumentException(
                    $"Telemetry device for sleeve '{sleeveName}' already exists."
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
                    decodedTailId => UpdateDeviceTailId(newTelemetryDevice, decodedTailId)
                );
                newTelemetryDevice.Channels.Add(channel);
                _portManager.AddPort(portNumbers[channelIndex], channel);
            }
        }

        private void UpdateDeviceTailId(TelemetryDevice device, int decodedTailId)
        {
            lock (_lockObject)
            {
                device.TailId = decodedTailId;
            }
        }

        public bool RemoveTelemetryDevice(string sleeveName)
        {
            TelemetryDevice? targetTelemetryDevice;
            lock (_lockObject)
            {
                if (!_telemetryDevicesBySleeveName.TryGetValue(sleeveName, out targetTelemetryDevice))
                {
                    return false;
                }

                _telemetryDevicesBySleeveName.Remove(sleeveName);
            }

            foreach (Channel telemetryDeviceChannel in targetTelemetryDevice.Channels)
            {
                _portManager.RemovePort(telemetryDeviceChannel.PortNumber);
                telemetryDeviceChannel.TelemetryPipeLine.Dispose();
            }

            return true;
        }

        public void UpdatePortsForSleeve(string sleeveName, IEnumerable<int> newPorts)
        {
            TelemetryDevice device;
            lock (_lockObject)
            {
                if (!_telemetryDevicesBySleeveName.TryGetValue(sleeveName, out device))
                {
                    return;
                }
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
                return _telemetryDevicesBySleeveName.Values.ToList();
            }
        }
    }
}
