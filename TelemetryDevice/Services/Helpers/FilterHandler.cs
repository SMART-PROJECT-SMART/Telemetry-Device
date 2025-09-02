using System.Text;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Helpers
{
    public static class FilterHandler
    {
        public static string BuildProtocolFilter(List<string> supportedProtocols)
        {
            if (supportedProtocols.Count == 0)
                return TelemetryDeviceConstants.Network.UDP_FILTER;

            if (supportedProtocols.Count == 1)
                return supportedProtocols[0];

            return $"({string.Join(TelemetryDeviceConstants.Network.FILTER_SEPARATOR, supportedProtocols)})";
        }

        public static string BuildFilterFromPorts(IReadOnlyCollection<int> monitoredPorts, string baseProtocolFilter)
        {
            if (monitoredPorts.Count == 0)
                return baseProtocolFilter;

            var sortedPortNumbers = monitoredPorts.OrderBy(portNumber => portNumber);

            var filterStringBuilder = new StringBuilder();
            filterStringBuilder.Append(baseProtocolFilter);
            filterStringBuilder.Append(TelemetryDeviceConstants.Network.AND_SEPERATOR);
            bool isFirstPort = true;
            foreach (var currentPortNumber in sortedPortNumbers)
            {
                if (!isFirstPort)
                    filterStringBuilder.Append(TelemetryDeviceConstants.Network.FILTER_SEPARATOR);
                filterStringBuilder.Append(
                    string.Format(TelemetryDeviceConstants.Network.DESTINATION_PORT_FILTER, currentPortNumber)
                );
                isFirstPort = false;
            }
            filterStringBuilder.Append(TelemetryDeviceConstants.Network.AND_SEPERATOR_END);
            return filterStringBuilder.ToString();
        }
    }
}
