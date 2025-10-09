using Core.Common.Enums;

namespace TelemetryDevices.Models
{
    public struct DecodingResult
    {
        public IEnumerable<KeyValuePair<TelemetryFields, double>> DecodedFields { get; set; }

        public DecodingResult(IEnumerable<KeyValuePair<TelemetryFields, double>> decodedFields)
        {
            DecodedFields = decodedFields;
        }

        public DecodingResult(Dictionary<TelemetryFields, double> decodedFields)
        {
            DecodedFields = decodedFields;
        }

        public Dictionary<TelemetryFields, double> ToDictionary()
        {
            return DecodedFields?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                ?? new Dictionary<TelemetryFields, double>();
        }

        public double? GetValue(TelemetryFields field)
        {
            return DecodedFields?.FirstOrDefault(kvp => kvp.Key == field).Value;
        }

        public bool ContainsField(TelemetryFields field)
        {
            return DecodedFields?.Any(kvp => kvp.Key == field) ?? false;
        }
    }
}
