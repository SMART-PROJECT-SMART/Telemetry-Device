using System.Collections;
using System.Threading.Tasks.Dataflow;
using Core.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public class ChecksumValidatorBlock : IValidatorBlock
    {
        private readonly TransformBlock<byte[], ValidationResult> _transformBlock;
        private readonly ICD _icd;

        public ChecksumValidatorBlock(ICD icd)
        {
            _icd = icd;
            _transformBlock = new TransformBlock<byte[], ValidationResult>(rawTelemetryData =>
            {
                bool isDataValid = Validate(rawTelemetryData, _icd);
                return new ValidationResult(isDataValid, rawTelemetryData);
            });
        }

        public bool Validate(byte[] compressedTelemetryData, ICD icd)
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
                    - (
                        dataPlusChecksumBits
                        % TelemetryDeviceConstants.TelemetryCompression.BYTE_ALIGNMENT
                    )
                ) % TelemetryDeviceConstants.TelemetryCompression.BYTE_ALIGNMENT;
            int expectedTotalBits = dataBitsLength + checksumBitsLength + paddingBitsLength;

            var dataBitsSection = SubBits(telemetryBits, 0, dataBitsLength);
            uint expectedChecksum = CalculateChecksum(dataBitsSection);

            int checksumStartPosition = totalBitsCount - paddingBitsLength - checksumBitsLength;
            uint actualChecksumPrePadding = ExtractUInt(
                telemetryBits,
                checksumStartPosition,
                checksumBitsLength
            );
            if (expectedChecksum == actualChecksumPrePadding && totalBitsCount == expectedTotalBits)
                return true;

            uint actualCheckSumBits = ExtractUInt(
                telemetryBits,
                totalBitsCount - checksumBitsLength,
                checksumBitsLength
            );
            return expectedChecksum == actualCheckSumBits;
        }

        public Task Completion => _transformBlock.Completion;
        public void Complete() => _transformBlock.Complete();
        public void Fault(Exception exception) => ((IDataflowBlock)_transformBlock).Fault(exception);
        
        public bool Post(byte[] item) => ((ITargetBlock<byte[]>)_transformBlock).Post(item);
        public Task<bool> SendAsync(byte[] item, CancellationToken cancellationToken = default) => 
            ((ITargetBlock<byte[]>)_transformBlock).SendAsync(item, cancellationToken);
        
        public bool TryReceive(Predicate<ValidationResult> filter, out ValidationResult item) => 
            _transformBlock.TryReceive(out item);
        public bool TryReceiveAll(out IList<ValidationResult> items) => 
            _transformBlock.TryReceiveAll(out items);
        
        public IDisposable LinkTo(ITargetBlock<ValidationResult> target, DataflowLinkOptions linkOptions) => 
            ((ISourceBlock<ValidationResult>)_transformBlock).LinkTo(target, linkOptions);
        
        public ValidationResult ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<ValidationResult> target, out bool messageConsumed) => 
            ((ISourceBlock<ValidationResult>)_transformBlock).ConsumeMessage(messageHeader, target, out messageConsumed);
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<ValidationResult> target) => 
            ((ISourceBlock<ValidationResult>)_transformBlock).ReserveMessage(messageHeader, target);
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<ValidationResult> target) => 
            ((ISourceBlock<ValidationResult>)_transformBlock).ReleaseReservation(messageHeader, target);
        
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, byte[] messageValue, ISourceBlock<byte[]> source, bool consumeToAccept) => 
            ((ITargetBlock<byte[]>)_transformBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);

        private BitArray SubBits(BitArray sourceBits, int startIndex, int bitsCount)
        {
            var destinationBits = new BitArray(bitsCount);
            for (
                int bitIndex = 0;
                bitIndex < bitsCount && startIndex + bitIndex < sourceBits.Length;
                bitIndex++
            )
                destinationBits[bitIndex] = sourceBits[startIndex + bitIndex];
            return destinationBits;
        }

        private static uint ExtractUInt(BitArray bitArray, int startPosition, int bitsCount)
        {
            uint extractedValue = TelemetryDeviceConstants.TelemetryCompression.DEFAULT_UINT_VALUE;
            for (
                int bitOffset = 0;
                bitOffset < bitsCount && startPosition + bitOffset < bitArray.Length;
                bitOffset++
            )
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
                    runningChecksum
                        * TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_MULTIPLIER
                        + TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_INCREMENT
                        + currentByteValue
                    & TelemetryDeviceConstants.TelemetryCompression.CHECKSUM_MODULO;
            }
            return runningChecksum;
        }

        private static byte GetByte(BitArray dataBits, int byteIndex, int bitsPerByteConstant)
        {
            byte extractedByteValue = TelemetryDeviceConstants
                .TelemetryCompression
                .DEFAULT_BYTE_VALUE;
            int startBitPosition = byteIndex * bitsPerByteConstant;
            int bitsInCurrentByte = Math.Min(
                bitsPerByteConstant,
                dataBits.Length - startBitPosition
            );
            for (
                int bitPositionInByte = 0;
                bitPositionInByte < bitsInCurrentByte;
                bitPositionInByte++
            )
                if (dataBits[startBitPosition + bitPositionInByte])
                    extractedByteValue |= (byte)(
                        TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE
                        << bitPositionInByte
                    );
            return extractedByteValue;
        }
    }
}
