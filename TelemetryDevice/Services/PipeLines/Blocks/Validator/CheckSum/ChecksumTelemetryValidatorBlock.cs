using System.Collections;
using System.Threading.Tasks.Dataflow;
using Core.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Extensions;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator.CheckSum
{
    public class ChecksumTelemetryValidatorBlock : ITelemetryValidatorBlock
    {
        public ValidationResult ValidateTelemetryData(byte[] compressedTelemetryData, ICD icd)
        {
            bool isValid = PerformValidation(compressedTelemetryData, icd);
            return new ValidationResult(isValid, compressedTelemetryData);
        }

        private bool PerformValidation(byte[] compressedTelemetryData, ICD icd)
        {
            var telemetryBits = new BitArray(compressedTelemetryData);
            int totalBitsCount = telemetryBits.Length;

            int icdBitsLength = icd.GetSizeInBites();
            int signBitsLength = icd.Document.Count;
            int checksumBitsLength = TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_BITS;

            int dataBitsLength = icdBitsLength + signBitsLength;
            int dataPlusChecksumBits = dataBitsLength + checksumBitsLength;
            int paddingBitsLength =
                (
                    TelemetryDeviceConstants.TelemetryCompression.BYTE_ALIGNMENT
                    - 
                        dataPlusChecksumBits
                        % TelemetryDeviceConstants.TelemetryCompression.BYTE_ALIGNMENT
                    
                ) % TelemetryDeviceConstants.TelemetryCompression.BYTE_ALIGNMENT;
            int expectedTotalBits = dataBitsLength + checksumBitsLength + paddingBitsLength;

            BitArray dataBitsSection = telemetryBits.SubBits(0, dataBitsLength);

            uint expectedChecksum = CalculateChecksum(dataBitsSection);

            int checksumStartPosition = totalBitsCount - paddingBitsLength - checksumBitsLength;
            uint actualChecksumPrePadding = telemetryBits.ExtractUInt(
                checksumStartPosition,
                checksumBitsLength
            );

            if (expectedChecksum == actualChecksumPrePadding && totalBitsCount == expectedTotalBits)
                return true;

            uint actualCheckSumBits = telemetryBits.ExtractUInt(
                totalBitsCount - checksumBitsLength,
                checksumBitsLength
            );
            return expectedChecksum == actualCheckSumBits;
        }

        private static uint CalculateChecksum(BitArray dataBits)
        {
            uint runningChecksum = TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_SEED;
            int bitsPerByteConstant = TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE;
            int totalByteCount = (dataBits.Length + bitsPerByteConstant - 1) / bitsPerByteConstant;

            for (int currentByteIndex = 0; currentByteIndex < totalByteCount; currentByteIndex++)
            {
                byte currentByteValue = dataBits.GetByte(currentByteIndex, bitsPerByteConstant);
                runningChecksum =
                    runningChecksum
                        * TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_MULTIPLIER
                        + TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_INCREMENT
                        + currentByteValue
                    & TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_MODULO;
            }
            return runningChecksum;
        }
    }
}
