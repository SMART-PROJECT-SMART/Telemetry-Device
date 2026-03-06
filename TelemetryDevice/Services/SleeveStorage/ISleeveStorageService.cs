using TelemetryDevices.Dto.DeviceManager;

namespace TelemetryDevices.Services.SleeveStorage
{
    public interface ISleeveStorageService
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task AddOrUpdateSleeveAsync(DeviceManagerSleeveDto sleeve);
        void RemoveSleeve(int id);
        DeviceManagerSleeveDto GetSleeve(int id);
        IEnumerable<DeviceManagerSleeveDto> GetAllSleeves();
    }
}
