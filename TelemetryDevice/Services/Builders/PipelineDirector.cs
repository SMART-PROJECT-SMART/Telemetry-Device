using TelemetryDevices.Services.Builders;
using TelemetryDevices.Services.PipeLines;

public class PipeLineDirector
{
    private readonly IPipeLineBuilder _builder;

    public PipeLineDirector(IPipeLineBuilder builder)
    {
        _builder = builder;
    }

    public IPipeLine BuildTelemetryPipeline()
    {
        _builder.Reset();
        _builder.BuildValidator();
        _builder.BuildDecoder();
        _builder.BuildOutputHandler();
        return _builder.GetProduct();
    }

    public IPipeLine BuildDebugPipeline()
    {
        _builder.Reset();
        _builder.BuildValidator();
        _builder.BuildOutputHandler();
        return _builder.GetProduct();
    }

    public IPipeLine BuildMinimalPipeline()
    {
        _builder.Reset();
        _builder.BuildValidator();
        return _builder.GetProduct();
    }
}