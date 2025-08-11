namespace TelemetryDevice.Common
{
    public static class TelemetryDeviceConstants
    {
        public static class Network
        {
            public const string UdpFilter = "udp";
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
