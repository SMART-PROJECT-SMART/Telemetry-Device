namespace TelemetryDevices.Services.PipeLines.Blocks.Builders;

public interface IPipeLineBuilder
{
    void AddValidator();
    void AddDecoder();
    void AddOutputHandler();
    void Reset();
    IPipeLine GetProduct();
}
