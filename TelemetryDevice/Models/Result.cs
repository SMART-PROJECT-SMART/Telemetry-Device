namespace TelemetryDevice.Models
{
    public struct Result
    {
        public bool IsValid;
        public byte[] Data;

        public Result(bool isValid, byte[] data)
        {
            IsValid = isValid;
            Data = data;
        }
    }
}
