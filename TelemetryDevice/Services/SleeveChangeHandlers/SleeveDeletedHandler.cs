using TelemetryDevices.Services.SleeveStorage;

namespace TelemetryDevices.Services.SleeveChangeHandlers
{
    public class SleeveDeletedHandler : ISleeveChangeHandler
    {
        private readonly ISleeveStorageService _sleeveStorageService;

        public SleeveDeletedHandler(ISleeveStorageService sleeveStorageService)
        {
            _sleeveStorageService = sleeveStorageService;
        }

        public Task HandleSleeveChangeAsync(string name, CancellationToken cancellationToken = default)
        {
            _sleeveStorageService.RemoveSleeve(name);
            return Task.CompletedTask;
        }
    }
}
