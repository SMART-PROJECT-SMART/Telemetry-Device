using Shared.Common.Enums;
using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.Helpers.Decoder
{
    public interface ITelemetryDecoder
    {
        public Dictionary<TelemetryFields, double> DecodeData(byte[] data, ICD icd);
    }
}
