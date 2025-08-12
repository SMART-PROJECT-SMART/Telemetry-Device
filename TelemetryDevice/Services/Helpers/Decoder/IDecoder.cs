namespace TelemetryDevice.Services.Helpers.Decoder
{
    public interface IDecoder<TEnum> where TEnum : Enum
    {
        public Dictionary<TEnum, double> DecodeData(byte[] data);
    }
}
