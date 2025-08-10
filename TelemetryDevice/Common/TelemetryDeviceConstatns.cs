namespace TelemetryDevice.Common
{
    public static class TelemetryDeviceConstants
    {
        public static class Network
        {
            public const string UdpFilter = "udp";
            public const string UdpPortFilter = "udp and ({0})";
            public const string DestinationPortFilter = "dst port {0}";
            public const string FilterSeparator = " or ";
        }

        public static class LoopbackInterface
        {
            public const string LoopbackDescription = "Loopback";
        }

        public static class PacketProcessing
        {
            public const int MaxHexPreviewLength = 32;
            public const string HexPreviewSuffix = "...";
        }
    }
}
