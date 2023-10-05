using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace MPEGTS
{
    // https://en.wikipedia.org/wiki/MPEG_transport_stream

    public class MPEGTransportStreamPacket
    {
        public const byte MPEGTSSyncByte = 71;

        public byte SyncByte { get; set; }

        public bool TransportErrorIndicator { get; set; }
        public bool PayloadUnitStartIndicator { get; set; }
        public bool TransportPriority { get; set; }

        public ScramblingControlEnum ScramblingControl { get; set; }
        public AdaptationFieldControlEnum AdaptationFieldControl { get; set; }

        public byte AdaptationFieldLength { get; set; }

        // Adaptation Field Flag:
        public bool DiscontinuityIndicator { get; set; }
        public bool RandomAccessIndicator { get; set; }
        public bool ElementaryStreamPriorityIndicator  { get; set; }
        public bool PCRFlag { get; set; }
        public bool OPCRFlag { get; set; }
        public bool SplicingPointFlag { get; set; }
        public bool TransportPrivateDataFlag { get; set; }
        public bool AdaptationFieldExtensionFlag { get; set; }

        public List<byte> PCR { get; set; } = new List<byte>(); // 6 bytes of PCR

        public int PID { get; set; }
        public byte ContinuityCounter { get; set; }

        public List<byte> Header { get; set; } = new List<byte>();
        public List<byte> Payload { get; set; } = new List<byte>();
        public List<byte> AdaptationField { get; set; } = new List<byte>();

        public static void WriteByteArrayAsListToConsole(byte[] bytes)
        {
            Console.WriteLine(string.Join(",", bytes));
        }

        public static void WriteByteArrayToConsole(byte[] bytes, bool includeHexa = true)
        {
            var sb = new StringBuilder();
            var sbc = new StringBuilder();
            var sbb = new StringBuilder();
            var sbh = new StringBuilder();
            int c = 0;
            int row = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                sbb.Append($"{Convert.ToString(bytes[i], 2).PadLeft(8, '0'),9} ");
                sbh.Append($"{("0x"+Convert.ToString(bytes[i], 16)).ToUpper().PadLeft(8, ' '),9} ");
                sb.Append($"{bytes[i].ToString(),9} ");


                if (bytes[i] >= 32 && bytes[i] <= 128)
                {
                    sbc.Append($"{Convert.ToChar(bytes[i]),9} ");
                }
                else
                {
                    sbc.Append($"{"",9} ");
                }
                c++;

                if (c >= 10)
                {
                    Console.WriteLine(sbb.ToString()+"  "+((row+1)*10).ToString().PadLeft(3));
                    if (includeHexa)
                    {
                        Console.WriteLine(sbh.ToString());
                    }
                    Console.WriteLine(sb.ToString());
                    Console.WriteLine(sbc.ToString());
                    Console.WriteLine();
                    sb.Clear();
                    sbb.Clear();
                    sbc.Clear();
                    sbh.Clear();

                    c = 0;
                    row++;
                }
            }
            Console.WriteLine(sbb.ToString());
            if (includeHexa)
            {
                Console.WriteLine(sbh.ToString());
            }
            Console.WriteLine(sb.ToString());
            Console.WriteLine(sbc.ToString());
            Console.WriteLine();
        }

        public void WriteToConsole()
        {
            Console.WriteLine($"Sync Byte: {Convert.ToChar(SyncByte)} ({SyncByte.ToString()})");
            Console.WriteLine($"PID      : {PID}");
            Console.WriteLine($"TransportErrorIndicator  : {TransportErrorIndicator}");
            Console.WriteLine($"PayloadUnitStartIndicator: {PayloadUnitStartIndicator}");
            Console.WriteLine($"TransportPriority        : {TransportPriority}");

            Console.WriteLine($"ScramblingControl        : {ScramblingControl}");
            Console.WriteLine($"AdaptationFieldControl   : {AdaptationFieldControl}");

            Console.WriteLine($"ContinuityCounter        : {ContinuityCounter}");

            WriteByteArrayToConsole(Payload.ToArray());
        }

        /// <summary>
        /// Returns 27MHz timestamp
        /// </summary>
        /// <returns></returns>
        public ulong? GetPCRClock()
        {
            if (!PCRFlag || PCR == null || PCR.Count<6)
            {
                return null;
            }

            return GetPCRClock(PCR);
        }

        public static ulong GetPCRClock(List<byte> PCR)
        {
            ulong pcrBase = ((ulong)PCR[0] << 25) | ((ulong)PCR[1] << 17) | ((ulong)PCR[2] << 9) | (ulong)(PCR[3] << 1) | (ulong)(PCR[4] >> 7);
            ulong pcrExtension = (ulong)(PCR[4] & 0x01) << 8 | PCR[5];
            ulong pcrValue = (pcrBase * 300) + pcrExtension;

            return pcrValue;
        }

        public static string WritePacketListToString(List<MPEGTransportStreamPacket> packets)
        {
            var res = new StringBuilder();

            res.AppendLine($"{"PID",10}{"AdF.Ctrl",10}{"AdF.Len",10}{"Con.Cn.",10}{"Start",10}");
            foreach (var packet in packets)
            {
                res.AppendLine($"{packet.PID,10}{(int)packet.AdaptationFieldControl,10}{(int)packet.AdaptationFieldLength,10}{(int)packet.ContinuityCounter,10}{Convert.ToInt32(packet.PayloadUnitStartIndicator),10}");
            }

            return res.ToString();
        }

        public static string WriteBytesToString(List<byte> bytes)
        {
            var res = new StringBuilder();

            var sb = new StringBuilder();
            var sbc = new StringBuilder();
            var sbb = new StringBuilder();
            var sbp = new StringBuilder();
            var sbh = new StringBuilder();
            int c = 0;
            int row = 0;

            for (var i = 0; i < bytes.Count; i++)
            {
                sbp.Append($"{("["+Convert.ToString(i)+"]").PadLeft(8, ' '),9} ");
                sbb.Append($"{Convert.ToString(bytes[i], 2).PadLeft(8, '0'),9} ");
                sbh.Append($"{("0x" + Convert.ToString(bytes[i], 16)).PadLeft(8, ' '),9} ");
                sb.Append($"{bytes[i].ToString(),9} ");


                if (bytes[i] >= 32 && bytes[i] <= 128)
                {
                    sbc.Append($"{Convert.ToChar(bytes[i]),9} ");
                }
                else
                {
                    sbc.Append($"{"",9} ");
                }
                c++;

                if (c >= 10)
                {
                    res.AppendLine(sbp.ToString());
                    res.AppendLine(sbb.ToString());
                    res.AppendLine(sb.ToString());
                    res.AppendLine(sbh.ToString());
                    res.AppendLine(sbc.ToString());
                    res.AppendLine();
                    sb.Clear();
                    sbb.Clear();
                    sbc.Clear();
                    sbp.Clear();
                    sbh.Clear();

                    c = 0;
                    row++;
                }
            }
            res.AppendLine(sbp.ToString());
            res.AppendLine(sbb.ToString());
            res.AppendLine(sb.ToString());
            res.AppendLine(sbc.ToString());
            res.AppendLine();

            return res.ToString();
        }

        public static List<MPEGTransportStreamPacket> FindPacketsByPID(List<MPEGTransportStreamPacket> packets, int PID)
        {
            var res = new List<MPEGTransportStreamPacket>();
            bool firstPacketFound = false;

            foreach (var packet in packets)
            {
                if (packet.PID == PID)
                {
                    if (!firstPacketFound)
                    {
                        if (packet.PayloadUnitStartIndicator)
                        {
                            firstPacketFound = true;
                            res.Add(packet);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (packet.PayloadUnitStartIndicator)
                        {
                            break;
                        } else
                        {
                            res.Add(packet);
                        }
                    }
                }
            }

            return res;
        }

        public static Dictionary<int, List<MPEGTransportStreamPacket>> GetFilteredPackets(List<MPEGTransportStreamPacket> packets, int PID)
        {
            var res = new Dictionary<int, List<MPEGTransportStreamPacket>>();
            var currentKey = -1;

            foreach (var packet in packets)
            {
                if (packet.PID == PID)
                {
                    if (packet.PayloadUnitStartIndicator)
                    {
                        // there can be bytes in next packet!
                        if (res.ContainsKey(currentKey))
                        {
                            res[currentKey].Add(packet);
                        }

                        currentKey++;

                        if (!res.ContainsKey(currentKey))
                        {
                            res.Add(currentKey, new List<MPEGTransportStreamPacket>());
                        }
                        res[currentKey].Add(packet);
                    }
                    else
                    {
                        if (res.ContainsKey(currentKey))
                        {
                            res[currentKey].Add(packet);
                        }
                    }
                }
            }

            return res;
        }

        public static Dictionary<int, List<byte>> GetAllPacketsPayloadBytesByPID(IEnumerable<MPEGTransportStreamPacket> packets, long PID)
        {
            var res = new Dictionary<int, List<byte>>();
            var currentKey = -1;

            foreach (var packet in packets)
            {
                if (packet.PID == PID)
                {
                    if (packet.PayloadUnitStartIndicator)
                    {
                        // there can be bytes in next packet!
                        if (res.ContainsKey(currentKey))
                        {
                            if (packet.Payload != null &&
                                packet.Payload.Count > 0 &&
                                packet.Payload[0] != 0)
                            {
                                // not zero pointer field in next packet -> adding bytes to payload (except pointer field byte)
                                var bytesFromNextPacketCount = packet.Payload[0];
                                var buff = new byte[bytesFromNextPacketCount];
                                packet.Payload.CopyTo(1, buff, 0, bytesFromNextPacketCount);
                                res[currentKey].AddRange(buff);
                            }
                        }

                        currentKey++;

                        if (!res.ContainsKey(currentKey))
                        {
                            res.Add(currentKey, new List<byte>());
                        }
                        res[currentKey].AddRange(packet.Payload);
                    }
                    else
                    {
                        if (res.ContainsKey(currentKey))
                        {
                            res[currentKey].AddRange(packet.Payload);
                        }
                    }
                }
            }

            return res;
        }

        public static List<byte> GetPacketPayloadBytes(List<MPEGTransportStreamPacket> packets)
        {
            var result = new List<byte>();

            foreach (var packet in packets)
            {
                result.AddRange(packet.Payload);
            }

            return result;
        }

        public static List<byte> GetPacketPayloadBytesByPID(List<byte> bytes, int PID)
        {
            var packets = Parse(bytes);

            var filteredPackets = FindPacketsByPID(packets, PID);

            var result = new List<byte>();

            foreach (var packet in filteredPackets)
            {
                result.AddRange(packet.Payload);
            }

            return result;
        }

        public static ulong? GetFirstPCRClock(long PID, byte[] buffer, int offset = 0)
        {
            while (offset + 188 < buffer.Length)
            {
                var syncByte = buffer[offset];

                if (syncByte != MPEGTSSyncByte)
                {
                    throw new Exception("invalid packet, sync byte found");
                }

                var packetPID = ((buffer[offset + 1] & 31) << 8) + buffer[offset + 2];

                if (packetPID == PID)
                {
                    var adaptationFieldControl = (AdaptationFieldControlEnum)((buffer[offset + 3] & 48) >> 4);

                    if (adaptationFieldControl == AdaptationFieldControlEnum.AdaptationFieldFollowedByPayload ||
                        adaptationFieldControl == AdaptationFieldControlEnum.AdaptationFieldOnlyNoPayload)
                    {
                        var adaptationFieldLength = buffer[offset + 4];
                        if (adaptationFieldLength > 6)  // 1 for flag, 6 for PCR
                        {
                            var PCRFlag = (buffer[offset + 5] & 16) == 16;
                            if (PCRFlag)
                            {
                                var pcr = new List<byte>()
                                {
                                    buffer[offset + 6],
                                    buffer[offset + 7],
                                    buffer[offset + 8],
                                    buffer[offset + 9],
                                    buffer[offset + 10],
                                    buffer[offset + 11]
                                };

                                var clock = GetPCRClock(pcr);
                                return clock / 27000000;
                            }

                        }
                    }
                }

                offset += 188;
            }

            return null;
        }

        public void ParseBytes(IEnumerable<byte> bytes)
        {
            Payload.Clear();
            int bytePos = 0;
            byte pidFirstByte = 0;
            foreach (var b in bytes)
            {
                switch (bytePos)
                {
                    case 0:
                        SyncByte = b;
                        Header.Add(b);
                        break;
                    case 1:
                        TransportErrorIndicator = (b & 128) == 128;
                        PayloadUnitStartIndicator = (b & 64) == 64;
                        TransportPriority = (b & 32) == 32;
                        pidFirstByte = b;
                        Header.Add(b);
                        break;
                    case 2:

                        var pidFirst5Bits = (pidFirstByte & 31) << 8;
                        PID = pidFirst5Bits + b;
                        Header.Add(b);

                        break;
                    case 3:
                        var enumByte = (b & 192) >> 6;
                        ScramblingControl = (ScramblingControlEnum)enumByte;

                        enumByte = (b & 48) >> 4;
                        AdaptationFieldControl = (AdaptationFieldControlEnum)enumByte;

                        ContinuityCounter = Convert.ToByte(b & 15);

                        Header.Add(b);

                        break;
                    default:

                        switch (AdaptationFieldControl)
                        {
                            case AdaptationFieldControlEnum.Unknown:
                            case AdaptationFieldControlEnum.NoAdaptationFieldPayloadOnly:
                                Payload.Add(b);
                                break;

                            case AdaptationFieldControlEnum.AdaptationFieldFollowedByPayload:
                            case AdaptationFieldControlEnum.AdaptationFieldOnlyNoPayload:

                                // reading Adaptation field
                                if (bytePos == 4)
                                {
                                    AdaptationFieldLength = b;
                                }
                                if (AdaptationFieldLength > 0 && bytePos == 5)
                                {
                                    DiscontinuityIndicator = (b & 128) == 128;
                                    RandomAccessIndicator = (b & 64) == 64;
                                    ElementaryStreamPriorityIndicator = (b & 32) == 32;
                                    PCRFlag = (b & 16) == 16;
                                    OPCRFlag = (b & 8) == 8;
                                    SplicingPointFlag = (b & 4) == 4;
                                    TransportPrivateDataFlag = (b & 2) == 2;
                                    AdaptationFieldExtensionFlag = (b & 1) == 1;
                                }
                                if (AdaptationFieldLength > 0 && PCRFlag && bytePos > 5 && bytePos < 12)
                                {
                                    PCR.Add(b);
                                }

                                if (AdaptationFieldLength > 0 && bytePos >= 4 && bytePos < 4 + AdaptationFieldLength)
                                {
                                    AdaptationField.Add(b);
                                } else
                                {
                                    Payload.Add(b);
                                }

                                break;
                        }
                        break;
                }
                bytePos++;
            }
        }

        /// <summary>
        /// Finding sync byte position
        /// </summary>
        /// <param name="bytes">byte buffer</param>
        /// <param name="startPos">first position</param>
        /// <param name="endPos">last position, -1 if end position corresponds to bytes size</param>
        /// <returns></returns>
        public static int FindSyncBytePosition(byte[] bytes, int startPos = 0, int endPos = -1)
        {
            var pos = startPos;
            if (endPos == -1)
            {
                endPos = bytes.Length;
            }
            if (bytes.Length == 188 && bytes[pos] == MPEGTSSyncByte)
            {
                return 0;
            }

            while (pos + 188 < endPos)
            {
                if (bytes[pos] != MPEGTSSyncByte)
                {
                    // bad position
                    //Console.WriteLine("Looking for sync byte .....");

                    pos++;
                    continue;
                }

                // is next byte sync byte?
                if (bytes[pos + 188] != MPEGTSSyncByte)
                {
                    pos++;
                    continue;
                }

                return pos;
            }

            return -1;
        }

        /// <summary>
        /// Finding sync byte position
        /// </summary>
        /// <param name="bytes">byte buffer</param>
        /// <param name="startPos">first position</param>
        /// <param name="endPos">last position, -1 if end position corresponds to bytes size</param>
        /// <returns></returns>
        public static int FindSyncBytePosition(List<byte> bytes, int startPos = 0, int endPos = -1)
        {
            return FindSyncBytePosition(bytes.ToArray(), startPos, endPos);
        }

        public static List<MPEGTransportStreamPacket> Parse(byte[] bytes, int PIDFilter = -1)
        {
            return Parse(new List<byte>(bytes), PIDFilter);
        }

        public static List<MPEGTransportStreamPacket> Parse(List<byte> bytes, int PIDFilter = -1)
        {
            return Parse(bytes.ToArray(), 0, bytes.Count, PIDFilter);
        }

        public static void SavePacketsToFile(List<MPEGTransportStreamPacket> packets, string fileName)
        {
            foreach (var packet in packets)
            {
                using (var fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
                {
                    fs.Write(packet.Header.ToArray(), 0, packet.Header.Count);
                    fs.Write(packet.Payload.ToArray(), 0, packet.Payload.Count);
                }
            }
        }

        public static List<MPEGTransportStreamPacket> Parse(byte[] bytes, int startPos, int endPos = -1, int PIDFilter = -1)
        {
            var pos = FindSyncBytePosition(bytes, startPos, endPos);
            if (endPos == -1)
            {
                endPos = bytes.Length;
            }

            var res = new List<MPEGTransportStreamPacket>();

            if (pos == -1)
                return res;

            var buff = new byte[188];

            while (pos + 188 <= endPos)
            {
                for (var i=0;i<188;i++)
                {
                    buff[i] = bytes[pos + i];
                }

                var packet = new MPEGTransportStreamPacket();
                packet.ParseBytes(buff);

                if (
                        (PIDFilter == -1)  // add all packets
                        ||
                        ((PIDFilter != -1) && (packet.PID == PIDFilter))
                   )
                {
                    res.Add(packet);
                }

                pos += 188;
            }

            return res;
        }

        public static SortedDictionary<long, List<MPEGTransportStreamPacket>> SortPacketsByPID(IEnumerable<MPEGTransportStreamPacket> packets)
        {
            var packetsByPID = new SortedDictionary<long, List<MPEGTransportStreamPacket>>();

            foreach (var packet in packets)
            {
                if (!packetsByPID.ContainsKey(packet.PID))
                {
                    packetsByPID.Add(packet.PID, new List<MPEGTransportStreamPacket>());
                }

                packetsByPID[packet.PID].Add(packet);
            }

            return packetsByPID;
        }

        public static Dictionary<ServiceDescriptor, long> GetAvailableServicesMapPIDs(SDTTable sDTTable, PSITable pSITable)
        {
            var res = new Dictionary<ServiceDescriptor, long>();

            foreach (var sdi in sDTTable.ServiceDescriptors)
            {
                foreach (var pr in pSITable.ProgramAssociations)
                {
                    if (pr.ProgramNumber == sdi.ProgramNumber)
                    {
                        res.Add(sdi, pr.ProgramMapPID);
                        break;
                    }
                }
            }

            return res;
        }

        public static PMTTable GetPMTTable(IEnumerable<MPEGTransportStreamPacket> packets, SDTTable sDTTable, PSITable pSITable)
        {
            var servicesMapPIDs = GetAvailableServicesMapPIDs(sDTTable, pSITable);

            foreach (var kvp in servicesMapPIDs)
            {
                var pmtTable = DVBTTable.CreateFromPackets<PMTTable>(packets, kvp.Value);

                if (pmtTable != null)
                {
                    return pmtTable;
                }
            }

            return null;
        }

        public static ulong GetFirstPacketPCRTimeStamp(IEnumerable<MPEGTransportStreamPacket> packets, long PCRPID)
        {
            // find first packet with PCR flag
            foreach (var packet in packets)
            {
                if (packet.PID == PCRPID && packet.PCRFlag)
                {
                    var msTime = packet.GetPCRClock().Value / 27000000;

                    return msTime;
                }
            }

            return ulong.MinValue;
        }
    }
}

