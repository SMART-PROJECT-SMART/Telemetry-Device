namespace TelemetryDevices.Models
{
    public struct DecodingResult
    {
        public bool IsValid;
        public byte[] Data;

        public DecodingResult(bool isValid, byte[] data)
        {
            IsValid = isValid;
            Data = data;
        }
    }
}
