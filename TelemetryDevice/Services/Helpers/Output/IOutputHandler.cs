using Shared.Common.Enums;

namespace TelemetryDevices.Services.Helpers.Output
{
    public interface IOutputHandler
    {
        void HandleOutput(Dictionary<TelemetryFields, double> decodedData);

    }
}
