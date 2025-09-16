using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Shared.Models.ICDModels;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

namespace TelemetryDevices.Services.PipeLines.Builder
{
    public class PipelineBuilder : IPipeLineBuilder
    {
        private readonly List<IDataflowBlock> _telemetryPipelineBlocks = new();
        private ICD _telemetryIcd;

        public PipelineBuilder() { }

        public IPipeLineBuilder AddValidator(IValidator telemetryValidator, ICD telemetryIcd)
        {
            _telemetryIcd = telemetryIcd;
            _telemetryPipelineBlocks.Add(telemetryValidator.GetBlock(telemetryIcd));
            return this;
        }

        public IPipeLineBuilder AddDecoder(ITelemetryDecoder telemetryDecoder, ICD telemetryIcd)
        {
            _telemetryIcd = telemetryIcd;
            _telemetryPipelineBlocks.Add(telemetryDecoder.GetBlock(telemetryIcd));
            return this;
        }

        public IPipeLineBuilder AddOutput(IOutputHandler telemetryOutputHandler, ICD telemetryIcd)
        {
            _telemetryIcd = telemetryIcd;
            _telemetryPipelineBlocks.Add(telemetryOutputHandler.GetBlock(telemetryIcd));
            return this;
        }

        public IPipeLineBuilder AddBlock(IDataflowBlock telemetryBlock)
        {
            _telemetryPipelineBlocks.Add(telemetryBlock);
            return this;
        }

        public Pipeline BuildPipeline()
        {
            if (_telemetryIcd == null)
            {
                throw new InvalidOperationException(
                    "Telemetry ICD must be set before building the pipeline."
                );
            }

            var telemetryPipelineBlocksCopy = new List<IDataflowBlock>(_telemetryPipelineBlocks);
            var telemetryIcdCopy = _telemetryIcd;

            var telemetryPipeline = new Pipeline(telemetryPipelineBlocksCopy, telemetryIcdCopy);

            Reset();

            return telemetryPipeline;
        }

        public void Reset()
        {
            _telemetryPipelineBlocks.Clear();
            _telemetryIcd = null;
        }
    }
}
