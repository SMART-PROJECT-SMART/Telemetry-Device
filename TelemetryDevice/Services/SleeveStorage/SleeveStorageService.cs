using System.Collections.Concurrent;
using TelemetryDevices.Dto.DeviceManager;
using TelemetryDevices.Services.DeviceManagerClient;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Services.SleeveStorage
{
    public class SleeveStorageService : ISleeveStorageService, IHostedService
    {
        private readonly ConcurrentDictionary<string, DeviceManagerSleeveDto> _sleeves;
        private readonly IDeviceManagerClient _deviceManagerClient;
        private readonly IPacketSniffer _packetSniffer;

        public SleeveStorageService(IDeviceManagerClient deviceManagerClient, IPacketSniffer packetSniffer)
        {
            _sleeves = new ConcurrentDictionary<string, DeviceManagerSleeveDto>();
            _deviceManagerClient = deviceManagerClient;
            _packetSniffer = packetSniffer;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<DeviceManagerSleeveDto> sleeves = await _deviceManagerClient.GetAllSleevesAsync(cancellationToken);

            foreach (DeviceManagerSleeveDto sleeve in sleeves)
            {
                _sleeves.TryAdd(sleeve.Name, sleeve);
                foreach (int port in sleeve.PortNumbers)
                {
                    _packetSniffer.AddPort(port);
                }
            }
        }

        public void AddOrUpdateSleeve(DeviceManagerSleeveDto sleeve)
        {
            if (_sleeves.TryGetValue(sleeve.Name, out DeviceManagerSleeveDto existingSleeve))
            {
                foreach (int port in existingSleeve.PortNumbers)
                {
                    _packetSniffer.RemovePort(port);
                }
            }

            _sleeves.AddOrUpdate(sleeve.Name, sleeve, (key, existing) => sleeve);

            foreach (int port in sleeve.PortNumbers)
            {
                _packetSniffer.AddPort(port);
            }
        }

        public void RemoveSleeve(string name)
        {
            if (_sleeves.TryRemove(name, out DeviceManagerSleeveDto removedSleeve))
            {
                foreach (int port in removedSleeve.PortNumbers)
                {
                    _packetSniffer.RemovePort(port);
                }
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
