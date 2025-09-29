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

        public Channel(int portNumber, ICD icd, ITelemetryProducer telemetryProducer)
        {
            PortNumber = portNumber;
            ICD = icd;
            PipeLine = CreatePipeline(icd, telemetryProducer);
        }

        private Pipeline CreatePipeline(ICD icd, ITelemetryProducer telemetryProducer)
        {
            var validator = new ChecksumValidatorBlock(icd);
            var decoder = new TelemetryDecoderBlock(icd);
            var outputHandler = new KafkaOutputBlock(telemetryProducer, icd);

            return new Pipeline(validator, decoder, outputHandler, icd);
        }

        public void ProcessTelemetryData(byte[] telemetryData)
        {
            _ = PipeLine.ProcessTelemetryDataAsync(telemetryData);
        }
    }
}
