namespace TelemetryDevice.Common
{
    public static class TelemetryDeviceConstants
    {
        public static class Network
        {
            public const int DeviceTimeoutMs = 1000;
            
            public const string UdpFilter = "udp";
            public const string UdpPortFilter = "udp and ({0})";
            public const string DestinationPortFilter = "dst port {0}";
            public const string FilterSeparator = " or ";
        }

        public static class LoopbackInterface
        {
            public const string LoopbackDescription = "Loopback";
            public const string LoopbackName = "Loopback";
            public const string LoopbackLo0 = "lo0";
            public const string LoopbackNpfDevice = "\\Device\\NPF_Loopback";
        }

        public static class PacketProcessing
        {
            public const int MaxHexPreviewLength = 32;
            public const string HexPreviewSuffix = "...";
            public const string UnknownAddress = "Unknown";
        }

        public static class DeviceIndex
        {
            public const int FirstDevice = 0;
        }
    }
}
