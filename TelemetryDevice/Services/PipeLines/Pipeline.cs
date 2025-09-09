using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines
{
    public class Pipeline : IPipeLine
    {
        private readonly List<IDataflowBlock> _telemetryPipelineBlocks;
        public ICD TelemetryICD { get; private set; }

        public Pipeline(List<IDataflowBlock> telemetryPipelineBlocks, ICD telemetryIcd)
        {
            _telemetryPipelineBlocks = telemetryPipelineBlocks;
            TelemetryICD = telemetryIcd;
            LinkTelemetryPipelineBlocks();
        }

        public Task ProcessTelemetryDataAsync(byte[] telemetryData)
        {
                var firstTelemetryBlock = _telemetryPipelineBlocks.First() as TransformBlock<byte[], DecodingResult>;
                if (firstTelemetryBlock == null)
                {
                    throw new InvalidOperationException("First telemetry block is not a valid TransformBlock<byte[], DecodingResult>");
                }
                
                var posted = firstTelemetryBlock.Post(telemetryData);
                
                return Task.CompletedTask;
        }

        public void SetTelemetryICD(ICD telemetryIcd)
        {
            TelemetryICD = telemetryIcd;
        }

        public ICD GetTelemetryICD()
        {
            return TelemetryICD;
        }

        private void LinkTelemetryPipelineBlocks()
        {
                for (int blockIndex = 0; blockIndex < _telemetryPipelineBlocks.Count - 1; blockIndex++)
                {
                    IDataflowBlock currentTelemetryBlock = _telemetryPipelineBlocks[blockIndex];
                    IDataflowBlock nextTelemetryBlock = _telemetryPipelineBlocks[blockIndex + 1];

                    switch (currentTelemetryBlock)
                    {
                        case TransformBlock<byte[], DecodingResult> validationBlock when
                            nextTelemetryBlock is TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> decoderBlockNext:
                            validationBlock.LinkTo(decoderBlockNext, result => result.IsValid);
                            validationBlock.LinkTo(DataflowBlock.NullTarget<DecodingResult>(), result => !result.IsValid);
                            break;
                        case TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> decoderBlockCurrent when
                            nextTelemetryBlock is ActionBlock<Dictionary<TelemetryFields, double>> outputBlock:
                            decoderBlockCurrent.LinkTo(outputBlock);
                            break;
                    }
                }
        }
    }
}
