using System.Collections;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Extensions
{
    public static class ByteArrayExtensions
    {
        
        public static BitArray ToBitArray(this byte[] bytes)
        {
            BitArray bitArray = new BitArray(
                bytes.Length * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE
            );
            
            for (int byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
            {
                for (
                    int bitPositionInByte = 0;
                    bitPositionInByte < TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE;
                    bitPositionInByte++
                )
                {
                    int absoluteBitPosition =
                        (byteIndex * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE)
                        + bitPositionInByte;
                    bitArray[absoluteBitPosition] =
                        (
                            bytes[byteIndex]
                            & (
                                TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE
                                << bitPositionInByte
                            )
                        ) != 0;
                }
            }
            
            return bitArray;
        }
    }
}