using TelemetryDevice.Common.Enums;

namespace TelemetryDevice.Services.Helpers.Decoder
{
    public class TelemetryDataDecoder : IDecoder<TelemetryFields>
    {
        public Dictionary<TelemetryFields, double> DecodeData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
