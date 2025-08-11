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
            public const int BYTE_SIZE = 8;
            public const string FALLBACK_IP = "127.0.0.1";
            public const int STARTING_PORT_NUMBER = 8000;
            public const int MAX_PORT_NUMBER = 8999;
            public const int PORT_INCREMENT = 1;
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

        public static class TelemetryCompression
        {
            public const int BITS_PER_BYTE = 8;
            public const uint CHECKSUM_SEED = 0x5A5A5A5A;
            public const uint CHECKSUM_MULTIPLIER = 1103515245;
            public const uint CHECKSUM_INCREMENT = 12345;
            public const uint CHECKSUM_MODULO = 0xFFFFFFFF;
            public const int CHECKSUM_BITS = 32;
            public const ulong BIT_SHIFT_BASE = 1UL;
            public const int ICD_BITS = 328;   
            public const int SIGN_BITS = 21;  
            public const int PADDING_BITS = 3;
        }

        public static class TelemetryData
        {
            public const double NO_SIGNAL = -120.5;
        }
    }
}