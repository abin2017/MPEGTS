using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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

        public static string[] ISO_8859_10 = new string[256]
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
            "\u00A0","\u0104","\u0112","\u0122","\u012A","\u0128","\u0136","\u00A7","\u013B","\u0110","\u0160","\u0166","\u017D","\u00AD","\u016A","\u014A",
            "\u00B0","\u0105","\u0113","\u0123","\u012B","\u0129","\u0137","\u00B7","\u013C","\u0111","\u0161","\u0167","\u017E","\u2015","\u016B","\u014B",
            "\u0100","\u00C1","\u00C2","\u00C3","\u00C4","\u00C5","\u00C6","\u012E","\u010C","\u00C9","\u0118","\u00CB","\u0116","\u00CD","\u00CE","\u00CF",
            "\u00D0","\u0145","\u014C","\u00D3","\u00D4","\u00D5","\u00D6","\u0168","\u00D8","\u0172","\u00DA","\u00DB","\u00DC","\u00DD","\u00DE","\u00DF",
            "\u0101","\u00E1","\u00E2","\u00E3","\u00E4","\u00E5","\u00E6","\u012F","\u010D","\u00E9","\u0119","\u00EB","\u0117","\u00ED","\u00EE","\u00EF",
            "\u00F0","\u0146","\u014D","\u00F3","\u00F4","\u00F5","\u00F6","\u0169","\u00F8","\u0173","\u00FA","\u00FB","\u00FC","\u00FD","\u00FE","\u0138"
        };

        public static string[] ISO_8859_13 = new string[256]
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

            "\u00A0","\u201D","\u00A2","\u00A3","\u00A4","\u201E","\u00A6","\u00A7","\u00D8","\u00A9","\u0156","\u00AB","\u00AC","\u00AD","\u00AE","\u00C6", // 176
            "\u00B0","\u00B1","\u00B2","\u00B3","\u201C","\u00B5","\u00B6","\u00B7","\u00F8","\u00B9","\u0157","\u00BB","\u00BC","\u00BD","\u00BE","\u00E6", // 192

            "\u0104","\u012E","\u0100","\u0106","\u00C4","\u00C5","\u0118","\u0112","\u010C","\u00C9","\u0179","\u0116","\u0122","\u0136","\u012A","\u013B", // 208
            "\u0160","\u0143","\u0145","\u00D3","\u014C","\u00D5","\u00D6","\u00D7","\u0172","\u0141","\u015A","\u016A","\u00DC","\u017B","\u017D","\u00DF",

            "\u0105","\u012F","\u0101","\u0107","\u00E4","\u00E5","\u0119","\u0113","\u010D","\u00E9","\u017A","\u0117","\u0123","\u0137","\u012B","\u013C",
            "\u0161","\u0144","\u0146","\u00F3","\u014D","\u00F5","\u00F6","\u00F7","\u0173","\u0142","\u015B","\u016B","\u00FC","\u017C","\u017E","\u2019"
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

        private static string GetManualEncodingString(string[] encoding, byte[] bytes, int index, int count)
        {
            var res = new StringBuilder();

            for (var i=index; i<index+count;i++)
            {
                res.Append(encoding[bytes[i]]);
            }

            return res.ToString();
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

                return GetTabledCodedString(bytes, characterTableByte, index, count);
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

        private static string GetTabledCodedString(byte[] bytes, byte characterTableByte, int index, int count)
        {
            // Xamarin does not suport this encodings:
            // iso-8859-10: "'iso-8859-10' is not a supported encoding name. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.\nParameter name: name"
            // iso-8859-13: {System.NotSupportedException: No data is available for encoding 28603. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.  at ....

            var txt = String.Empty;

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
                    txt = GetManualEncodingString(ISO_8859_10, bytes, index, count);
                    break;
                case 7:
                    // ISO/IEC 8859-11
                    txt = System.Text.Encoding.GetEncoding("iso-8859-11").GetString(bytes, index, count);
                    break;
                case 9:
                    // ISO/IEC 8859-13
                    txt = GetManualEncodingString(ISO_8859_13, bytes, index, count);
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

                case 0x10: // dynamically selected part of ISO / IEC 8859
                    if (count>3)
                    {
                        var secondByte = bytes[index];
                        if (secondByte == 0)
                        {
                            var thirdByte = bytes[index + 1];
                            index += 2;
                            count -= 2;
                            switch (thirdByte)
                            {
                                case 1:
                                    // ISO 8859-1 West European - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(bytes, index, count);
                                    break;
                                case 2:
                                    // ISO 8859-2 East European - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-2").GetString(bytes, index, count);
                                    break;
                                case 3:
                                    // ISO 8859-3 South European - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-3").GetString(bytes, index, count);
                                    break;
                                case 4:
                                    // ISO 8859-4 North and North-East European - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-4").GetString(bytes, index, count);
                                    break;
                                case 5:
                                    // ISO 8859-5 Latin/Cyrillic - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-5").GetString(bytes, index, count);
                                    break;
                                case 6:
                                    // ISO 8859-5 Latin/Arabic - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-6").GetString(bytes, index, count);
                                    break;
                                case 7:
                                    // ISO 8859-7 Latin/Greek - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-7").GetString(bytes, index, count);
                                    break;
                                case 8:
                                    // ISO 8859-8 Latin/Hebrew - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-8").GetString(bytes, index, count);
                                    break;
                                case 9:
                                    // ISO 8859-9 West European & Turkish - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-9").GetString(bytes, index, count);
                                    break;
                                case 10:
                                    // ISO 8859-10 North European - see table A.4
                                    txt = GetManualEncodingString(ISO_8859_10, bytes, index, count);
                                    break;
                                case 11:
                                    // ISO 8859-11 Thai - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-11").GetString(bytes, index, count);
                                    break;
                                case 13:
                                    // ISO 8859-13 Baltic - see table A.4
                                    txt = GetManualEncodingString(ISO_8859_13, bytes, index, count);
                                    break;
                                case 14:
                                    // ISO 8859-14 Celtic - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-14").GetString(bytes, index, count);
                                    break;
                                case 15:
                                    // ISO 8859-5 West European - see table A.4
                                    txt = System.Text.Encoding.GetEncoding("iso-8859-15").GetString(bytes, index, count);
                                    break;
                            }
                        }
                    }
                    break;

                case 0x08: // reserved for future use (see NOTE)
                case 0x0A: // ISO/IEC 8859-14 not supported in .NET!
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
    }
}

