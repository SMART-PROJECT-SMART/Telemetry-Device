using System.Collections.Concurrent;
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

        public SleeveStorageService(
            IDeviceManagerClient deviceManagerClient,
            ITelemetryDeviceManager telemetryDeviceManager)
        {
            _sleeves = new ConcurrentDictionary<string, DeviceManagerSleeveDto>();
            _deviceManagerClient = deviceManagerClient;
            _telemetryDeviceManager = telemetryDeviceManager;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<DeviceManagerSleeveDto> sleeves = await _deviceManagerClient.GetAllSleevesAsync(cancellationToken);

            foreach (DeviceManagerSleeveDto sleeve in sleeves)
            {
                _sleeves.TryAdd(sleeve.Name, sleeve);

                await _telemetryDeviceManager.AddTelemetryDeviceAsync(
                    sleeve.Name,
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
