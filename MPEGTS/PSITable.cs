using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{
    // https://en.wikipedia.org/wiki/Program-specific_information#PAT_(Program_association_specific_data)

    public class PSITable : DVBTTable
    {
        public int TableIdExt { get; set; }

        public List<ProgramAssociation> ProgramAssociations { get; set; } = new List<ProgramAssociation>();

        private void ParsePAT(byte[] bytes)
        {
            int pos = 2;
            while (pos < bytes.Length)
            {
                var programNum = Convert.ToInt32(((bytes[pos + 0]) << 8) + (bytes[pos + 1]));
                var programPID = Convert.ToInt32(((bytes[pos + 2] & 31) << 8) + (bytes[pos + 3]));

                ProgramAssociations.Add(new ProgramAssociation()
                {
                    ProgramNumber = programNum,
                    ProgramMapPID = programPID
                });

                pos += 4;
            }
        }

        public override void Parse(List<byte> bytes)
        {
            if (bytes == null || bytes.Count < 5)
                return;

            MPEGTransportStreamPacket.WriteByteArrayToConsole(bytes.ToArray());

            var pointerField = bytes[0];
            var pos = 1;

            if (pointerField != 0)
            {
                pos = pos + pointerField;
            }

            if (bytes.Count < pos + 2)
                return;

            while (pos < bytes.Count)
            {
                var tableID = bytes[pos];
                if (tableID == 0xFF)
                    break;

                // read next 2 bytes
                var tableHeader1 = bytes[pos + 1];
                var tableHeader2 = bytes[pos + 2];

                var sectionSyntaxIndicator = ((tableHeader1 & 128) == 128);
                //Private = ((tableHeader1 & 64) == 64);
                //Reserved = Convert.ToByte((tableHeader1 & 48) >> 4);
                var sectionLength = Convert.ToInt32(((tableHeader1 & 15) << 8) + tableHeader2);

                var tableData = new byte[sectionLength];
                var crc = new byte[4];

                tableData[0] = 0;
                bytes.CopyTo(pos+1, tableData, 1, sectionLength-1);
                bytes.CopyTo(pos + sectionLength - 1, crc, 0, 4);

                if (tableID == 0)
                {
                    // PAT
                    //ParsePAT(tableData);

                    ID = 0;
                    Data = tableData;
                    CRC = crc;
                }

                pos += sectionLength + 4; // Data + CRC
            }
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
                sb.AppendLine($"CRC OK                : {CRCIsValid()}");

                if (SectionSyntaxIndicator)
                {
                    sb.AppendLine($"TableIdExt            : {TableIdExt}");
                    sb.AppendLine($"Version               : {Version}");
                    sb.AppendLine($"CurrentIndicator      : {CurrentIndicator}");
                    sb.AppendLine($"SectionNumber         : {SectionNumber}");
                    sb.AppendLine($"LastSectionNumber     : {LastSectionNumber}");
                }
            }

            sb.AppendLine();

            sb.AppendLine($"{"Program number",14} {" (Map) PID".PadRight(10, ' '),10}");
            sb.AppendLine($"{"--------------",14} {"---".PadRight(10, '-'),10}");
            foreach (var programAssociations in ProgramAssociations)
            {
                sb.AppendLine($"{programAssociations.ProgramNumber,14} {programAssociations.ProgramMapPID,10}");
            }

            return sb.ToString();
        }
    }
}
