
namespace TelemetryDevice.Services.Helpers.Decoder
{
    public class TelemetryDataDecoder : ITelemetryDecoder
    {
        private readonly Dictionary<TelemetryFields, double> _decodedData= new Dictionary<TelemetryFields, double>();
        public Dictionary<TelemetryFields, double> DecodeData(byte[] data)
        {
            foreach (var field in Enum.GetValues<TelemetryFields>())
            {
                _decodedData[field] = 1;
            }
            return _decodedData;
        }
    }
}
