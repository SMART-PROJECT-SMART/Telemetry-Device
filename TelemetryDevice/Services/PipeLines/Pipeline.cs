using System.Threading.Tasks.Dataflow;
using Core.Models.ICDModels;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

namespace TelemetryDevices.Services.PipeLines
{
    public class Pipeline : IPipeLine
    {
        private readonly IValidatorBlock _validatorBlock;
        private readonly ITelemetryDecoderBlock _telemetryDecoderBlock;
        private readonly IOutputBlock _outputBlock;
        private readonly TransformBlock<byte[], ValidationResult> _pipelineValidatorBlock;
        private readonly TransformBlock<ValidationResult, DecodingResult> _pipelineDecoderBlock;
        private readonly ActionBlock<DecodingResult> _pipelineOutputBlock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;
        public ICD TelemetryICD { get;}

        public Pipeline(IValidatorBlock validatorBlock, ITelemetryDecoderBlock decoderBlock, IOutputBlock outputBlock, ICD telemetryIcd)
        {
            _validatorBlock = validatorBlock;
            _telemetryDecoderBlock = decoderBlock;
            _outputBlock = outputBlock;
            TelemetryICD = telemetryIcd;
            _cancellationTokenSource = new CancellationTokenSource();

            _pipelineValidatorBlock = new TransformBlock<byte[], ValidationResult>(
                data => _validatorBlock.Validate(data, TelemetryICD),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _pipelineDecoderBlock = new TransformBlock<ValidationResult, DecodingResult>(
                validationResult => _telemetryDecoderBlock.DecodeTelemetryData(validationResult, TelemetryICD),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _pipelineOutputBlock = new ActionBlock<DecodingResult>(
                decodingResult => _outputBlock.HandleOutput(decodingResult, TelemetryICD),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });


            LinkTelemetryPipelineBlocks();
        }
        public async Task ProcessTelemetryDataAsync(byte[] telemetryData)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Pipeline));

            var posted = await _pipelineValidatorBlock.SendAsync(
                telemetryData,
                _cancellationTokenSource.Token
            );
            if (!posted)
            {
                throw new InvalidOperationException("Failed to post data to pipeline");
            }
        }
        private void LinkTelemetryPipelineBlocks()
        {
            _pipelineValidatorBlock.LinkTo(_pipelineDecoderBlock, 
                new DataflowLinkOptions { PropagateCompletion = true }, 
                result => result.IsValid);
            _pipelineValidatorBlock.LinkTo(DataflowBlock.NullTarget<ValidationResult>());
            _pipelineDecoderBlock.LinkTo(_pipelineOutputBlock, new DataflowLinkOptions 
            { 
                PropagateCompletion = true 
            });
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _cancellationTokenSource.Cancel();
            _pipelineValidatorBlock.Complete();
            _pipelineDecoderBlock.Complete();
            _pipelineOutputBlock.Complete();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}
