using System.Collections;
using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Extensions;
using TelemetryDevices.Services.Helpers;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public class TelemetryDataDecoder : ITelemetryDecoder
    {
        private readonly TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> _transformBlock;
        private readonly ICD _icd;

        public TelemetryDataDecoder(ICD icd)
        {
            _icd = icd;
            _transformBlock = new TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>>(
                decodingResult =>
                {
                    if (!decodingResult.IsValid)
                    {
                        return new Dictionary<TelemetryFields, double>();
                    }

                    return DecodeData(decodingResult.Data, _icd);
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
        
        public bool Post(DecodingResult item) => ((ITargetBlock<DecodingResult>)_transformBlock).Post(item);
        public Task<bool> SendAsync(DecodingResult item, CancellationToken cancellationToken = default) => 
            ((ITargetBlock<DecodingResult>)_transformBlock).SendAsync(item, cancellationToken);
        
        public bool TryReceive(Predicate<Dictionary<TelemetryFields, double>> filter, out Dictionary<TelemetryFields, double> item) => 
            _transformBlock.TryReceive(out item);
        public bool TryReceiveAll(out IList<Dictionary<TelemetryFields, double>> items) => 
            _transformBlock.TryReceiveAll(out items);
        
        public IDisposable LinkTo(ITargetBlock<Dictionary<TelemetryFields, double>> target, DataflowLinkOptions linkOptions) => 
            ((ISourceBlock<Dictionary<TelemetryFields, double>>)_transformBlock).LinkTo(target, linkOptions);
        
        public Dictionary<TelemetryFields, double> ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Dictionary<TelemetryFields, double>> target, out bool messageConsumed) => 
            ((ISourceBlock<Dictionary<TelemetryFields, double>>)_transformBlock).ConsumeMessage(messageHeader, target, out messageConsumed);
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Dictionary<TelemetryFields, double>> target) => 
            ((ISourceBlock<Dictionary<TelemetryFields, double>>)_transformBlock).ReserveMessage(messageHeader, target);
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Dictionary<TelemetryFields, double>> target) => 
            ((ISourceBlock<Dictionary<TelemetryFields, double>>)_transformBlock).ReleaseReservation(messageHeader, target);
        
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, DecodingResult messageValue, ISourceBlock<DecodingResult> source, bool consumeToAccept) => 
            ((ITargetBlock<DecodingResult>)_transformBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);

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
