using System.Collections;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Helpers
{
    public static class TailIdExtractor
    {
        public static int? GetTailIdByICD(byte[] packetPayload, ICD telemetryIcd)
        {
            ICDItem tailIdIcdItem = telemetryIcd.Document.FirstOrDefault(icdItem =>
                icdItem.Name == TelemetryFields.TailId
            );
            if (tailIdIcdItem == null)
            {
                return null;
            }

            BitArray payloadBitArray = ConvertBytesToBitArray(packetPayload);
            if (payloadBitArray.Length < tailIdIcdItem.StartBitArrayIndex + tailIdIcdItem.BitLength)
            {
                return null;
            }

            ulong extractedTailIdBits = ExtractBitsAsULong(
                payloadBitArray,
                tailIdIcdItem.StartBitArrayIndex,
                tailIdIcdItem.BitLength
            );
            double reconstructedTailIdValue = ConvertFromMeaningfulBits(
                extractedTailIdBits,
                tailIdIcdItem.BitLength
            );

            return (int)Math.Round(reconstructedTailIdValue);
        }

        private static BitArray ConvertBytesToBitArray(byte[] packetPayload)
        {
            BitArray payloadBitArray = new BitArray(
                packetPayload.Length * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE
            );
            for (int byteIndex = 0; byteIndex < packetPayload.Length; byteIndex++)
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
                    payloadBitArray[absoluteBitPosition] =
                        (
                            packetPayload[byteIndex]
                            & (
                                TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE
                                << bitPositionInByte
                            )
                        ) != 0;
                }
            }
            return payloadBitArray;
        }

        private static ulong ExtractBitsAsULong(
            BitArray payloadBitArray,
            int startBitPosition,
            int bitLength
        )
        {
            ulong extractedValue = TelemetryDeviceConstants.TelemetryCompression.DEFAULT_ULONG_VALUE;
            for (int bitOffset = 0; bitOffset < bitLength; bitOffset++)
            {
                if (payloadBitArray[startBitPosition + bitOffset])
                    extractedValue |= TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << bitOffset;
            }
            return extractedValue;
        }

        private static double ConvertFromMeaningfulBits(ulong compressedBits, int totalBitLength)
        {
            if (compressedBits == TelemetryDeviceConstants.TelemetryCompression.DEFAULT_ULONG_VALUE)
                return TelemetryDeviceConstants.TelemetryCompression.DEFAULT_DOUBLE_VALUE;

            int exponentBitsCount = Math.Min(
                TelemetryDeviceConstants.TelemetryCompression.MAX_EXPONENT_BITS,
                totalBitLength / TelemetryDeviceConstants.TelemetryCompression.EXPONENT_BITS_DIVISOR
            );
            int significandBitsCount = totalBitLength - exponentBitsCount;

            ulong exponentBitMask = (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << exponentBitsCount) - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE;
            ulong significandBitMask = (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << significandBitsCount) - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE;

            int storedExponentValue =
                (int)(compressedBits >> significandBitsCount) & (int)exponentBitMask;
            ulong storedSignificandValue = compressedBits & significandBitMask;

            int exponentBias = (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE << (exponentBitsCount - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE)) - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE;
            int actualExponentValue = storedExponentValue - exponentBias;

            double significandValue =
                TelemetryDeviceConstants.TelemetryCompression.SIGNIFICAND_BASE_VALUE + (double)storedSignificandValue / (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << significandBitsCount);

            return significandValue * Math.Pow(TelemetryDeviceConstants.TelemetryCompression.MATH_POWER_BASE, actualExponentValue);
        }
    }
}
