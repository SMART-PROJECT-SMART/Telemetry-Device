using System.Threading.Tasks.Dataflow;
using Core.Models.ICDModels;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

namespace TelemetryDevices.Services.PipeLines
{
    public class Pipeline : IPipeLine, IDisposable
    {
        private readonly IValidatorBlock _validatorBlock;
        private readonly ITelemetryDecoderBlock _decoderBlock;
        private readonly IOutputBlock _outputBlock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;
        public ICD TelemetryICD { get; private set; }

        public Pipeline(IValidatorBlock validatorBlock, ITelemetryDecoderBlock decoderBlock, IOutputBlock outputBlock, ICD telemetryIcd)
        {
            _validatorBlock = validatorBlock;
            _decoderBlock = decoderBlock;
            _outputBlock = outputBlock;
            TelemetryICD = telemetryIcd;
            _cancellationTokenSource = new CancellationTokenSource();
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
