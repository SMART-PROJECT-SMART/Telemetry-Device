using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TelemetryDevices.Dto.DeviceManager;
using TelemetryDevices.Services.DeviceManagerClient;
using TelemetryDevices.Services.TelemetryDevicesManager;

namespace TelemetryDevices.Services.SleeveStorage
{
    public class SleeveStorageService : ISleeveStorageService, IHostedService
    {
        private readonly ConcurrentDictionary<int, DeviceManagerSleeveDto> _sleeves;
        private readonly IDeviceManagerClient _deviceManagerClient;
        private readonly ITelemetryDeviceManager _telemetryDeviceManager;
        private readonly ILogger<SleeveStorageService> _logger;

        public SleeveStorageService(
            IDeviceManagerClient deviceManagerClient,
            ITelemetryDeviceManager telemetryDeviceManager,
            ILogger<SleeveStorageService> logger)
        {
            _sleeves = new ConcurrentDictionary<int, DeviceManagerSleeveDto>();
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
                _sleeves.TryAdd(sleeve.Id, sleeve);
                _logger.LogInformation("Adding device for sleeve {SleeveName} (Id={SleeveId}) on startup", sleeve.Name, sleeve.Id);

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
            bool exists = _sleeves.ContainsKey(sleeve.Id);

            _sleeves.AddOrUpdate(sleeve.Id, sleeve, (key, existing) => sleeve);

            if (exists)
            {
                _telemetryDeviceManager.UpdatePortsForSleeve(sleeve.Id, sleeve.PortNumbers);
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

        public void RemoveSleeve(int id)
        {
            if (_sleeves.TryRemove(id, out _))
            {
                _telemetryDeviceManager.RemoveTelemetryDevice(id);
            }
        }

        public DeviceManagerSleeveDto GetSleeve(int id)
        {
            _sleeves.TryGetValue(id, out DeviceManagerSleeveDto sleeve);
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
