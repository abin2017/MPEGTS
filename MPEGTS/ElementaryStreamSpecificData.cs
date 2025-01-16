using System;
using System.Collections;
using System.Collections.Generic;
namespace MPEGTS
{
    public class ElementaryStreamSpecificData
    {
        public SubtitlingDescriptor SubtitleDescriptor { get; set; } = new SubtitlingDescriptor();
        public string LangugeAndAudioType { get; set; }

        public byte StreamType { get; set; }
        public int PID { get; set; }
        public int ESInfoLength { get; set; }

        public string GetSubtitleLanguageCode
        {
            get
            {
                if (SubtitleDescriptor == null ||
                    SubtitleDescriptor.SubtitleInfos == null ||
                    SubtitleDescriptor.SubtitleInfos.Count == 0)
                    return null;

                return SubtitleDescriptor.SubtitleInfos[0].LanguageCode;
            }
        }

        public StreamTypeEnum StreamTypeDesc
        {
            get
            {
                try
                {
                    return (StreamTypeEnum)StreamType;
                } catch
                {
                    return StreamTypeEnum.Unknown;
                }
            }
        }

        public bool IsAudio
        {
            get
            {
                if (StreamType == 0x6A || //DVB_DESC_AC3
                    StreamType == 0x7A || //DVB_DESC_EAC3
                    StreamType == 3 ||
                    StreamType == 4 || 
                    StreamType == 15 ||
                    StreamType == 17 ||
                    StreamType == 28 ||
                    StreamType == 128 ||
                    StreamType == 129 ||
                    StreamType == 131 ||
                    StreamType == 132 ||
                    StreamType == 135
                    ) {
                    return true;
                }
                return false;
            }
        }

        public bool IsVideo
        {
            get
            {
                if (StreamType == 2 ||
                    StreamType == 1 ||
                    StreamType == 16 ||
                    StreamType == 18 ||
                    StreamType == 19 ||
                    StreamType == 27 ||
                    StreamType == 30 ||
                    StreamType == 31 ||
                    StreamType == 32 ||
                    StreamType == 33 ||
                    StreamType == 36 ||
                    StreamType == 66
                    )
                {
                    return true;
                }
                return false;
            }
        }
    }

    public class ElementaryCasDescriptor
    {
        public Dictionary<long, long> CasDesc = new Dictionary<long, long>();

        public int GetCount() { 
            return CasDesc.Count;
        }

        public void Add(long casid, long ecmpid)
        {
            if (ecmpid != 0x1FFF && !CasDesc.ContainsKey(casid)) {
                CasDesc.Add(casid, ecmpid);
            }
        }

        public long Get(long casid)
        {
            long ecmpid = 0x1FFF;
            if (CasDesc.TryGetValue(casid, out ecmpid)) { 
                return ecmpid;
            }

            return 0x1FFF;
        }


    }
}
