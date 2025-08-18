using Shared.Common.Enums;
using TelemetryDevices.Services.Helpers.Decoder;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.Builders
{
    public interface IPipeLineBuilder
    {
        void BuildValidator();
        void BuildDecoder();
        void BuildOutputHandler();
        void Reset();
        IPipeLine GetProduct();
    }
}
