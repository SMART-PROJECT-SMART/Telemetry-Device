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
            try
            {
                var firstTelemetryBlock = _telemetryPipelineBlocks.First() as TransformBlock<byte[], DecodingResult>;
                if (firstTelemetryBlock == null)
                {
                    throw new InvalidOperationException("First telemetry block is not a valid TransformBlock<byte[], DecodingResult>");
                }
                
                // Post the data to the first block - this will trigger the pipeline execution
                var posted = firstTelemetryBlock.Post(telemetryData);
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw; // Re-throw to allow proper error handling upstream
            }
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
            try
            {
                for (int blockIndex = 0; blockIndex < _telemetryPipelineBlocks.Count - 1; blockIndex++)
                {
                    var currentTelemetryBlock = _telemetryPipelineBlocks[blockIndex];
                    var nextTelemetryBlock = _telemetryPipelineBlocks[blockIndex + 1];

                    if (currentTelemetryBlock is TransformBlock<byte[], DecodingResult> validationBlock &&
                        nextTelemetryBlock is TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> decoderBlockNext)
                    {
                        validationBlock.LinkTo(decoderBlockNext, result => result.IsValid);
                        validationBlock.LinkTo(DataflowBlock.NullTarget<DecodingResult>(), result => !result.IsValid);
                    }
                    else if (currentTelemetryBlock is TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> decoderBlockCurrent &&
                             nextTelemetryBlock is ActionBlock<Dictionary<TelemetryFields, double>> outputBlock)
                    {
                        decoderBlockCurrent.LinkTo(outputBlock);
                    }
                }
            }
            catch (Exception ex)
            {
                throw; // Re-throw to prevent pipeline creation with broken links
            }
        }
    }
}
