using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TelemetryDevices.Dto.DeviceManager;
using TelemetryDevices.Services.DeviceManagerClient;
using TelemetryDevices.Services.TelemetryDevicesManager;

namespace TelemetryDevices.Services.SleeveStorage
{
    public class SleeveStorageService : ISleeveStorageService, IHostedService
    {
        private readonly ConcurrentDictionary<string, DeviceManagerSleeveDto> _sleeves;
        private readonly IDeviceManagerClient _deviceManagerClient;
        private readonly ITelemetryDeviceManager _telemetryDeviceManager;
        private readonly ILogger<SleeveStorageService> _logger;

        public SleeveStorageService(
            IDeviceManagerClient deviceManagerClient,
            ITelemetryDeviceManager telemetryDeviceManager,
            ILogger<SleeveStorageService> logger)
        {
            _sleeves = new ConcurrentDictionary<string, DeviceManagerSleeveDto>();
            _deviceManagerClient = deviceManagerClient;
            _telemetryDeviceManager = telemetryDeviceManager;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<DeviceManagerSleeveDto> sleeves = await _deviceManagerClient.GetAllSleevesAsync(cancellationToken);
            int sleeveCount = sleeves.Count();
            _logger.LogInformation("InitializeAsync: fetched {SleeveCount} sleeves from DeviceManager", sleeveCount);

            foreach (DeviceManagerSleeveDto sleeve in sleeves)
            {
                _sleeves.TryAdd(sleeve.Name, sleeve);
                _logger.LogInformation("Adding device for sleeve {SleeveName} on startup", sleeve.Name);

                await _telemetryDeviceManager.AddTelemetryDeviceAsync(
                    sleeve.Name,
                    sleeve.Id,
                    null,
                    sleeve.PortNumbers,
                    sleeve.Location);
            }
        }

        public async Task AddOrUpdateSleeveAsync(DeviceManagerSleeveDto sleeve)
        {
            bool exists = _sleeves.ContainsKey(sleeve.Name);

            _sleeves.AddOrUpdate(sleeve.Name, sleeve, (key, existing) => sleeve);

            if (exists)
            {
                _telemetryDeviceManager.UpdatePortsForSleeve(sleeve.Name, sleeve.PortNumbers);
            }
            else
            {
                await _telemetryDeviceManager.AddTelemetryDeviceAsync(
                    sleeve.Name,
                    sleeve.Id,
                    null,
                    sleeve.PortNumbers,
                    sleeve.Location);
            }
        }

        public void RemoveSleeve(string name)
        {
            if (_sleeves.TryRemove(name, out _))
            {
                _telemetryDeviceManager.RemoveTelemetryDevice(name);
            }
        }

        public DeviceManagerSleeveDto GetSleeve(string name)
        {
            _sleeves.TryGetValue(name, out DeviceManagerSleeveDto sleeve);
            return sleeve;
        }

        public IEnumerable<DeviceManagerSleeveDto> GetAllSleeves()
        {
            return _sleeves.Values;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return InitializeAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
