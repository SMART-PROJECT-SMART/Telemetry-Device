using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;
using TelemetryDevices.Services.Kafka.Producers;

namespace TelemetryDevices.Models
{
    public class Channel
    {
        public int PortNumber { get; set; }
        public IPipeLine PipeLine { get; set; }
        public ICD ICD { get; set; }

        public Channel(int portNumber, ICD icd, IValidator validator, ITelemetryDecoder telemetryDecoder, IOutputHandler outputHandler)
        {
            PortNumber = portNumber;
            ICD = icd;
            PipeLine = new Pipeline(validator, telemetryDecoder, outputHandler, icd);
        }


        public void ProcessTelemetryData(byte[] telemetryData)
        {
            _ = PipeLine.ProcessTelemetryDataAsync(telemetryData);
        }
    }
}
