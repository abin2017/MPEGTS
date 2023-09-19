using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LoggerService;
using MPEGTS;

namespace MPEGTSStreamer
{
    public class MPEGTSStreamer
    {
        public IPEndPoint _endPoint { get; set; }

        public const int UDPMTU = 1316;            // MTU => Maximum Transmission Unit (UDP mtu is limited in VLC 3.0 to 1316)
        private const int MinBufferSize = 50;      // ~  2 kb/s
        private const int MaxBufferSize = 1250000; // ~ 50 Mb/s

        private UdpClient _UDPClient = null;
        private ILoggingService _loggingService = new BasicLoggingService();

        public MPEGTSStreamer(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            _UDPClient = new UdpClient();
        }

        /// <summary>
        /// Setting EndPoint from string "IP:Port"
        /// </summary>
        /// <param name="ipAndPort">Ip and port.</param>
        public void SetEndpoint(string ipAndPort)
        {
            _loggingService.Info($"Setting streaming url: udp://@{ipAndPort}");


            var ipPort = ipAndPort.Split(':');
            _endPoint = new IPEndPoint(IPAddress.Parse(ipPort[0]), Convert.ToInt32(ipPort[1]));
        }

        private void SendByteArray(byte[] array, int count)
        {
            try
            {
                if (_UDPClient == null || _endPoint == null)
                    return;

                if (array != null && count > 0)
                {
                    var bufferPart = new byte[UDPMTU];
                    var bufferPartSize = 0;
                    var bufferPos = 0;

                    while (bufferPos < count)
                    {
                        if (bufferPos + UDPMTU <= count)
                        {
                            bufferPartSize = UDPMTU;
                        }
                        else
                        {
                            bufferPartSize = count - bufferPos;
                        }

                        Buffer.BlockCopy(array, bufferPos, bufferPart, 0, bufferPartSize);
                        _UDPClient.Send(bufferPart, bufferPartSize, _endPoint);
                        bufferPos += bufferPartSize;
                    }
                }

            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
        }

        private static string GetComputedSProgress(int totalBytesRead, long totalLength)
        {
            return $"{(totalBytesRead / (totalLength / 100.00)).ToString("N2")}% ";
        }

        private int FindSyncBytePosition(FileStream fs)
        {
            if (!fs.CanRead)
                return -1;

            if (fs.Length < 188 * 2)
                return -1;

            var buff = new byte[188 * 2];
            var bytesRead = fs.Read(buff, 0, 188 * 2);

            if (bytesRead < 188 * 2)
                return -1;

            var pos = 0;

            while (pos < 188)
            {
                if (
                        (buff[pos] == MPEGTransportStreamPacket.MPEGTSSyncByte) &&
                        (buff[pos + 188] == MPEGTransportStreamPacket.MPEGTSSyncByte)
                   )
                {
                    fs.Position = pos;
                    return pos;

                }
            }

            return -1;
        }

        private int GetCorrectedBufferSize(int bufferSize)
        {
            // divisible by 188
            while (bufferSize % 188 != 0)
            {
                bufferSize++;
            }

            return bufferSize;
        }

        private int CalculateNewBufferSize(int bufferSize, TimeSpan timeShift, DateTime TDTTime, DateTime lastTDTTime, double loopsPerSecond)
        {
            var expectedTime = TDTTime.Add(timeShift);
            var timeDiff = DateTime.Now - expectedTime;

            if (timeDiff.TotalMilliseconds != 0)
            {
                var timeSpanFromLastTDT = DateTime.Now - lastTDTTime;
                if (timeSpanFromLastTDT.TotalSeconds > 1)
                {
                    var missingBytes = (timeDiff.TotalSeconds / loopsPerSecond) * bufferSize;
                    var bytesTransferedFromLastTDT = (timeSpanFromLastTDT.TotalSeconds / loopsPerSecond) * bufferSize;

                    var bytesTransferedFromLastTDTWithMissingBytes = bytesTransferedFromLastTDT + missingBytes;

                    var newBufferSize = Convert.ToInt32(bytesTransferedFromLastTDTWithMissingBytes / (timeSpanFromLastTDT.TotalSeconds / loopsPerSecond));

                    if (newBufferSize > MaxBufferSize)
                    {
                        _loggingService.Debug($" .. cannot increase buffer size to {newBufferSize} KB!");
                        bufferSize = GetCorrectedBufferSize(MaxBufferSize);
                    }
                    else if (newBufferSize < MinBufferSize)
                    {
                        _loggingService.Debug($" .. cannot decrease buffer size to {newBufferSize} KB!");
                        bufferSize = GetCorrectedBufferSize(MinBufferSize);
                    }
                    else
                    {
                        var newBufferSpeed = GetHumanReadableSize((newBufferSize * loopsPerSecond) * 8) + "/sec";

                        if (newBufferSize > bufferSize)
                        {
                            _loggingService.Debug($" .. >>> increasing buffer size to: {bufferSize / 1024} KB  (~{newBufferSpeed}) [timeDiff: {GetHumanReadableTimeSpan(timeDiff)}]");
                        }
                        else
                        if (newBufferSize < bufferSize)
                        {
                            _loggingService.Debug($" .. <<< decreasing buffer size to: {bufferSize / 1024} KB  (~{newBufferSpeed}) [timeDiff: {GetHumanReadableTimeSpan(timeDiff)}]");
                        }

                        bufferSize = GetCorrectedBufferSize(newBufferSize);
                    }
                }
            }

            return bufferSize;
        }

        private string GetHumanReadableTimeSpan(TimeSpan span)
        {
            var res = "";

            if (span.Days > 0)
            {
                res += $"{span.Days} days ";
            }
            res += $"{span.Hours.ToString().PadLeft(2, '0')}:";
            res += $"{span.Minutes.ToString().PadLeft(2, '0')}:";
            res += $"{span.Seconds.ToString().PadLeft(2, '0')}";

            return res;
        }

        private string GetHumanReadableSize(double bytes, bool highPrecision = false)
        {
            var frm = highPrecision ? "N2" : "N0";

            if (bytes > 1000000)
            {
                return Math.Round(bytes / 1000000.0, 2).ToString(frm) + " MB";
            }

            if (bytes > 1000)
            {
                return Math.Round(bytes / 1000.0, 2).ToString(frm) + " kB";
            }

            return bytes.ToString(frm) + " B";
        }

        public Dictionary<DateTime,double> CalculateBitRate(string fileName, double initialMegaBitsSpeed = 4.0)
        {
            _loggingService.Info($"Analyzing bitrate of file: {fileName}");
            var res = new Dictionary<DateTime, double>();

            var startTime = DateTime.Now;

            var bufferSize = 100000;

            var buffer = new byte[MaxBufferSize];;
            var lastTDTTime = DateTime.MinValue;
            var bytesReadFromLastTDT = 0;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                FindSyncBytePosition(fs);

                var totalBytesRead = 0;
                while (fs.CanRead && totalBytesRead < fs.Length)
                {
                    // reading data

                    var bytesRead = fs.Read(buffer, 0, bufferSize);

                    totalBytesRead += bytesRead;
                    bytesReadFromLastTDT += bytesRead;

                    if (bytesRead > 0)
                    {
                        var packets = MPEGTransportStreamPacket.Parse(buffer, 0, bytesRead);
                        var tdtTable = DVBTTable.CreateFromPackets<TDTTable>(packets, 20);
                        if (tdtTable != null && tdtTable.UTCTime != DateTime.MinValue)
                        {
                            if (lastTDTTime != DateTime.MinValue && (tdtTable.UTCTime - lastTDTTime).TotalSeconds > 1)
                            {
                                var timeShiftFromLastTDT = tdtTable.UTCTime - lastTDTTime;
                                var bitsPerSec = (bytesReadFromLastTDT / timeShiftFromLastTDT.TotalSeconds) * 8;

                                var speedAndPosition = GetComputedSProgress(totalBytesRead, fs.Length);
                                var bitRate = $"continuous bitrate: { GetHumanReadableSize(bitsPerSec, true)}/ sec";
                                _loggingService.Debug($"Analzying stream {Path.GetFileName(fileName)}: {speedAndPosition} {bitRate}");

                                res.Add(tdtTable.UTCTime, bitsPerSec);
                            }

                            lastTDTTime = tdtTable.UTCTime;
                            bytesReadFromLastTDT = 0;
                        }
                    }
                }
            }

            _loggingService.Debug($"Analyzing time {GetHumanReadableTimeSpan(DateTime.Now - startTime)}");

            return res;
        }

        public void Stream(string fileName, double initialMegaBitsSpeed = 4.0, double loopsPerSecond = 3)
        {
            _loggingService.Info($"Streaming file: {fileName}");

            var bufferSize = GetCorrectedBufferSize(Convert.ToInt32((initialMegaBitsSpeed * 1000000 / 8) / loopsPerSecond));

            var buffer = new byte[MaxBufferSize];
            var lastSpeedCalculationTime = DateTime.MinValue;
            var lastSpeedCalculationTimeLog = DateTime.MinValue;
            var lastTDTTime = DateTime.MinValue;
            var speedAndPosition = "";
            var timeShift = TimeSpan.MinValue;
            var streamStartTime = DateTime.MinValue;

            SDTTable sDTTable = null;
            PSITable psiTable = null;
            PMTTable pmtTable = null;

            var lastPCRTimeStamp = ulong.MinValue;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                FindSyncBytePosition(fs);

                streamStartTime = DateTime.Now;

                var totalBytesRead = 0;
                while (fs.CanRead && totalBytesRead < fs.Length)
                {
                    if ((DateTime.Now - lastSpeedCalculationTime).TotalMilliseconds > (1000/ loopsPerSecond))
                    {
                        // calculate speed & progress

                        lastSpeedCalculationTime = DateTime.Now;

                        speedAndPosition = GetComputedSProgress(totalBytesRead, fs.Length) + " " + GetHumanReadableSize((bufferSize * loopsPerSecond) * 8, true)+"/sec";

                        // reading data

                        var bytesRead = fs.Read(buffer, 0, bufferSize);

                        totalBytesRead += bytesRead;

                        // sending data to UDP

                        SendByteArray(buffer, bytesRead);

                        // calculating buffer size for balancing bitrate

                        if (bytesRead > 0)
                        {
                            var packets = MPEGTransportStreamPacket.Parse(buffer, 0, bytesRead);

                            var tdtTable = DVBTTable.CreateFromPackets<TDTTable>(packets, 20);
                            /*
                            if (tdtTable != null && tdtTable.UTCTime != DateTime.MinValue)
                            {
                                _loggingService.Debug($" .. !!!!!!!! TDT table time: {tdtTable.UTCTime}");

                                if (timeShift != TimeSpan.MinValue)
                                {
                                    bufferSize = CalculateNewBufferSize(bufferSize, timeShift, tdtTable.UTCTime, lastTDTTime, loopsPerSecond);
                                }

                                timeShift = DateTime.Now - tdtTable.UTCTime;
                                lastTDTTime = DateTime.Now;
                            }
                            */
                            if (psiTable == null)
                            {
                                psiTable = DVBTTable.CreateFromPackets<PSITable>(packets, 0);
                            }
                            if (sDTTable == null)
                            {
                                sDTTable = DVBTTable.CreateFromPackets<SDTTable>(packets, 17);
                            }
                            if (pmtTable == null && psiTable != null && sDTTable != null)
                            {
                                var servicesMapPIDs = MPEGTransportStreamPacket.GetAvailableServicesMapPIDs(sDTTable, psiTable);
             
                                foreach (var kvp in servicesMapPIDs)
                                {
                                    pmtTable = DVBTTable.CreateFromPackets<PMTTable>(packets, kvp.Value);

                                    if (pmtTable != null)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (pmtTable != null)
                            {
                                // find first packet with PCR flag
                                foreach (var packet in packets)
                                {
                                    if (packet.PID == pmtTable.PCRPID && packet.PCRFlag)
                                    {
                                        var msTime = packet.GetPCRClock().Value / 27000000;
                                        speedAndPosition += $" (msTime: {msTime})";
                                        break;
                                    }
                                }
                            }
                        }

                        speedAndPosition += $" (exec time: {((DateTime.Now - lastSpeedCalculationTime).TotalMilliseconds).ToString("N2")} ms)";
                        speedAndPosition += $" bytes per 1/{loopsPerSecond} sec: {GetHumanReadableSize(bytesRead)}";
                    }

                    if ((DateTime.Now - lastSpeedCalculationTimeLog).TotalMilliseconds > 1000)
                    {
                        lastSpeedCalculationTimeLog = DateTime.Now;
                        _loggingService.Debug($"Streaming {Path.GetFileName(fileName)}: {GetHumanReadableTimeSpan(DateTime.Now - streamStartTime)} {speedAndPosition}");
                    }
                }
            }
        }
    }
}
