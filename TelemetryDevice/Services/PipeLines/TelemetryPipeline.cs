using System.Threading.Tasks.Dataflow;
using Core.Models.ICDModels;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

namespace TelemetryDevices.Services.PipeLines
{
    public class TelemetryPipeline : ITelemetryPipeLine
    {
        private readonly ITelemetryValidatorBlock _telemetryValidatorBlock;
        private readonly ITelemetryDecoderBlock _telemetryDecoderBlock;
        private readonly ITelemetryOutputBlock _telemetryOutputBlock;
        private TransformBlock<byte[], ValidationResult> _pipelineValidatorBlock;
        private TransformBlock<ValidationResult, DecodingResult> _pipelineDecoderBlock;
        private ActionBlock<DecodingResult> _pipelineOutputBlock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;

        public TelemetryPipeline(ITelemetryValidatorBlock telemetryValidatorBlock, ITelemetryDecoderBlock telemetryDecoderBlock, ITelemetryOutputBlock telemetryOutputBlock)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _telemetryValidatorBlock = telemetryValidatorBlock;
            _telemetryDecoderBlock = telemetryDecoderBlock;
            _telemetryOutputBlock = telemetryOutputBlock;
        }

        public void BuildPipelineBlocks(ICD telemetryIcd)
        {
            _pipelineValidatorBlock = new TransformBlock<byte[], ValidationResult>(
                data => _telemetryValidatorBlock.ValidateTelemetryData(data, telemetryIcd),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _pipelineDecoderBlock = new TransformBlock<ValidationResult, DecodingResult>(
                validationResult => _telemetryDecoderBlock.DecodeTelemetryData(validationResult, telemetryIcd),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _pipelineOutputBlock = new ActionBlock<DecodingResult>(
                decodingResult => _telemetryOutputBlock.OutputTelemetryData(decodingResult, telemetryIcd),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });
            LinkTelemetryPipelineBlocks();
        }

        public async Task ProcessTelemetryDataAsync(byte[] telemetryData)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(TelemetryPipeline));

            bool posted = await _pipelineValidatorBlock.SendAsync(
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
                validationResult => validationResult.IsValid);
            _pipelineValidatorBlock.LinkTo(DataflowBlock.NullTarget<ValidationResult>());
            _pipelineDecoderBlock.LinkTo(_pipelineOutputBlock, new DataflowLinkOptions 
            { 
                PropagateCompletion = true 
            });
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _cancellationTokenSource.Cancel();
            _pipelineValidatorBlock.Complete();
            _cancellationTokenSource.Dispose();
            _isDisposed = true;
        }
    }
}
