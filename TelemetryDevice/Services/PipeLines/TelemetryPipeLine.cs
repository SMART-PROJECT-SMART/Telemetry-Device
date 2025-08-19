using Shared.Common.Enums;
using Shared.Models.ICDModels;
using System.Threading.Tasks.Dataflow;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Helpers.Decoder;
using TelemetryDevices.Services.Helpers.Output;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;

public class TelemetryPipeLine : IPipeLine
{
    private readonly IValidator _validator;
    private readonly ITelemetryDecoder _telemetryDecoder;
    private readonly IOutputHandler _outputHandler;
    private readonly List<IPipelineComponent> _components;
    private TransformBlock<byte[], Result> _validationBlock;
    private TransformBlock<Result, Dictionary<TelemetryFields, double>> _decodingBlock;
    private ActionBlock<Dictionary<TelemetryFields, double>> _outputBlock;
    private readonly ILogger<TelemetryPipeLine> _logger;
    public ICD ICD { get; set; }

    public TelemetryPipeLine(List<IPipelineComponent> components, ILogger<TelemetryPipeLine> logger)
    {
        _components = components;
        _validator = components.OfType<IValidator>().FirstOrDefault();
        _telemetryDecoder = components.OfType<ITelemetryDecoder>().FirstOrDefault();
        _outputHandler = components.OfType<IOutputHandler>().FirstOrDefault();
        _logger = logger;
    }

    public async Task ProcessDataAsync(byte[] data)
    {
        _validationBlock.Post(data);
        await _outputBlock.Completion;
    }

    public void SetICD(ICD icd)
    {
        ICD = icd;
        BuildPipeLine();
    }

    public ICD GetICD()
    {
        return ICD;
    }

    private void BuildPipeLine()
    {
        BuildValidationBlock();
        BuildDecodingBlock();
        BuildOutputBlock();
        LinkBlocks();
    }

    private void BuildValidationBlock()
    {
        _validationBlock = new TransformBlock<byte[], Result>(data =>
        {
            bool isValid = _validator.Validate(data);
            return new Result(isValid, data);
        });
    }

    private void BuildDecodingBlock()
    {
        _decodingBlock = new TransformBlock<Result, Dictionary<TelemetryFields, double>>(
            result => result.IsValid ? _telemetryDecoder.DecodeData(result.Data, ICD) : new Dictionary<TelemetryFields, double>()
        );
    }

    private void BuildOutputBlock()
    {
        _outputBlock = new ActionBlock<Dictionary<TelemetryFields, double>>(decodedData =>
        {
            _outputHandler.HandleOutput(decodedData);
        });
    }

    private void LinkBlocks()
    {
        _validationBlock.LinkTo(_decodingBlock, result => result.IsValid);
        _validationBlock.LinkTo(DataflowBlock.NullTarget<Result>(), result => !result.IsValid);
        _decodingBlock.LinkTo(_outputBlock);
    }

}
