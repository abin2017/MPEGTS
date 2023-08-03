using System;
using System.Collections.Generic;
using System.Text;

namespace MPEGTS
{
    public class ContentDescriptor : EventDescriptor
    {
        public int ContentNibbleLevel1 { get; set; }
        public int ContentNibbleLevel2 { get; set; }

        public static ContentDescriptor Parse(byte[] bytes, int startPos = 0)
        {
            var res = new ContentDescriptor();

            res.Tag = bytes[startPos+0];
            res.Length = bytes[startPos+1];

            if (res.Length == 2 && startPos+2<= bytes.Length)
            {
                res.ContentNibbleLevel1 = Convert.ToByte(bytes[startPos + 2] >> 4);
                res.ContentNibbleLevel2 = Convert.ToByte(bytes[startPos + 2] & 15);

                // TODO: Table 28: Content_nibble level 1 and 2 assignments
            }

            return res;
        }
    }
}
