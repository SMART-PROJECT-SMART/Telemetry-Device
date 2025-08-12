namespace TelemetryDevice.Services.Helpers
{
    public interface IValidator
    {
        public bool Validate(byte[] compressedData);
    }
}
