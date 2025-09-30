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
        public DecodingResult DecodeTelemetryData(
            ValidationResult validationResult,
            ICD telemetryIcd
        )
        {
            BitArray compressedBitArray = validationResult.Data.ToBitArray();
            Dictionary<TelemetryFields, double> decompressedTelemetryData =
                DecompressTelemetryDataByICD(compressedBitArray, telemetryIcd);
            return new DecodingResult(decompressedTelemetryData);
        }

        private Dictionary<TelemetryFields, double> DecompressTelemetryDataByICD(
            BitArray compressedTelemetryData,
            ICD telemetryIcd
        )
        {
            BitArray mainDataBitSection = ExtractMainTelemetryDataBits(
                compressedTelemetryData,
                telemetryIcd
            );
            BitArray signBitSection = ExtractSignBits(compressedTelemetryData, telemetryIcd);
            return ReconstructTelemetryValues(mainDataBitSection, signBitSection, telemetryIcd);
        }

        private BitArray ExtractMainTelemetryDataBits(
            BitArray compressedTelemetryData,
            ICD telemetryIcd
        )
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
