using System.Collections;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Helpers
{
    public static class BitManipulationHelper
    {
        public static ulong ExtractBitsAsULong(
            BitArray bitArray,
            int startBitPosition,
            int bitLength
        )
        {
            ulong extractedValue = TelemetryDeviceConstants
                .TelemetryCompression
                .DEFAULT_ULONG_VALUE;
            for (int bitOffset = 0; bitOffset < bitLength; bitOffset++)
            {
                if (bitArray[startBitPosition + bitOffset])
                    extractedValue |=
                        TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << bitOffset;
            }
            return extractedValue;
        }

        public static double ConvertFromMeaningfulBits(ulong compressedBits, int totalBitLength)
        {
            if (compressedBits == TelemetryDeviceConstants.TelemetryCompression.DEFAULT_ULONG_VALUE)
                return TelemetryDeviceConstants.TelemetryCompression.DEFAULT_DOUBLE_VALUE;

            int exponentBitsCount = CalculateExponentBitsCount(totalBitLength);
            int significandBitsCount = totalBitLength - exponentBitsCount;

            ulong exponentBitMask = CreateExponentMask(exponentBitsCount);
            ulong significandBitMask = CreateSignificandMask(significandBitsCount);

            int storedExponentValue = ExtractStoredExponent(compressedBits, significandBitsCount, exponentBitMask);
            ulong storedSignificandValue = ExtractStoredSignificand(compressedBits, significandBitMask);

            int actualExponentValue = CalculateActualExponent(storedExponentValue, exponentBitsCount);
            double significandValue = CalculateSignificandValue(storedSignificandValue, significandBitsCount);

            return ReconstructFloatingPointValue(significandValue, actualExponentValue);
        }

        private static int CalculateExponentBitsCount(int totalBitLength)
        {
            return Math.Min(
                TelemetryDeviceConstants.TelemetryCompression.MAX_EXPONENT_BITS,
                totalBitLength / TelemetryDeviceConstants.TelemetryCompression.EXPONENT_BITS_DIVISOR
            );
        }   

        private static ulong CreateExponentMask(int exponentBitsCount)
        {
            return (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << exponentBitsCount)
                - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE;
        }

        private static ulong CreateSignificandMask(int significandBitsCount)
        {
            return (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << significandBitsCount)
                - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE;
        }

        private static int ExtractStoredExponent(ulong compressedBits, int significandBitsCount, ulong exponentBitMask)
        {
            return (int)(compressedBits >> significandBitsCount) & (int)exponentBitMask;
        }

        private static ulong ExtractStoredSignificand(ulong compressedBits, ulong significandBitMask)
        {
            return compressedBits & significandBitMask;
        }

        private static int CalculateActualExponent(int storedExponentValue, int exponentBitsCount)
        {
            int exponentBias = (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE
                << (exponentBitsCount - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE))
                - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE;
            return storedExponentValue - exponentBias;
        }

        private static double CalculateSignificandValue(ulong storedSignificandValue, int significandBitsCount)
        {
            return TelemetryDeviceConstants.TelemetryCompression.SIGNIFICAND_BASE_VALUE
                + (double)storedSignificandValue
                    / (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << significandBitsCount);
        }

        private static double ReconstructFloatingPointValue(double significandValue, int actualExponentValue)
        {
            return significandValue * Math.Pow(
                TelemetryDeviceConstants.TelemetryCompression.MATH_POWER_BASE,
                actualExponentValue
            );
        }
    }
}
