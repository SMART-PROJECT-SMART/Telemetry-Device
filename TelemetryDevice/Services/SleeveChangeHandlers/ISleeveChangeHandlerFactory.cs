using Core.Common.Enums;

namespace TelemetryDevices.Services.SleeveChangeHandlers
{
    public interface ISleeveChangeHandlerFactory
    {
        ISleeveChangeHandler CreateHandler(CrudOperation operation);
    }
}
