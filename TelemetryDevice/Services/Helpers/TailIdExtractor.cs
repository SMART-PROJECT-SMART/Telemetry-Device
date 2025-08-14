using System.Collections;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Helpers
{
    public static class TailIdExtractor
    {
        public static int? GetTailIdByICD(byte[] payload, ICD icd)
        {
            ICDItem tailIdItem = icd.Document.FirstOrDefault(item =>
                item.Name == TelemetryFields.TailId
            );
            if (tailIdItem == null)
            {
                return null;
            }

            BitArray payloadBits = ConvertBytesToBitArray(payload);
            if (payloadBits.Length < tailIdItem.StartBitArrayIndex + tailIdItem.BitLength)
            {
                return null;
            }

            ulong extractedBits = ExtractBitsAsULong(
                payloadBits,
                tailIdItem.StartBitArrayIndex,
                tailIdItem.BitLength
            );
            double reconstructedValue = ConvertFromMeaningfulBits(
                extractedBits,
                tailIdItem.BitLength
            );

            return (int)Math.Round(reconstructedValue);
        }

        private static BitArray ConvertBytesToBitArray(byte[] payload)
        {
            BitArray bitArray = new BitArray(
                payload.Length * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE
            );
            for (int byteIndex = 0; byteIndex < payload.Length; byteIndex++)
            {
                for (
                    int bitIndex = 0;
                    bitIndex < TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE;
                    bitIndex++
                )
                {
                    int absoluteBitIndex =
                        (byteIndex * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE)
                        + bitIndex;
                    bitArray[absoluteBitIndex] =
                        (
                            payload[byteIndex]
                            & (
                                TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE
                                << bitIndex
                            )
                        ) != 0;
                }
            }
            return bitArray;
        }

        private static ulong ExtractBitsAsULong(BitArray bits, int startBit, int bitLength)
        {
            ulong value = 0;
            for (int offset = 0; offset < bitLength; offset++)
            {
                if (bits[startBit + offset])
                {
                    value |= TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << offset;
                }
            }
            return value;
        }

        private static double ConvertFromMeaningfulBits(ulong storedBits, int bitLength)
        {
            if (storedBits == 0)
                return 0.0;

            int exponentBits = Math.Min(
                TelemetryDeviceConstants.TelemetryCompression.MAX_EXPONENT_BITS,
                bitLength / TelemetryDeviceConstants.TelemetryCompression.EXPONENT_BITS_DIVISOR
            );
            int significandBits = bitLength - exponentBits;

            ulong exponentMask = (1UL << exponentBits) - 1;
            ulong significandMask = (1UL << significandBits) - 1;

            int storedExponent = (int)(storedBits >> significandBits) & (int)exponentMask;
            ulong storedSignificand = storedBits & significandMask;

            int ourBias = (1 << (exponentBits - 1)) - 1;
            int actualExponent = storedExponent - ourBias;

            double significand = 1.0 + (double)storedSignificand / (1UL << significandBits);

            return significand * Math.Pow(2, actualExponent);
        }
    }
}
