using System.Collections;
using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Extensions;
using TelemetryDevices.Services.Helpers;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public class TelemetryDataDecoder : ITelemetryDecoder
    {
        public TelemetryDataDecoder() { }

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

        public TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> GetBlock(ICD icd)
        {
            return new TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>>(
                decodingResult =>
                {
                    if (!decodingResult.IsValid)
                    {
                        return new Dictionary<TelemetryFields, double>();
                    }

                    return DecodeData(decodingResult.Data, icd);
                }
            );
        }

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
