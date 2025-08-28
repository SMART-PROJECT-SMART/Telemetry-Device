using TelemetryDevices.Common.Enums;

namespace TelemetryDevices.Services.PipeLines;

public interface IPipelineComponent
{
    PipeLineComponents ComponentType { get; }
}
