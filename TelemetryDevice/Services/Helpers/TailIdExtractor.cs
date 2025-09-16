using System.Collections;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common;
using TelemetryDevices.Services.Extensions;

namespace TelemetryDevices.Services.Helpers
{
    public static class TailIdExtractor
    {
        public static int? GetTailIdByICD(byte[] packetPayload, ICD telemetryIcd)
        {
            ICDItem tailIdIcdItem = telemetryIcd.Document.FirstOrDefault(icdItem =>
                icdItem.Name == TelemetryFields.TailId
            )!;

            BitArray payloadBitArray = packetPayload.ToBitArray();
            if (payloadBitArray.Length < tailIdIcdItem.StartBitArrayIndex + tailIdIcdItem.BitLength)
            {
                return null;
            }

            ulong extractedTailIdBits = BitManipulationHelper.ExtractBitsAsULong(
                payloadBitArray,
                tailIdIcdItem.StartBitArrayIndex,
                tailIdIcdItem.BitLength
            );
            double reconstructedTailIdValue = BitManipulationHelper.ConvertFromMeaningfulBits(
                extractedTailIdBits,
                tailIdIcdItem.BitLength
            );

            return (int)Math.Round(reconstructedTailIdValue);
        }
    }
}
