using TelemetryDevices.Services.Builders;
using TelemetryDevices.Services.Helpers.Decoder;
using TelemetryDevices.Services.Helpers.Output;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;

public class PipeLineBuilder : IPipeLineBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private IValidator _validator;
    private ITelemetryDecoder _decoder;
    private IOutputHandler _outputHandler;

    public PipeLineBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Reset();
    }

    public void Reset()
    {
        _validator = null;
        _decoder = null;
        _outputHandler = null;
    }

    public void BuildValidator()
    {
        _validator = _serviceProvider.GetRequiredService<IValidator>();
    }

    public void BuildDecoder()
    {
        _decoder = _serviceProvider.GetRequiredService<ITelemetryDecoder>();
    }

    public void BuildOutputHandler()
    {
        _outputHandler = _serviceProvider.GetRequiredService<IOutputHandler>();
    }

    public IPipeLine GetProduct()
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<TelemetryPipeLine>>();
        IPipeLine result = new TelemetryPipeLine(_validator, _decoder, _outputHandler, logger);
        Reset();
        return result;
    }
}