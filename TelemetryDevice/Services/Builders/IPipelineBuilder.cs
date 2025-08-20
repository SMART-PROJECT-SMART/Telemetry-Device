using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.Builders;

public interface IPipeLineBuilder
{
    void AddValidator();
    void AddDecoder();
    void AddOutputHandler();
    void Reset();
    IPipeLine GetProduct();
}
