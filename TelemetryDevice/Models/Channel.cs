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
        public ITelemetryPipeLine TelemetryPipeLine { get; set; }
        public ICD ICD { get; set; }

        public Channel(int portNumber, ICD icd, ITelemetryValidatorBlock telemetryValidatorBlock, ITelemetryDecoderBlock telemetryDecoderBlock, ITelemetryOutputBlock telemetryOutputBlock)
        {
            PortNumber = portNumber;
            ICD = icd;
            TelemetryPipeLine = new TelemetryPipeline(telemetryValidatorBlock, telemetryDecoderBlock, telemetryOutputBlock, icd);
        }


        public void ProcessTelemetryData(byte[] telemetryData)
        {
            _ = TelemetryPipeLine.ProcessTelemetryDataAsync(telemetryData);
        }
    }
}
