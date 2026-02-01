namespace TelemetryDevices.Services.SleeveChangeHandlers
{
    public interface ISleeveChangeHandler
    {
        Task HandleSleeveChangeAsync(string name, CancellationToken cancellationToken = default);
    }
}
