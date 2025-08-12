using System.Collections;
using TelemetryDevice.Common;

namespace TelemetryDevice.Services.Helpers
{
    public class ChecksumValidator : IValidator
    {
        public  bool Validate(byte[] compressedData)
        {
            var bits = new BitArray(compressedData);
            int totalBits = bits.Length;

            int icdBits = TelemetryDeviceConstants.TelemetryCompression.ICD_BITS;
            int signBits = TelemetryDeviceConstants.TelemetryCompression.SIGN_BITS;
            int checksumBits = TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_BITS;
            int paddingBits = TelemetryDeviceConstants.TelemetryCompression.PADDING_BITS;

            int dataBitsLen = icdBits + signBits;          
            int expectedTotal = dataBitsLen + checksumBits + paddingBits;

            if (totalBits < dataBitsLen + checksumBits)
                return false; 

            var dataBits = SubBits(bits, 0, dataBitsLen);
            uint expected = CalculateChecksum(dataBits);

            int checksumStart = totalBits - paddingBits - checksumBits;
            uint actualPrePad = ExtractUInt(bits, checksumStart, checksumBits);
            if (expected == actualPrePad && (totalBits == expectedTotal || checksumStart >= 0))
                return true;

            uint actualLast32 = ExtractUInt(bits, totalBits - checksumBits, checksumBits);
            return expected == actualLast32;
        }

        private  BitArray SubBits(BitArray src, int start, int count)
        {
            var dest = new BitArray(count);
            for (int i = 0; i < count && (start + i) < src.Length; i++)
                dest[i] = src[start + i];
            return dest;
        }

        private static uint ExtractUInt(BitArray bits, int start, int count)
        {
            if (start < 0 || start + count > bits.Length) return 0u;
            uint value = 0u;
            for (int i = 0; i < count; i++)
                if (bits[start + i])
                    value |= (uint)(TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << i);
            return value;
        }

        private static uint CalculateChecksum(BitArray data)
        {
            uint checksum = TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_SEED;
            int bitsPerByte = TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE;
            int byteCount = (data.Length + bitsPerByte - 1) / bitsPerByte;

            for (int byteIndex = 0; byteIndex < byteCount; byteIndex++)
            {
                byte b = GetByte(data, byteIndex, bitsPerByte);
                checksum = (checksum * TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_MULTIPLIER
                           + TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_INCREMENT + b)
                           & TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_MODULO;
            }
            return checksum;
        }

        private static byte GetByte(BitArray data, int byteIndex, int bitsPerByte)
        {
            byte value = 0;
            int startBit = byteIndex * bitsPerByte;
            int bitsInThisByte = Math.Min(bitsPerByte, data.Length - startBit);
            for (int bit = 0; bit < bitsInThisByte; bit++)
                if (data[startBit + bit]) value |= (byte)(1 << bit);
            return value;
        }
    }
}