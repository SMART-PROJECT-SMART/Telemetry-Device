namespace TelemetryDevices.Models
{
    public struct ValidationResult
    {
        public bool IsValid;
        public byte[] Data;

        public ValidationResult(bool isValid, byte[] data)
        {
            IsValid = isValid;
            Data = data;
        }
    }
}
