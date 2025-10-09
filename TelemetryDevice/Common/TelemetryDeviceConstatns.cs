namespace TelemetryDevices.Common
{
    public static class TelemetryDeviceConstants
    {
        public static class Network
        {
            public const string UDP_FILTER = "udp";
            public const string DESTINATION_PORT_FILTER = "dst port {0}";
            public const string FILTER_SEPARATOR = " or ";
            public const string AND_SEPERATOR = " and (";
            public const string AND_SEPERATOR_END = ")";
        }

        public static class Config
        {
            public const string ICD_DIRECTORY = "ICD";
        }

        public static class PacketProcessing
        {
            public const int MINIMUM_PAYLOAD_LENGTH = 0;
        }

        public static class Configuration
        {
            public const string NETWORKING_SECTION = "Networking";
            public const string KAFKA = "Kafka";
        }

        public static class Kafka
        {
            public const string BASE_TOPIC_NAME = "telemetry-tailId-";
            public const int REPLICATION_FACTOR = 1;
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
            public const int MAX_EXPONENT_BITS = 8;
            public const int EXPONENT_BITS_DIVISOR = 4;
            public const uint DEFAULT_UINT_VALUE = 0u;
            public const byte DEFAULT_BYTE_VALUE = 0;
            public const int BYTE_ALIGNMENT = 8;
            public const int BIT_SHIFT_ONE = 1;
            public const double DEFAULT_DOUBLE_VALUE = 0.0;
            public const ulong DEFAULT_ULONG_VALUE = 0UL;
            public const double SIGNIFICAND_BASE_VALUE = 1.0;
            public const int MATH_POWER_BASE = 2;
        }

        public static class DefaultValues
        {
            public const int DEFAULT_TAIL_ID = 1;
            public const int DEFAULT_PORT_1 = 8000;
            public const int DEFAULT_PORT_2 = 8001;
            public const double DEFAULT_LOCATION_LAT = 0.0;
            public const double DEFAULT_LOCATION_LON = 0.0;
        }
    }
}
