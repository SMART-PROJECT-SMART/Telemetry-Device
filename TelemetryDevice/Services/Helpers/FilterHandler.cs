using System.Text;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Helpers
{
    public static class FilterHandler
    {
        public static string BuildProtocolFilter(List<string> protocols)
        {
            if (protocols.Count == 0)
                return TelemetryDeviceConstants.Network.UDP_FILTER;

            if (protocols.Count == 1)
                return protocols[0];

            return $"({string.Join(TelemetryDeviceConstants.Network.FILTER_SEPARATOR, protocols)})";
        }

        public static string BuildFilterFromPorts(
            IReadOnlyCollection<int> ports,
            string baseFilter
        )
        {
            if (ports.Count == 0)
                return baseFilter;

            var ordered = ports.OrderBy(p => p);

            var sb = new StringBuilder();
            sb.Append(baseFilter);
            sb.Append(" and (");
            bool first = true;
            foreach (var p in ordered)
            {
                if (!first)
                    sb.Append(TelemetryDeviceConstants.Network.FILTER_SEPARATOR);
                sb.Append(
                    string.Format(TelemetryDeviceConstants.Network.DESTINATION_PORT_FILTER, p)
                );
                first = false;
            }
            sb.Append(')');
            return sb.ToString();
        }
    }
}