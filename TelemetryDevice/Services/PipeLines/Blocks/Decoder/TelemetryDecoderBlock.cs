using System.Collections;
using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Extensions;
using TelemetryDevices.Services.Helpers;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public class TelemetryDecoderBlock : ITelemetryDecoderBlock
    {
        private readonly TransformBlock<ValidationResult, DecodingResult> _transformBlock;
        private readonly ICD _icd;

        public TelemetryDecoderBlock(ICD icd)
        {
            _icd = icd;
            _transformBlock = new TransformBlock<ValidationResult, DecodingResult>(
                validationResult =>
                {
                    if (!validationResult.IsValid)
                    {
                        return new DecodingResult(new Dictionary<TelemetryFields, double>());
                    }

                    var decodedData = DecodeData(validationResult.Data, _icd);
                    return new DecodingResult(decodedData);
                });
        }

        public Dictionary<TelemetryFields, double> DecodeData(
            byte[] rawTelemetryData,
            ICD telemetryIcd
        )
        {
            BitArray compressedBitArray = rawTelemetryData.ToBitArray();
            Dictionary<TelemetryFields, double> decompressedTelemetryData =
                DecompressTelemetryDataByICD(compressedBitArray, telemetryIcd);
            return decompressedTelemetryData;
        }

        public Task Completion => _transformBlock.Completion;
        public void Complete() => _transformBlock.Complete();
        public void Fault(Exception exception) => ((IDataflowBlock)_transformBlock).Fault(exception);
        
        public bool Post(ValidationResult item) => ((ITargetBlock<ValidationResult>)_transformBlock).Post(item);
        public Task<bool> SendAsync(ValidationResult item, CancellationToken cancellationToken = default) => 
            ((ITargetBlock<ValidationResult>)_transformBlock).SendAsync(item, cancellationToken);
        
        public bool TryReceive(Predicate<DecodingResult> filter, out DecodingResult item) => 
            _transformBlock.TryReceive(out item);
        public bool TryReceiveAll(out IList<DecodingResult> items) => 
            _transformBlock.TryReceiveAll(out items);
        
        public IDisposable LinkTo(ITargetBlock<DecodingResult> target, DataflowLinkOptions linkOptions) => 
            ((ISourceBlock<DecodingResult>)_transformBlock).LinkTo(target, linkOptions);
        
        public DecodingResult ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<DecodingResult> target, out bool messageConsumed) => 
            ((ISourceBlock<DecodingResult>)_transformBlock).ConsumeMessage(messageHeader, target, out messageConsumed);
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<DecodingResult> target) => 
            ((ISourceBlock<DecodingResult>)_transformBlock).ReserveMessage(messageHeader, target);
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<DecodingResult> target) => 
            ((ISourceBlock<DecodingResult>)_transformBlock).ReleaseReservation(messageHeader, target);
        
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, ValidationResult messageValue, ISourceBlock<ValidationResult> source, bool consumeToAccept) => 
            ((ITargetBlock<ValidationResult>)_transformBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);

        private Dictionary<TelemetryFields, double> DecompressTelemetryDataByICD(
            BitArray compressedTelemetryData,
            ICD telemetryIcd
        )
        {
            BitArray mainDataBitSection = ExtractMainDataBits(
                compressedTelemetryData,
                telemetryIcd
            );
            BitArray signBitSection = ExtractSignBits(compressedTelemetryData, telemetryIcd);
            return ReconstructTelemetryValues(mainDataBitSection, signBitSection, telemetryIcd);
        }

        private BitArray ExtractMainDataBits(BitArray compressedTelemetryData, ICD telemetryIcd)
        {
            int mainDataLength = telemetryIcd.GetSizeInBites();
            BitArray mainDataSection = new BitArray(mainDataLength);

            for (int bitIndex = 0; bitIndex < mainDataLength; bitIndex++)
                mainDataSection[bitIndex] = compressedTelemetryData[bitIndex];

            return mainDataSection;
        }

        private BitArray ExtractSignBits(BitArray compressedTelemetryData, ICD telemetryIcd)
        {
            int signBitsCount = telemetryIcd.Document.Count;
            int mainDataLength = telemetryIcd.GetSizeInBites();
            BitArray signBitSection = new BitArray(signBitsCount);

            for (int signBitIndex = 0; signBitIndex < signBitsCount; signBitIndex++)
                signBitSection[signBitIndex] = compressedTelemetryData[
                    mainDataLength + signBitIndex
                ];

            return signBitSection;
        }

        private Dictionary<TelemetryFields, double> ReconstructTelemetryValues(
            BitArray mainDataBitSection,
            BitArray signBitSection,
            ICD telemetryIcd
        )
        {
            Dictionary<TelemetryFields, double> reconstructedTelemetryData =
                new Dictionary<TelemetryFields, double>();
            int telemetryFieldIndex = 0;

            foreach (ICDItem telemetryParameter in telemetryIcd)
            {
                ulong extractedBitValue = BitManipulationHelper.ExtractBitsAsULong(
                    mainDataBitSection,
                    telemetryParameter.StartBitArrayIndex,
                    telemetryParameter.BitLength
                );
                double reconstructedParameterValue =
                    BitManipulationHelper.ConvertFromMeaningfulBits(
                        extractedBitValue,
                        telemetryParameter.BitLength
                    );

                if (signBitSection[telemetryFieldIndex])
                    reconstructedParameterValue = -reconstructedParameterValue;

                reconstructedTelemetryData[telemetryParameter.Name] = reconstructedParameterValue;
                telemetryFieldIndex++;
            }

            return reconstructedTelemetryData;
        }
    }
}
