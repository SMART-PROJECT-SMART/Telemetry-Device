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
        public static BitArray SubBits(this BitArray sourceBits, int startIndex, int bitsCount)
        {
            var destinationBits = new BitArray(bitsCount);
            for (int bitIndex = 0; bitIndex < bitsCount && startIndex + bitIndex < sourceBits.Length; bitIndex++)
                destinationBits[bitIndex] = sourceBits[startIndex + bitIndex];
            return destinationBits;
        }

        public static uint ExtractUInt(this BitArray bitArray, int startPosition, int bitsCount)
        {
            uint extractedValue = TelemetryDeviceConstants.TelemetryCompression.DEFAULT_UINT_VALUE;
            for (int bitOffset = 0; bitOffset < bitsCount && startPosition + bitOffset < bitArray.Length; bitOffset++)
                if (bitArray[startPosition + bitOffset])
                    extractedValue |= (uint)(TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << bitOffset);
            return extractedValue;
        }

        public static byte GetByte(this BitArray dataBits, int byteIndex, int bitsPerByteConstant)
        {
            byte extractedByteValue = TelemetryDeviceConstants.TelemetryCompression.DEFAULT_BYTE_VALUE;
            int startBitPosition = byteIndex * bitsPerByteConstant;
            int bitsInCurrentByte = Math.Min(bitsPerByteConstant, dataBits.Length - startBitPosition);

            for (int bitPositionInByte = 0; bitPositionInByte < bitsInCurrentByte; bitPositionInByte++)
                if (dataBits[startBitPosition + bitPositionInByte])
                    extractedByteValue |= (byte)(TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE << bitPositionInByte);
            return extractedByteValue;
        }
    }
}
