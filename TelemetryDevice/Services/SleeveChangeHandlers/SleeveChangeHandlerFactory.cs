using Core.Common.Enums;
using TelemetryDevices.Services.DeviceManagerClient;
using TelemetryDevices.Services.SleeveStorage;

namespace TelemetryDevices.Services.SleeveChangeHandlers
{
    public class SleeveChangeHandlerFactory : ISleeveChangeHandlerFactory
    {
        private readonly ISleeveStorageService _sleeveStorageService;
        private readonly IDeviceManagerClient _deviceManagerClient;

        public SleeveChangeHandlerFactory(ISleeveStorageService sleeveStorageService, IDeviceManagerClient deviceManagerClient)
        {
            _sleeveStorageService = sleeveStorageService;
            _deviceManagerClient = deviceManagerClient;
        }

        public ISleeveChangeHandler CreateHandler(CrudOperation operation)
        {
            return operation switch
            {
                CrudOperation.Created => new SleeveCreatedHandler(_sleeveStorageService, _deviceManagerClient),
                CrudOperation.Updated => new SleeveUpdatedHandler(_sleeveStorageService, _deviceManagerClient),
                CrudOperation.Deleted => new SleeveDeletedHandler(_sleeveStorageService),
                _ => throw new ArgumentException($"Unsupported operation: {operation}", nameof(operation))
            };
        }
    }
}
