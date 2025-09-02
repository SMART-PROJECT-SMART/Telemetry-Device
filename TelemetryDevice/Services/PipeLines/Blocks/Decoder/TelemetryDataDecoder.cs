using System.Collections;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public class TelemetryDataDecoder : ITelemetryDecoder
    {
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
            {
                mainDataSection[bitIndex] = compressedTelemetryData[bitIndex];
            }

            return mainDataSection;
        }

        private BitArray ExtractSignBits(BitArray compressedTelemetryData, ICD telemetryIcd)
        {
            int signBitsCount = telemetryIcd.Document.Count;
            int mainDataLength = telemetryIcd.GetSizeInBites();
            BitArray signBitSection = new BitArray(signBitsCount);

            for (int signBitIndex = 0; signBitIndex < signBitsCount; signBitIndex++)
            {
                signBitSection[signBitIndex] = compressedTelemetryData[
                    mainDataLength + signBitIndex
                ];
            }

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
                {
                    reconstructedParameterValue = -reconstructedParameterValue;
                }

                reconstructedTelemetryData[telemetryParameter.Name] = reconstructedParameterValue;
                telemetryFieldIndex++;
            }

            return reconstructedTelemetryData;
        }

        private ulong ExtractFieldBits(BitArray mainDataBitSection, ICDItem telemetryParameter)
        {
            int fieldStartBit = telemetryParameter.StartBitArrayIndex;
            int fieldBitLength = telemetryParameter.BitLength;
            ulong extractedFieldValue = 0;

            for (int bitOffset = 0; bitOffset < fieldBitLength; bitOffset++)
            {
                if (mainDataBitSection[fieldStartBit + bitOffset])
                {
                    extractedFieldValue |=
                        TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << bitOffset;
                }
            }

            return extractedFieldValue;
        }

        private double ConvertFromMeaningfulBits(ulong compressedBits, int totalBitLength)
        {
            if (compressedBits == 0)
                return 0.0;

            int exponentBitsCount = Math.Min(
                TelemetryDeviceConstants.TelemetryCompression.MAX_EXPONENT_BITS,
                totalBitLength / TelemetryDeviceConstants.TelemetryCompression.EXPONENT_BITS_DIVISOR
            );
            int significandBitsCount = totalBitLength - exponentBitsCount;

            ulong exponentBitMask = (1UL << exponentBitsCount) - 1;
            ulong significandBitMask = (1UL << significandBitsCount) - 1;

            int storedExponentValue =
                (int)(compressedBits >> significandBitsCount) & (int)exponentBitMask;
            ulong storedSignificandValue = compressedBits & significandBitMask;

            int exponentBias = (1 << exponentBitsCount - 1) - 1;
            int actualExponentValue = storedExponentValue - exponentBias;

            double significandValue =
                1.0 + (double)storedSignificandValue / (1UL << significandBitsCount);

            return significandValue * Math.Pow(2, actualExponentValue);
        }
    }
}
