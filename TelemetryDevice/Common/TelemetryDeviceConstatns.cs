namespace TelemetryDevice.Common
{
    public static class TelemetryDeviceConstants
    {
        public static class Network
        {
            public const string UDP_FILTER = "udp";
            public const string TCP_FILTER = "tcp";
            public const string ICMP_FILTER = "icmp";
            public const string UDP_PORT_FILTER = "udp and ({0})";
            public const string DESTINATION_PORT_FILTER = "dst port {0}";
            public const string FILTER_SEPARATOR = " or ";
            public const string INTERFACE_FILTER = "interface \"{0}\"";
        }

        public static class LoopbackInterface
        {
            public const string LOOPBACK_DESCRIPTION = "Loopback";
        }

        public static class PacketProcessing
        {
            public const int MAX_HEX_PREVIEW_LENGTH = 32;
            public const string HEX_PREVIEW_SUFFIX = "...";
        }

        public static class Configuration
        {
            public const string NETWORKING_SECTION = "Networking";
            public const string DEFAULT_PROTOCOL = "udp";
            public const string DEFAULT_INTERFACE = "loopback";
        }
    }
}