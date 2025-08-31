using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.PipeLines.Blocks.Builders;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

public class PipeLineBuilder : IPipeLineBuilder
{
    private readonly IValidator _validator;
    private readonly ITelemetryDecoder _decoder;
    private readonly IOutputHandler _outputHandler;
    private readonly ILogger<TelemetryPipeLine> _logger;
    private List<IPipelineComponent> _components = new List<IPipelineComponent>();

    public PipeLineBuilder(
        IValidator validator,
        ITelemetryDecoder decoder,
        IOutputHandler outputHandler,
        ILogger<TelemetryPipeLine> logger
    )
    {
        _validator = validator;
        _decoder = decoder;
        _outputHandler = outputHandler;
        _logger = logger;
        Reset();
    }

    public void Reset()
    {
        _components = new List<IPipelineComponent>();
    }

    public void AddValidator()
    {
        _components.Add(_validator);
    }

    public void AddDecoder()
    {
        _components.Add(_decoder);
    }

    public void AddOutputHandler()
    {
        _components.Add(_outputHandler);
    }

    public IPipeLine GetProduct()
    {
        IPipeLine result = new TelemetryPipeLine(
            [.._components],
            _logger
        );
        Reset();
        return result;
    }
}