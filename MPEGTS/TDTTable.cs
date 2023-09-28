using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{
    public class TDTTable : DVBTTable
    {
        // https://www.etsi.org/deliver/etsi_en/300400_300499/300468/01.17.01_20/en_300468v011701a.pdf

        public DateTime UTCTime { get; set; }

        public override void Parse(List<byte> bytes)
        {
            if (bytes == null || bytes.Count < 5)
                return;

            var pointerField = bytes[0];
            var pos = 1;

            if (pointerField != 0)
            {
                pos = pos + pointerField;
            }

            if (bytes.Count < pos + 2)
                return;

            ID = bytes[pos];

            if (! (ID == 0x70 ||  // Time and Date Table
                  ID == 0x73))    // Time Offset Table
                return;

            SectionSyntaxIndicator = ((bytes[pos + 1] & 128) == 128);
            Private = ((bytes[pos + 1] & 64) == 64);
            Reserved = Convert.ToByte((bytes[pos + 1] & 48) >> 4);
            SectionLength = Convert.ToInt32(((bytes[pos + 1] & 15) << 8) + bytes[pos + 2]);

            pos += 3;

            UTCTime = DVBTTable.ParseTime(bytes, pos);
         }

        public override bool CRCIsValid()
        {
            return true; // TDT has no CRC
        }

        public void WriteToConsole(bool detailed = false)
        {
            Console.WriteLine(WriteToString(detailed));
        }

        public string WriteToString(bool detailed = false)
        {
            var sb = new StringBuilder();

            if (detailed)
            {
                sb.AppendLine($"ID                    : {ID}");
                sb.AppendLine($"SectionSyntaxIndicator: {SectionSyntaxIndicator}");
                sb.AppendLine($"Private               : {Private}");
                sb.AppendLine($"Reserved              : {Reserved}");
                sb.AppendLine($"SectionLength         : {SectionLength}");

                if (SectionSyntaxIndicator)
                {
                    sb.AppendLine($"Version                : {Version}");
                    sb.AppendLine($"CurrentIndicator       : {CurrentIndicator}");
                    sb.AppendLine($"SectionNumber          : {SectionNumber}");
                    sb.AppendLine($"LastSectionNumber      : {LastSectionNumber}");
                }
            }

            sb.AppendLine($"UTCTime            : {UTCTime}");

            return sb.ToString();
        }
    }
}
