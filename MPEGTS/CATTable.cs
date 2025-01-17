using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{
    // https://en.wikipedia.org/wiki/Program-specific_information#PAT_(Program_association_specific_data)

    public class CATTable : DVBTTable
    {
        public List<CasInformation> CasInfo { get; set; } = new List<CasInformation>();

        private int _make_words(byte high, byte low){
            return Convert.ToInt32((high << 8) + low);
        }

        private byte _get_low_half_byte(byte X){
            return (byte)((X) & (0x0f));
        }

        private byte _get_low_five_bits(byte X)  {
            return (byte)(((X) & (0x1f)));
        }

        public override void Parse(List<byte> bytes)
        {
            if (bytes == null || bytes.Count < 5)
                return;

            var pointerField = bytes[0];
            var cnt = 1;
            var sec_length = 0;
            var desc_lenth = 0;
            List<byte> p_inbuf = bytes;

            if (pointerField != 0)
            {
                cnt = cnt + pointerField;
            }

            ID = p_inbuf[cnt];
            cnt++;

            SectionSyntaxIndicator = ((bytes[cnt] & 128) == 128);
            Private = ((bytes[cnt] & 64) == 64);
            Reserved = Convert.ToByte((bytes[cnt] & 48) >> 4);

            //Get section length
            SectionLength = sec_length = _make_words(_get_low_half_byte(p_inbuf[cnt]), p_inbuf[cnt + 1]);
            cnt += 2;

            Data = new byte[sec_length];
            CRC = new byte[4];

            Data[0] = 0;
            bytes.CopyTo(pointerField + 1, Data, 1, SectionLength - 1);
            bytes.CopyTo(pointerField + SectionLength, CRC, 0, 4);

            //Skip reserved
            cnt += 2;
            sec_length--;

            //Get version number
            Version = (byte)((p_inbuf[cnt] & 0x3e) >> 1);
            cnt++;
            sec_length--;

            //Get section number
            SectionNumber = p_inbuf[cnt];

            cnt++;
            sec_length--;

            //Get last section number
            LastSectionNumber = p_inbuf[cnt];
            cnt++;
            sec_length--;

            while (sec_length > 0)
            {
                desc_lenth = 0;
                switch (p_inbuf[cnt])
                {
                    case 0x9:
                        {
                            //skip tag
                            cnt++;
                            sec_length--;

                            //Get section length
                            desc_lenth = p_inbuf[cnt];
                            cnt++;
                            sec_length--;

                            //Delete whole descriptor
                            sec_length -= desc_lenth;

                            CasInformation ca_desc = new CasInformation();
                            ca_desc.CasSystemID = _make_words(p_inbuf[cnt], p_inbuf[cnt + 1]);
                            cnt += 2;
                            desc_lenth -= 2;

                            ca_desc.EmmPid = _make_words(_get_low_five_bits(p_inbuf[cnt]), p_inbuf[cnt + 1]);
                            CasInfo.Add(ca_desc);
                            cnt += 2;
                            desc_lenth -= 2;
                        }
                        break;

                    default:
                        {
                            //skip tag
                            cnt++;
                            sec_length--;

                            //Get section length
                            desc_lenth = p_inbuf[cnt];
                            cnt++;
                            sec_length--;

                            //Delete whole descriptor
                            sec_length -= desc_lenth;
                        }
                        break;
                }
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
            }

            sb.AppendLine();

            // sb.AppendLine($"{"Program number",14} {" (Map) PID".PadRight(10, ' '),10}");
            // sb.AppendLine($"{"--------------",14} {"---".PadRight(10, '-'),10}");
            foreach (var programAssociations in CasInfo)
            {
                sb.AppendLine($"{programAssociations.CasSystemID,14} {programAssociations.EmmPid,10}");
            }

            return sb.ToString();
        }
    }
}
