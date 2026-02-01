using TelemetryDevices.Dto.DeviceManager;

namespace TelemetryDevices.Services.SleeveStorage
{
    public interface ISleeveStorageService
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        void AddOrUpdateSleeve(DeviceManagerSleeveDto sleeve);
        void RemoveSleeve(string name);
        DeviceManagerSleeveDto GetSleeve(string name);
        IEnumerable<DeviceManagerSleeveDto> GetAllSleeves();
    }
}
