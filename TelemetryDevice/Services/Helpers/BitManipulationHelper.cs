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
            ulong extractedValue = TelemetryDeviceConstants.TelemetryCompression.DEFAULT_ULONG_VALUE;
            for (int bitOffset = 0; bitOffset < bitLength; bitOffset++)
            {
                if (bitArray[startBitPosition + bitOffset])
                    extractedValue |= TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << bitOffset;
            }
            return extractedValue;
        }
        public static double ConvertFromMeaningfulBits(ulong compressedBits, int totalBitLength)
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