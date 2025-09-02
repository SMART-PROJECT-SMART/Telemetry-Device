using Shared.Common.Enums;
using TelemetryDevices.Common.Enums;
using TelemetryDevices.Services.PipeLines;
using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.Factories.PipeLineFactory
{
    public class PipeLineFactory : IPipeLineFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PipeLineFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPipeLine GetPipeLine(CommunicationDataType communicationDataType)
        {
            return communicationDataType switch
            {
                CommunicationDataType.Telemetry => _serviceProvider.GetRequiredService<TelemetryPipeLine>(),
                _ => throw new NotImplementedException($"PipeLine for {communicationDataType} is not implemented.")
            };
        }

        public IPipeLine GetPipeLine(ICD telemetryIcd)
        {
            return GetPipeLine(telemetryIcd.DataType);
        }
    }
}
