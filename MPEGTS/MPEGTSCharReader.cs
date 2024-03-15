using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{
    public class MPEGTSCharReader
    {
        /// <summary>
        ///  [0] https://en.wikipedia.org/wiki/T.51/ISO/IEC_6937
        ///  [1] https://dvb.org/wp-content/uploads/2019/08/A038r14_Specification-for-Service-Information-SI-in-DVB-Systems_Draft_EN_300-468-v1-17-1_Dec-2021.pdf
        /// </summary>
        /// <value>The</value>
        public static Dictionary<byte, Tuple<string, string>> ISO6937Table { get; set; } =
            new Dictionary<byte, Tuple<string, string>>()
            {
                { 0xC1, new Tuple<string, string>("AEIOUaeiou", "ÀÈÌÒÙàèìòù") },
                { 0xC2, new Tuple<string, string>("ACEILNORSUYZacegilnorsuyz", "ÁĆÉÍĹŃÓŔŚÚÝŹáćéģíĺńóŕśúýź")},
                { 0xC3, new Tuple<string, string>("ACEGHIJOSUWYaceghijosuwy", "ÂĈÊĜĤÎĴÔŜÛŴŶâĉêĝĥîĵôŝûŵŷ")},
                { 0xC4, new Tuple<string, string>("AINOUainou", "ÃĨÑÕŨãĩñõũ")},
                { 0xC5, new Tuple<string, string>("AEIOUaeiou", "ĀĒĪŌŪāēīōū")},
                { 0xC6, new Tuple<string, string>("AGUagu", "ĂĞŬăğŭ")},
                { 0xC7, new Tuple<string, string>("CEGIZcegz", "ĊĖĠİŻċėġż")},
                { 0xC8, new Tuple<string, string>("AEIOUYaeiouy", "ÄËÏÖÜŸäëïöüÿ")},
                { 0xCA, new Tuple<string, string>("AUau", "ÅŮåů")},
                { 0xCB, new Tuple<string, string>("CGKLNRSTcklnrst", "ÇĢĶĻŅŖŞŢçķļņŗşţ")},
                { 0xCD, new Tuple<string, string>("OUou", "ŐŰőű")},
                { 0xCE, new Tuple<string, string>("AEIUaeiu", "ĄĘĮŲąęįų")},
                { 0xCF, new Tuple<string, string>("CDELNRSTZcdelnrstz", "ČĎĚĽŇŘŠŤŽčďěľňřšťž")},
            };

        public static string[] CelticLatin = new string[256]  // Character code table 0A - Latin/Celtic alphabet with Unicode equivalents
        {
            "","","","","","","","","","","","","","","","",   //  0 .. 15
            "","","","","","","","","","","","","","","","",   // 16 .. 31
            "\u0020","\u0021","\u0022","\u0023","\u0024","\u0025","\u0026","\u0027","\u0028","\u0029","\u002A","\u002B","\u002C","\u002D","\u002E","\u002F",   // 32
            "\u0030","\u0031","\u0032","\u0033","\u0034","\u0035","\u0036","\u0037","\u0038","\u0039","\u003A","\u003B","\u003C","\u003D","\u003E","\u003F",   // 64
            "\u0040","\u0041","\u0042","\u0043","\u0044","\u0045","\u0046","\u0047","\u0048","\u0049","\u004A","\u004B","\u004C","\u004D","\u004E","\u004F",   // 80
            "\u0050","\u0051","\u0052","\u0053","\u0054","\u0055","\u0056","\u0057","\u0058","\u0059","\u005A","\u005B","\u005C","\u005D","\u005E","\u005F",   // 96
            "\u0060","\u0061","\u0062","\u0063","\u0064","\u0065","\u0066","\u0067","\u0068","\u0069","\u006A","\u006B","\u006C","\u006D","\u006E","\u006F",   // 112
            "\u0070","\u0071","\u0072","\u0073","\u0074","\u0075","\u0076","\u0077","\u0078","\u0079","\u007A","\u007B","\u007C","\u007D","\u007E","",   // 128
            "","","","","","","","","","","","","","","","",   // 144
            "","","","","","","","","","","","","","","","",   // 160
            "\u00A0","\u1E02","\u1E03","\u00A3","\u010A","\u010B","\u1E0A","\u00A7","\u1E80","\u00A9","\u1E82","\u1E0B","\u1EF2","\u00AD","\u00AE","\u0178",   // 176
            "\u1E1E","\u1E1F","\u0120","\u0121","\u1E40","\u1E41","\u00B6","\u1E56","\u1E81","\u1E57","\u1E83","\u1E60","\u1EF3","\u1E84","\u1E85","\u1E61",   // 192
            "\u00C0","\u00C1","\u00C2","\u00C3","\u00C4","\u00C5","\u00C6","\u00C7","\u00C8","\u00C9","\u00CA","\u00CB","\u00CC","\u00CD","\u00CE","\u00CF",   // 208
            "\u0174","\u00D1","\u00D2","\u00D3","\u00D4","\u00D5","\u00D6","\u00D7","\u00D8","\u00D9","\u00DA","\u00DB","\u00DC","\u00DD","\u0176","\u00DF",   // 224
            "\u00E0","\u00E1","\u00E2","\u00E3","\u00E4","\u00E5","\u00E6","\u00E7","\u00E8","\u00E9","\u00EA","\u00EB","\u00EC","\u00ED","\u00EE","\u00EF",   // 240
            "\u0175","\u00F1","\u00F2","\u00F3","\u00F4","\u00F5","\u00F6","\u1E6B","\u00F8","\u00F9","\u00FA","\u00FB","\u00FC","\u00FD","\u0177","\u00FF"    // 256
        };

        private static string ReadControlCode(byte b)
        {
            if ((b >= 0x80) && (b <= 0x85))
            {
                // reserved for future use
                return String.Empty;
            }

            if (b == 0x86)
            {
                // character emphasis on
                return String.Empty;
            }

            if (b == 0x87)
            {
                // character emphasis off
                return String.Empty;
            }

            if ((b >= 0x88) && (b <= 0x89))
            {
                // reserved for future use
                return String.Empty;
            }

            if (b == 0x8A)
            {
                // CRLF
                return Environment.NewLine;
            }

            if ((b >= 0x8B) && (b <= 0x9F))
            {
                // user defined
                return String.Empty;
            }

            return null; // not control code
        }

        public static string ReadString(byte[] bytes, int index, int count)
        {
            if (bytes == null ||
                bytes.Length == 0 ||
                count == 0 ||
                index+count > bytes.Length)
            {
                return String.Empty;
            }

            var characterTableByte = bytes[index];

            if ((characterTableByte > 0) && (characterTableByte < 0x20))
            {
                // not default encoding

                // first byte determines encoding
                index++;
                count--;

                string txt = null;

                // [1] see Table A.3: Character coding table
                switch (characterTableByte)
                {
                    case 1:
                        // ISO 8859-5 Latin/Cyrillic alphabet - see table A.2
                        txt = System.Text.Encoding.GetEncoding("iso-8859-5").GetString(bytes, index, count);
                        break;
                    case 2:
                        // ISO 8859-6 Latin/Arabic alphabet - see table A.3
                        txt = System.Text.Encoding.GetEncoding("iso-8859-6").GetString(bytes, index, count);
                        break;
                    case 3:
                        // ISO 8859-7 Latin/Arabic alphabet - see table A.4
                        txt = System.Text.Encoding.GetEncoding("iso-8859-7").GetString(bytes, index, count);
                        break;
                    case 4:
                        // ISO 8859-8 Latin/Arabic alphabet - see table A.5
                        txt = System.Text.Encoding.GetEncoding("iso-8859-8").GetString(bytes, index, count);
                        break;
                    case 5:
                        // ISO 8859-9 Latin/Arabic alphabet - see table A.6
                        txt = System.Text.Encoding.GetEncoding("iso-8859-9").GetString(bytes, index, count);
                        break;
                    case 6:
                        // ISO/IEC 8859-10
                        txt = System.Text.Encoding.GetEncoding("iso-8859-10").GetString(bytes, index, count);
                        break;
                    case 7:
                        // ISO/IEC 8859-11
                        txt = System.Text.Encoding.GetEncoding("iso-8859-11").GetString(bytes, index, count);
                        break;
                    case 9:
                        // ISO/IEC 8859-13
                        txt = System.Text.Encoding.GetEncoding("iso-8859-13").GetString(bytes, index, count);
                        break;
                    case 0xB:
                        // ISO/IEC 8859-15
                        txt = System.Text.Encoding.GetEncoding("iso-8859-15").GetString(bytes, index, count);
                        break;
                    case 0x11:
                        // Unicode - ISO/IEC 10646 [52]
                        txt = System.Text.Encoding.Unicode.GetString(bytes, index, count);
                        break;
                    case 0x15:
                        // UTF - 8 encoding of ISO / IEC 10646[52] BMP
                        txt = System.Text.Encoding.UTF8.GetString(bytes, index, count);
                        break;

                    case 0x08: // reserved for future use (see NOTE)
                    case 0x0A: // ISO/IEC 8859-14 not supported in .NET!
                    case 0x10: // dynamically selected part of ISO / IEC 8859
                    case 0x12: // KS X 1001 - 2014[54] Korean character set
                    case 0x13: // GB - 2312 - 1980[53] Simplified Chinese character set
                    case 0x14: // Big5 subset of ISO/IEC 10646 [16] Traditional Chinese
                    default:
                        return string.Empty;
                }

                if (txt != null)
                {
                    return txt;
                }

                return String.Empty;
            }

            // all subsequent bytes in the text item are coded using the default character coding table (Latin alphabet)

            var res = new StringBuilder();

            byte accent = 0;

            for (var i=index; i<index+count;i++)
            {
                var b = bytes[i];

                var controlCode = ReadControlCode(b);

                if (controlCode != null)
                {
                    // control code found:
                    res.Append(controlCode);
                    accent = 0;
                    continue;
                }

                if (ISO6937Table.ContainsKey(b))
                {
                    // accent
                    accent = b;
                }
                else
                {
                    if (b >= 0x20 && b <= 0x7F)
                    {
                        if (accent == 0)
                        {
                            res.Append(Encoding.ASCII.GetString(new byte[] { b }));
                        }
                        else
                        {
                            var notAccentedChar = Encoding.ASCII.GetString(new byte[] { b });

                            var accentedChar = notAccentedChar;

                            if (!String.IsNullOrEmpty(ISO6937Table[accent].Item1))
                            {
                                var pos = ISO6937Table[accent].Item1.IndexOf(notAccentedChar);

                                if (pos >= 0)
                                {
                                    accentedChar = ISO6937Table[accent].Item2.Substring(pos, 1);
                                }
                            }

                            res.Append(accentedChar);

                        }
                    }
                    accent = 0;
                }
            }

            return res.ToString();
        }
    }
}
