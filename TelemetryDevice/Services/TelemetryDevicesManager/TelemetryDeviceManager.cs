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
        private readonly Dictionary<int, TelemetryDevice> _telemetryDevicesByTailId;
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
            _telemetryDevicesByTailId = new Dictionary<int, TelemetryDevice>();
            _isSchedulerStarted = false;
        }

        public async Task AddTelemetryDeviceAsync(
            int tailId,
            IEnumerable<int> portNumbers,
            Location location
        )
        {
            TelemetryDevice newTelemetryDevice;
            lock (_lockObject)
            {
                ValidateTelemetryDeviceDoesNotExist(tailId);
                newTelemetryDevice = new TelemetryDevice(location, tailId);
                _telemetryDevicesByTailId[tailId] = newTelemetryDevice;
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

        private void ValidateTelemetryDeviceDoesNotExist(int tailId)
        {
            if (_telemetryDevicesByTailId.ContainsKey(tailId))
            {
                throw new ArgumentException(
                    $"Telemetry device with tail ID {tailId} already exists."
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
                _telemetryDevicesByTailId.Remove(device.TailId);
                device.TailId = decodedTailId;
                _telemetryDevicesByTailId[decodedTailId] = device;
            }
        }

        public bool RemoveTelemetryDevice(int tailId)
        {
            TelemetryDevice? targetTelemetryDevice;
            lock (_lockObject)
            {
                if (
                    !_telemetryDevicesByTailId.TryGetValue(
                        tailId,
                        out targetTelemetryDevice
                    )
                )
                {
                    return false;
                }

                _telemetryDevicesByTailId.Remove(tailId);
            }

            foreach (Channel telemetryDeviceChannel in targetTelemetryDevice.Channels)
            {
                _portManager.RemovePort(telemetryDeviceChannel.PortNumber);
                telemetryDeviceChannel.TelemetryPipeLine.Dispose();
            }

            return true;
        }

        public IEnumerable<TelemetryDevice> GetAllTelemetryDevices()
        {
            lock (_lockObject)
            {
                return _telemetryDevicesByTailId.Values.ToList();
            }
        }
    }
}
