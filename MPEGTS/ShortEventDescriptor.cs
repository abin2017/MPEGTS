using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{
    public class ShortEventDescriptor : EventDescriptor
    {
        public string LanguageCode { get; set; }
        public string EventName { get; set; }
        public string Text { get; set; }

        public static ShortEventDescriptor Parse(byte[] bytes, int startPos = 0)
        {
            var res = new ShortEventDescriptor();

            res.Tag = bytes[startPos + 0];
            res.Length = bytes[startPos + 1];

            res.LanguageCode = Encoding.GetEncoding("iso-8859-1").GetString(bytes, startPos + 2, 3);

            var eventNameLength = bytes[startPos + 5];

            var pos = startPos + 6;

            res.EventName = MPEGTSCharReader.ReadString(bytes, pos, eventNameLength, true);

            pos = pos + eventNameLength;

            var textLength = bytes[pos];

            pos++;

            res.Text = MPEGTSCharReader.ReadString(bytes, pos, textLength, true);

            return res;
        }
    }
}
