using System.Collections.Concurrent;
using TelemetryDevices.Dto.DeviceManager;
using TelemetryDevices.Services.DeviceManagerClient;

namespace TelemetryDevices.Services.SleeveStorage
{
    public class SleeveStorageService : ISleeveStorageService, IHostedService
    {
        private readonly ConcurrentDictionary<string, DeviceManagerSleeveDto> _sleeves;
        private readonly IDeviceManagerClient _deviceManagerClient;

        public SleeveStorageService(IDeviceManagerClient deviceManagerClient)
        {
            _sleeves = new ConcurrentDictionary<string, DeviceManagerSleeveDto>();
            _deviceManagerClient = deviceManagerClient;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<DeviceManagerSleeveDto> sleeves = await _deviceManagerClient.GetAllSleevesAsync(cancellationToken);

            foreach (DeviceManagerSleeveDto sleeve in sleeves)
            {
                _sleeves.TryAdd(sleeve.Name, sleeve);
            }
        }

        public void AddOrUpdateSleeve(DeviceManagerSleeveDto sleeve)
        {
            _sleeves.AddOrUpdate(sleeve.Name, sleeve, (key, existing) => sleeve);
        }

        public void RemoveSleeve(string name)
        {
            _sleeves.TryRemove(name, out _);
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
