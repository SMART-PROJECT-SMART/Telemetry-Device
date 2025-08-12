using Shared.Common.Enums;

namespace TelemetryDevice.Services.Helpers.Decoder
{
    public interface ITelemetryDecoder
    {
        public Dictionary<TelemetryFields, double> DecodeData(byte[] data);
    }
}
