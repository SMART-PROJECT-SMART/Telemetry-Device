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
        private readonly IValidator _validator;
        private readonly ITelemetryDecoder _telemetryDecoder;
        private readonly IOutputHandler _outputHandler;
        private readonly TransformBlock<byte[], ValidationResult> _validatorBlock;
        private readonly TransformBlock<ValidationResult, DecodingResult> _decoderBlock;
        private readonly ActionBlock<DecodingResult> _outputBlock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;
        public ICD TelemetryICD { get; private set; }

        public Pipeline(IValidator validator, ITelemetryDecoder decoder, IOutputHandler outputHandler, ICD telemetryIcd)
        {
            _validator = validator;
            _telemetryDecoder = decoder;
            _outputHandler = outputHandler;
            TelemetryICD = telemetryIcd;
            _cancellationTokenSource = new CancellationTokenSource();

            _validatorBlock = new TransformBlock<byte[], ValidationResult>(
                data => _validator.Validate(data, TelemetryICD),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _decoderBlock = new TransformBlock<ValidationResult, DecodingResult>(
                validationResult => _telemetryDecoder.DecodeData(validationResult, TelemetryICD),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _outputBlock = new ActionBlock<DecodingResult>(
                async decodingResult => _outputHandler.HandleOutput(decodingResult, TelemetryICD),
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

            var posted = await _validatorBlock.SendAsync(
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
            _validatorBlock.LinkTo(_decoderBlock, 
                new DataflowLinkOptions { PropagateCompletion = true }, 
                result => result.IsValid);
            _validatorBlock.LinkTo(DataflowBlock.NullTarget<ValidationResult>());
            _decoderBlock.LinkTo(_outputBlock, new DataflowLinkOptions 
            { 
                PropagateCompletion = true 
            });
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _cancellationTokenSource.Cancel();
            _validatorBlock?.Complete();
            _decoderBlock?.Complete();
            _outputBlock?.Complete();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}
