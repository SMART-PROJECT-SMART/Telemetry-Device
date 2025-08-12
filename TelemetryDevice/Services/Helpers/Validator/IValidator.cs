namespace TelemetryDevice.Services.Helpers.Validator
{
    public interface IValidator
    {
        public bool Validate(byte[] compressedData);
    }
}
