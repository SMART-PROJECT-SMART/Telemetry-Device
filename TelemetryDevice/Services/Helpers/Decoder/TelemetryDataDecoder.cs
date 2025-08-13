using System.Collections;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Helpers.Decoder
{
    public class TelemetryDataDecoder : ITelemetryDecoder
    {
        private readonly Dictionary<TelemetryFields, double> _decodedData = new Dictionary<TelemetryFields, double>();

        public Dictionary<TelemetryFields, double> DecodeData(byte[] data, ICD icd)
        {
            BitArray compressedBitArray = ConvertBytesToBitArray(data);
            Dictionary<TelemetryFields, double> decompressedData = DecompressTelemetryDataByICD(compressedBitArray, icd);
            return decompressedData;
        }

        private BitArray ConvertBytesToBitArray(byte[] data)
        {
            BitArray bitArray = new BitArray(data.Length * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE);

            for (int byteIndex = 0; byteIndex < data.Length; byteIndex++)
            {
                for (int bitIndex = 0; bitIndex < TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE; bitIndex++)
                {
                    int absoluteBitIndex = byteIndex * TelemetryDeviceConstants.TelemetryCompression.BITS_PER_BYTE + bitIndex;
                    bitArray[absoluteBitIndex] = (data[byteIndex] & TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << bitIndex) != 0;
                }
            }

            return bitArray;
        }

        private Dictionary<TelemetryFields, double> DecompressTelemetryDataByICD(BitArray compressedData, ICD icd)
        {
            BitArray mainDataBits = ExtractMainDataBits(compressedData, icd);
            BitArray signBits = ExtractSignBits(compressedData, icd);
            return ReconstructTelemetryValues(mainDataBits, signBits, icd);
        }

        private BitArray ExtractMainDataBits(BitArray compressedData, ICD icd)
        {
            int dataLength = icd.GetSizeInBites();
            BitArray mainData = new BitArray(dataLength);

            for (int i = 0; i < dataLength; i++)
            {
                mainData[i] = compressedData[i];
            }

            return mainData;
        }

        private BitArray ExtractSignBits(BitArray compressedData, ICD icd)
        {
            int signBitsCount = icd.Document.Count;
            int dataLength = icd.GetSizeInBites();
            BitArray signBits = new BitArray(signBitsCount);

            for (int i = 0; i < signBitsCount; i++)
            {
                signBits[i] = compressedData[dataLength + i];
            }

            return signBits;
        }

        private Dictionary<TelemetryFields, double> ReconstructTelemetryValues(BitArray mainDataBits, BitArray signBits, ICD icd)
        {
            Dictionary<TelemetryFields, double> telemetryData = new Dictionary<TelemetryFields, double>();
            int fieldIndex = 0;

            foreach (ICDItem telemetryParameter in icd)
            {
                ulong extractedBits = ExtractFieldBits(mainDataBits, telemetryParameter);
                double reconstructedValue = ConvertFromMeaningfulBits(extractedBits, telemetryParameter.BitLength);

                if (signBits[fieldIndex])
                {
                    reconstructedValue = -reconstructedValue;
                }

                telemetryData[telemetryParameter.Name] = reconstructedValue;
                fieldIndex++;
            }

            return telemetryData;
        }

        private ulong ExtractFieldBits(BitArray mainDataBits, ICDItem telemetryParameter)
        {
            int startBit = telemetryParameter.StartBitArrayIndex;
            int bitLength = telemetryParameter.BitLength;
            ulong valueInBits = 0;

            for (int offset = 0; offset < bitLength; offset++)
            {
                if (mainDataBits[startBit + offset])
                {
                    valueInBits |= TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << offset;
                }
            }

            return valueInBits;
        }

        private double ConvertFromMeaningfulBits(ulong storedBits, int bitLength)
        {
            if (storedBits == 0) return 0.0;

            int exponentBits = Math.Min(TelemetryDeviceConstants.TelemetryCompression.MAX_EXPONENT_BITS, bitLength / TelemetryDeviceConstants.TelemetryCompression.EXPONENT_BITS_DIVISOR);
            int significandBits = bitLength - exponentBits;

            ulong exponentMask = (1UL << exponentBits) - 1;
            ulong significandMask = (1UL << significandBits) - 1;

            int storedExponent = (int)(storedBits >> significandBits) & (int)exponentMask;
            ulong storedSignificand = storedBits & significandMask;

            int ourBias = (1 << (exponentBits - 1)) - 1;
            int actualExponent = storedExponent - ourBias;

            double significand = 1.0 + (double)storedSignificand / (1UL << significandBits);

            return significand * Math.Pow(2, actualExponent);
        }
    }
}