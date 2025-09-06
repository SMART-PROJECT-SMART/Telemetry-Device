using System.Collections;
using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public class TelemetryDataDecoder : ITelemetryDecoder
    {
        public TelemetryDataDecoder()
        {
        }

        private readonly Dictionary<TelemetryFields, double> _decodedData =
            new Dictionary<TelemetryFields, double>();

        public Dictionary<TelemetryFields, double> DecodeData(
            byte[] rawTelemetryData,
            ICD telemetryIcd
        )
        {
            BitArray compressedBitArray = ConvertBytesToBitArray(rawTelemetryData);
            Dictionary<TelemetryFields, double> decompressedTelemetryData =
                DecompressTelemetryDataByICD(compressedBitArray, telemetryIcd);
            return decompressedTelemetryData;
        }

        public TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> GetBlock(ICD icd)
        {
            return new TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>>(decodingResult =>
            {
                if (!decodingResult.IsValid)
                {
                    return new Dictionary<TelemetryFields, double>();
                }
                
                return DecodeData(decodingResult.Data, icd);
            });
        }

        private BitArray ConvertBytesToBitArray(byte[] rawTelemetryData)
        {
            BitArray telemetryBitArray = new BitArray(
                rawTelemetryData.Length
                    * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE
            );

            for (int byteIndex = 0; byteIndex < rawTelemetryData.Length; byteIndex++)
            {
                for (
                    int bitPositionInByte = 0;
                    bitPositionInByte < TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE;
                    bitPositionInByte++
                )
                {
                    int absoluteBitPosition =
                        byteIndex * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE
                        + bitPositionInByte;
                    telemetryBitArray[absoluteBitPosition] =
                        (
                            rawTelemetryData[byteIndex]
                            & TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE
                                << bitPositionInByte
                        ) != 0;
                }
            }

            return telemetryBitArray;
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
                signBitSection[signBitIndex] = compressedTelemetryData[mainDataLength + signBitIndex];

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
                ulong extractedBitValue = ExtractFieldBits(mainDataBitSection, telemetryParameter);
                double reconstructedParameterValue = ConvertFromMeaningfulBits(
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

        private ulong ExtractFieldBits(BitArray mainDataBitSection, ICDItem telemetryParameter)
        {
            int fieldStartBit = telemetryParameter.StartBitArrayIndex;
            int fieldBitLength = telemetryParameter.BitLength;
            ulong extractedFieldValue = TelemetryDeviceConstants.TelemetryCompression.DEFAULT_ULONG_VALUE;

            for (int bitOffset = 0; bitOffset < fieldBitLength; bitOffset++)
            {
                if (mainDataBitSection[fieldStartBit + bitOffset])
                    extractedFieldValue |= TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << bitOffset;
            }

            return extractedFieldValue;
        }

        private double ConvertFromMeaningfulBits(ulong compressedBits, int totalBitLength)
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

            int exponentBias = (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE << exponentBitsCount - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE) - TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_ONE;
            int actualExponentValue = storedExponentValue - exponentBias;

            double significandValue =
                TelemetryDeviceConstants.TelemetryCompression.SIGNIFICAND_BASE_VALUE + (double)storedSignificandValue / (TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << significandBitsCount);

            return significandValue * Math.Pow(TelemetryDeviceConstants.TelemetryCompression.MATH_POWER_BASE, actualExponentValue);
        }
    }
}
