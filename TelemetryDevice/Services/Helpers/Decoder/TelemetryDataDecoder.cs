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
                ulong valueWithSign = ApplySignBit(extractedBits, signBits[fieldIndex]);
                double telemetryValue = ConvertBitsToDouble(valueWithSign);

                telemetryData[telemetryParameter.Name] = telemetryValue;
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

        private ulong ApplySignBit(ulong valueBits, bool isNegative)
        {

            if (isNegative)
            {
                valueBits |= TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << TelemetryDeviceConstants.TelemetryCompression.DOUBLE_SIGN_BIT_POSITION;
            }
            else
            {
                valueBits &= ~(TelemetryDeviceConstants.TelemetryCompression.BIT_SHIFT_BASE << TelemetryDeviceConstants.TelemetryCompression.DOUBLE_SIGN_BIT_POSITION);
            }

            return valueBits;
        }

        private double ConvertBitsToDouble(ulong valueBits)
        {
            byte[] doubleBytes = BitConverter.GetBytes(valueBits);
            return BitConverter.ToDouble(doubleBytes, 0);
        }
    }
}