using TelemetryDevices.Dto.DeviceManager;
using TelemetryDevices.Services.DeviceManagerClient;
using TelemetryDevices.Services.SleeveStorage;

namespace TelemetryDevices.Services.SleeveChangeHandlers
{
    public class SleeveCreatedHandler : ISleeveChangeHandler
    {
        private readonly ISleeveStorageService _sleeveStorageService;
        private readonly IDeviceManagerClient _deviceManagerClient;

        public SleeveCreatedHandler(ISleeveStorageService sleeveStorageService, IDeviceManagerClient deviceManagerClient)
        {
            _sleeveStorageService = sleeveStorageService;
            _deviceManagerClient = deviceManagerClient;
        }

        public async Task HandleSleeveChangeAsync(int id, CancellationToken cancellationToken = default)
        {
            IEnumerable<DeviceManagerSleeveDto> allSleeves = await _deviceManagerClient.GetAllSleevesAsync(cancellationToken);
            DeviceManagerSleeveDto sleeve = allSleeves.FirstOrDefault(s => s.Id == id);

            if (sleeve != null)
            {
                await _sleeveStorageService.AddOrUpdateSleeveAsync(sleeve);
            }
        }
    }
}
