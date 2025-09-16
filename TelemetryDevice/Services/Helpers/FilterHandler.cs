using System.Text;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Helpers
{
    public static class FilterHandler
    {
        public static string BuildProtocolFilter(List<string> supportedProtocols)
        {
            return supportedProtocols.Count switch
            {
                0 => TelemetryDeviceConstants.Network.UDP_FILTER,
                1 => supportedProtocols[0],
                _ =>
                    $"({string.Join(TelemetryDeviceConstants.Network.FILTER_SEPARATOR, supportedProtocols)})",
            };
        }

        public static string BuildFilterFromPorts(
            IReadOnlyCollection<int> monitoredPorts,
            string baseProtocolFilter
        )
        {
            if (monitoredPorts.Count == 0)
                return baseProtocolFilter;

            var portFilters = monitoredPorts
                .OrderBy(port => port)
                .Select(port =>
                    string.Format(TelemetryDeviceConstants.Network.DESTINATION_PORT_FILTER, port)
                );

            return $"{baseProtocolFilter}{TelemetryDeviceConstants.Network.AND_SEPERATOR}"
                + $"{string.Join(TelemetryDeviceConstants.Network.FILTER_SEPARATOR, portFilters)}"
                + $"{TelemetryDeviceConstants.Network.AND_SEPERATOR_END}";
        }
    }
}
