using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.Builders
{
    public class PipelineDirector
    {
        private readonly IPipelineBuilder _builder;
        
        public PipelineDirector(IPipelineBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }
        
        public void BuildMinimalViablePipeline()
        {
            this._builder.BuildValidator();
        }
        
        public void BuildTelemetryPipeline()
        {
            this._builder.BuildValidator();
            this._builder.BuildDecoder();
            this._builder.BuildOutputHandler();
        }
    }
}
