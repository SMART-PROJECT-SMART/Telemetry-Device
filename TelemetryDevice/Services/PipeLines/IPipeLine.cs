using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines
{
    public interface IPipeLine
    {
        public Task ProcessDataAsync(byte[] data);
        public void SetICD(ICD icd);
        public ICD GetICD();
    }
}
