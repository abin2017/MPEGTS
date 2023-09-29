using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{
    public class ExtendedEventDescriptor : EventDescriptor
    {
        public string LanguageCode { get; set; }

        public string Text { get; set; }

        public int DescriptorNumber { get; set; }
        public int LastDescriptorNumber { get; set; }

        public static ExtendedEventDescriptor Parse(byte[] bytes, int startPos = 0)
        {
            var res = new ExtendedEventDescriptor();

            res.Tag = bytes[startPos + 0];
            res.Length = bytes[startPos + 1];

            var numberAndLength = bytes[startPos + 2];

            res.DescriptorNumber = Convert.ToByte(numberAndLength >> 4);
            res.LastDescriptorNumber = Convert.ToByte(numberAndLength & 15);

            res.LanguageCode = Encoding.GetEncoding("iso-8859-1").GetString(bytes, startPos + 3, 3);

            var itemsLength = bytes[startPos + 6];

            // skipping reading items description

            var pos = startPos + 6 + itemsLength + 1;

            if (pos <= bytes.Length - 1)
            {
                var textLength = bytes[pos];

                res.Text = MPEGTSCharReader.ReadString(bytes, pos + 1, textLength);
            }

            return res;
        }
    }
}
