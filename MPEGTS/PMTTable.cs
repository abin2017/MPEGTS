using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MPEGTS
{
    // https://en.wikipedia.org/wiki/Program-specific_information#PAT_(Program_association_specific_data
    // https://www.etsi.org/deliver/etsi_en/300400_300499/300468/01.17.01_20/en_300468v011701a.pdf

    public class PMTTable : DVBTTable
    {
        public List<ElementaryStreamSpecificData> Streams { get; set; } = new List<ElementaryStreamSpecificData>();
        public List<ElementaryStreamSpecificData> Audio { get; set; } = new List<ElementaryStreamSpecificData>();
        public List<ElementaryStreamSpecificData> Video { get; set; } = new List<ElementaryStreamSpecificData>();

        public ElementaryCasDescriptor CasInfo = new ElementaryCasDescriptor();
        public long PCRPID { get; set; }

        

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

            // read next 2 bytes
            var tableHeader1 = bytes[pos + 1];
            var tableHeader2 = bytes[pos + 2];

            SectionSyntaxIndicator = ((tableHeader1 & 128) == 128);
            Private = ((tableHeader1 & 64) == 64);
            Reserved = Convert.ToByte((tableHeader1 & 48) >> 4);
            SectionLength = Convert.ToInt32(((tableHeader1 & 15) << 8) + tableHeader2);

            Data = new byte[SectionLength];
            CRC = new byte[4];

            Data[0] = 0;
            bytes.CopyTo(pointerField + 1, Data, 1, SectionLength - 1);
            bytes.CopyTo(pointerField + SectionLength, CRC, 0, 4);

            pos = pos + 3;

            var posAfterTable = pos + SectionLength - 4;

            TableIDExt = (bytes[pos + 0] << 8) + bytes[pos + 1];

            pos = pos + 2;

            Version = Convert.ToByte((bytes[pos + 0] & 64) >> 1);
            CurrentIndicator = (bytes[pos + 0] & 1) == 1;

            SectionNumber = bytes[pos + 1];
            LastSectionNumber = bytes[pos + 2];

            pos = pos + 3;

            // reserved bits, PCR PID
            PCRPID = Convert.ToInt32(((bytes[pos] & 31) << 8) + bytes[pos + 1]);

            pos = pos + 2;

            var programInfoLength = Convert.ToInt32(((bytes[pos+0] & 3) << 8) + bytes[pos + 1]);
            pos = pos + 2;
            var programInfoBytes = new byte[programInfoLength];
            var cnt = 0;

            
            bytes.CopyTo(pos, programInfoBytes, 0, programInfoLength);
            
            while (cnt < programInfoLength) {
                var descriptorTag = programInfoBytes[cnt + 0];
                var descriptorLength = programInfoBytes[cnt + 1];

                if (descriptorTag == 0x9)
                {
                    var idx = 0;

                    idx++;
                    //skip desc len
                    idx++;

                    var ca_sys_id = Convert.ToInt32((programInfoBytes[cnt + idx] << 8) + programInfoBytes[cnt + idx + 1]);
                    cnt += 2;
                    var ecm_pid = Convert.ToInt32(((programInfoBytes[cnt + idx] & 0x1F) << 8) + programInfoBytes[cnt + idx + 1]);
                    cnt += 2;

                    CasInfo.Add(ca_sys_id, ecm_pid);
                }
                cnt += descriptorLength;
            }

            
            // skipping program info
            pos += programInfoLength;

            // 2 (pointer and Table Id) + 2 (Section Length) + length  - CRC - Program Info Length
            //var posAfterElementaryStreamSpecificData = 4 + SectionLength - 4 - programInfoLength;

            while (pos < posAfterTable)
            {
                // reading elementary stream info data until end of section
                var stream = new ElementaryStreamSpecificData();
                stream.StreamType = bytes[pos + 0];
                stream.PID = Convert.ToInt32(((bytes[pos + 1] & 31) << 8) + bytes[pos + 2]);
                stream.ESInfoLength = Convert.ToInt32(((bytes[pos + 3] & 15) << 8) + bytes[pos + 4]);

                Streams.Add(stream);

                pos += 5;

                // Elementary stream descriptors folow

                var descriptorTag = bytes[pos + 0];
                var descriptorLength = bytes[pos + 1];

                var descriptorBytes = new byte[descriptorLength +2];

                if (bytes.Count < pos + descriptorLength + 2)
                {
                    // invalid descriptor length - skipping reading descriptors
                }
                else
                {
                    bytes.CopyTo(pos, descriptorBytes, 0, descriptorLength + 2);

                    if (descriptorTag == 0x59)  // 89
                    {
                        // subtitling_descriptor - see section 6.2.41
                        stream.SubtitleDescriptor.Parse(descriptorBytes);
                    }
                    else
                    if (descriptorTag == 0xA)  // 10
                    {
                        stream.LangugeAndAudioType = MPEGTSCharReader.ReadString(descriptorBytes, 2, descriptorLength);
                    }
                    else
                    if (descriptorTag == 0x9) {
                        cnt = 0;

                        cnt++;
                        //skip desc len
                        cnt++;

                        var ca_sys_id = Convert.ToInt32((descriptorBytes[cnt] << 8) + descriptorBytes[cnt + 1]);
                        cnt += 2;
                        var ecm_pid = Convert.ToInt32((descriptorBytes[cnt] << 8) + descriptorBytes[cnt + 1]);
                        cnt += 2;

                        CasInfo.Add(ca_sys_id, ecm_pid);
                    }
                    else
                    {
                        // TODO - read other descriptors
                        //Console.WriteLine($"PMT: unknown tag descriptor: {descriptorTag:X} hex ({descriptorTag} dec)");
                    }
                }

                pos += stream.ESInfoLength;
            }

            foreach (var stream in Streams) {
                if (stream.IsVideo) {
                    Video.Add(stream);
                }

                if (stream.IsAudio)
                {
                    Audio.Add(stream);
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
                Console.WriteLine($"ID                    : {ID}");
                Console.WriteLine($"SectionSyntaxIndicator: {SectionSyntaxIndicator}");
                Console.WriteLine($"Private               : {Private}");
                Console.WriteLine($"Reserved              : {Reserved}");
                Console.WriteLine($"SectionLength         : {SectionLength}");

                if (SectionSyntaxIndicator)
                {
                    Console.WriteLine($"Version                : {Version}");
                    Console.WriteLine($"CurrentIndicator       : {CurrentIndicator}");
                    Console.WriteLine($"SectionNumber          : {SectionNumber}");
                    Console.WriteLine($"LastSectionNumber      : {LastSectionNumber}");
                }

                Console.WriteLine($"PCR PID                    : {PCRPID}");

                Console.WriteLine($"---- Stream:-----------------------");
                foreach (var stream in Streams)
                {
                    Console.WriteLine($"PID                    : {stream.PID}");
                    Console.WriteLine($"StreamType (byte)      : {stream.StreamType}");
                    Console.WriteLine($"StreamType (enum)      : {stream.StreamTypeDesc}");
                    Console.WriteLine($"-----------------------------------");
                }
            } else
            {
                Console.WriteLine($"PCR PID: {PCRPID,55}");

                foreach (var stream in Streams)
                {
                    Console.WriteLine($"  {stream.StreamTypeDesc.ToString().PadRight(38, ' '),38} {"".PadRight(14,' '),14} {stream.PID,8}");

                    var subLangugage = stream.GetSubtitleLanguageCode;

                    if (subLangugage != null)
                    {
                        Console.WriteLine($"    Subtitiles: {subLangugage}");
                    }

                    if (!string.IsNullOrEmpty(stream.LangugeAndAudioType))
                    {
                        Console.WriteLine($"    Language: {stream.LangugeAndAudioType}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
