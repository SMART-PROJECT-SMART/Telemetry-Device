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
        private readonly IValidator _validatorBlock;
        private readonly ITelemetryDecoder _decoderBlock;
        private readonly IOutputHandler _outputBlock;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _disposed;
        public ICD TelemetryICD { get; private set; }

        public Pipeline(IValidator validatorBlock, ITelemetryDecoder decoderBlock, IOutputHandler outputBlock, ICD telemetryIcd)
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
            _validatorBlock.LinkTo(_decoderBlock, new DataflowLinkOptions(), result => result.IsValid);
            _validatorBlock.LinkTo(DataflowBlock.NullTarget<DecodingResult>());
            _decoderBlock.LinkTo(_outputBlock, new DataflowLinkOptions());
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
