namespace TelemetryDevice.Services.PipeLines
{
    public interface IPipeLine
    {
        public Task ProcessDataAsync(byte[] data);
    }
}
