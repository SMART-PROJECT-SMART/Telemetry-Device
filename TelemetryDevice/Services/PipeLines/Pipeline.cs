using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

namespace TelemetryDevices.Services.PipeLines
{
    public class Pipeline : IPipeLine, IDisposable
    {
        private readonly IValidator _validatorBlock;
        private readonly ITelemetryDecoder _decoderBlock;
        private readonly IOutputHandler _outputBlock;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _disposed;
        public ICD TelemetryICD { get; private set; }

        public Pipeline(IValidator validatorBlock,ITelemetryDecoder decoderBlock,IOutputHandler outputBlock, ICD telemetryIcd)
        {
            _validatorBlock = validatorBlock;
            _decoderBlock = decoderBlock;
            _outputBlock = outputBlock;
            TelemetryICD = telemetryIcd;
            LinkTelemetryPipelineBlocks();
        }

        public async Task ProcessTelemetryDataAsync(byte[] telemetryData)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Pipeline));

            if (_telemetryPipelineBlocks.First() is not ITargetBlock<byte[]> firstTelemetryBlock)
            {
                throw new InvalidOperationException(
                    "First telemetry block does not accept byte[] input"
                );
            }

            var posted = await firstTelemetryBlock.SendAsync(
                telemetryData,
                _cancellationTokenSource.Token
            );
            if (!posted)
            {
                throw new InvalidOperationException("Failed to post data to pipeline");
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
            _validatorBlock
            for (int blockIndex = 0; blockIndex < _telemetryPipelineBlocks.Count - 1; blockIndex++)
            {
                IDataflowBlock currentTelemetryBlock = _telemetryPipelineBlocks[blockIndex];
                IDataflowBlock nextTelemetryBlock = _telemetryPipelineBlocks[blockIndex + 1];

                switch (currentTelemetryBlock)
                {
                    case TransformBlock<byte[], DecodingResult> validationBlock
                        when nextTelemetryBlock
                            is TransformBlock<
                                DecodingResult,
                                Dictionary<TelemetryFields, double>
                            > decoderBlockNext:
                        validationBlock.LinkTo(decoderBlockNext, result => result.IsValid);
                        validationBlock.LinkTo(
                            DataflowBlock.NullTarget<DecodingResult>(),
                            result => !result.IsValid
                        );
                        break;
                    case TransformBlock<
                        DecodingResult,
                        Dictionary<TelemetryFields, double>
                    > decoderBlockCurrent
                        when nextTelemetryBlock
                            is ActionBlock<Dictionary<TelemetryFields, double>> outputBlock:
                        decoderBlockCurrent.LinkTo(outputBlock);
                        break;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _cancellationTokenSource.Cancel();

            foreach (var block in _telemetryPipelineBlocks.OfType<IDisposable>())
            {
                block.Dispose();
            }

            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}
