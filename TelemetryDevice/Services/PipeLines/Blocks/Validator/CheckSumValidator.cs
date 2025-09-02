using System.Collections;
using TelemetryDevices.Common;
using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public class ChecksumValidator : IValidator
    {
        public bool Validate(byte[] compressedTelemetryData, ICD icd)
        {
            var telemetryBits = new BitArray(compressedTelemetryData);
            int totalBitsCount = telemetryBits.Length;

            int icdBitsLength = icd.GetSizeInBites();
            int signBitsLength = icd.Document.Count;
            int checksumBitsLength = TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_BITS;
            
            int dataBitsLength = icdBitsLength + signBitsLength;
            int dataPlusChecksumBits = dataBitsLength + checksumBitsLength;
            int paddingBitsLength = (TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE - (dataPlusChecksumBits % TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE)) % TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE;
            int expectedTotalBits = dataBitsLength + checksumBitsLength + paddingBitsLength;

            if (totalBitsCount < dataBitsLength + checksumBitsLength)
                return false;

            var dataBitsSection = SubBits(telemetryBits, 0, dataBitsLength);
            uint expectedChecksum = CalculateChecksum(dataBitsSection);

            int checksumStartPosition = totalBitsCount - paddingBitsLength - checksumBitsLength;
            uint actualChecksumPrePadding = ExtractUInt(telemetryBits, checksumStartPosition, checksumBitsLength);
            if (expectedChecksum == actualChecksumPrePadding && (totalBitsCount == expectedTotalBits || checksumStartPosition >= 0))
                return true;

            uint actualChecksumLast32Bits = ExtractUInt(telemetryBits, totalBitsCount - checksumBitsLength, checksumBitsLength);
            return expectedChecksum == actualChecksumLast32Bits;
        }

        private BitArray SubBits(BitArray sourceBits, int startIndex, int bitsCount)
        {
            var destinationBits = new BitArray(bitsCount);
            for (int bitIndex = 0; bitIndex < bitsCount && startIndex + bitIndex < sourceBits.Length; bitIndex++)
                destinationBits[bitIndex] = sourceBits[startIndex + bitIndex];
            return destinationBits;
        }

        private static uint ExtractUInt(BitArray bitArray, int startPosition, int bitsCount)
        {
            if (startPosition < 0 || startPosition + bitsCount > bitArray.Length)
                return TelemetryDeviceConstants.TelemetryCompression.DEFAULT_UINT_VALUE;
            uint extractedValue = TelemetryDeviceConstants.TelemetryCompression.DEFAULT_UINT_VALUE;
            for (int bitOffset = 0; bitOffset < bitsCount; bitOffset++)
                if (bitArray[startPosition + bitOffset])
                    extractedValue |= (uint)(
                        TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << bitOffset
                    );
            return extractedValue;
        }

        private static uint CalculateChecksum(BitArray dataBits)
        {
            uint runningChecksum = TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_SEED;
            int bitsPerByteConstant = TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE;
            int totalByteCount = (dataBits.Length + bitsPerByteConstant - 1) / bitsPerByteConstant;

            for (int currentByteIndex = 0; currentByteIndex < totalByteCount; currentByteIndex++)
            {
                byte currentByteValue = GetByte(dataBits, currentByteIndex, bitsPerByteConstant);
                runningChecksum =
                    runningChecksum * TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_MULTIPLIER
                        + TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_INCREMENT
                        + currentByteValue
                    & TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_MODULO;
            }
            return runningChecksum;
        }

        private static byte GetByte(BitArray dataBits, int byteIndex, int bitsPerByteConstant)
        {
            byte extractedByteValue = TelemetryDeviceConstants.TelemetryCompression.DEFAULT_BYTE_VALUE;
            int startBitPosition = byteIndex * bitsPerByteConstant;
            int bitsInCurrentByte = Math.Min(bitsPerByteConstant, dataBits.Length - startBitPosition);
            for (int bitPositionInByte = 0; bitPositionInByte < bitsInCurrentByte; bitPositionInByte++)
                if (dataBits[startBitPosition + bitPositionInByte])
                    extractedByteValue |= (byte)(1 << bitPositionInByte);
            return extractedByteValue;
        }
    }
}
