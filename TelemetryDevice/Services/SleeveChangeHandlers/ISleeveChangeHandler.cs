namespace TelemetryDevices.Services.SleeveChangeHandlers
{
    public interface ISleeveChangeHandler
    {
        Task HandleSleeveChangeAsync(int id, CancellationToken cancellationToken = default);
    }
}
