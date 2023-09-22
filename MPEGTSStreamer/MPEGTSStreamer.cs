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
        private const int MinBufferSize = 188*20;
        private const int MaxBufferSize = 1250000;

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

        private static int FindSyncBytePosition(FileStream fs)
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

        private long FindPCRPID(string fileName)
        {
            _loggingService.Info($"Finding PCRID...");

            SDTTable sDTTable = null;
            PSITable psiTable = null;
            PMTTable pmtTable = null;

            var allPackets = new List<MPEGTransportStreamPacket>();

            var bufferSize = 188 * 1000;
            var buffer = new byte[bufferSize];

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                FindSyncBytePosition(fs);

                while (fs.CanRead && fs.Position+bufferSize < fs.Length)
                {
                        var bytesRead = fs.Read(buffer, 0, bufferSize);

                        if (bytesRead == bufferSize)
                        {
                            var packets = MPEGTransportStreamPacket.Parse(buffer);
                            allPackets.AddRange(packets);

                            if (psiTable == null)
                            {
                                psiTable = DVBTTable.CreateFromPackets<PSITable>(allPackets, 0);
                            }
                            if (sDTTable == null)
                            {
                                sDTTable = DVBTTable.CreateFromPackets<SDTTable>(allPackets, 17);
                            }
                            if (pmtTable == null && psiTable != null && sDTTable != null)
                            {
                                pmtTable = MPEGTransportStreamPacket.GetPMTTable(allPackets, sDTTable, psiTable);
                                if (pmtTable != null)
                                {
                                    return pmtTable.PCRPID;
                                }
                            }
                        } else
                        {
                            return -1;
                        }
                }
            }

            return -1;
        }

        /// <summary>
        /// Stream file to UDP
        /// </summary>
        /// <param name="fileName">Full file name</param>
        /// <param name="initialMegaBitsSpeed">Initial Mb/s speed fro streaming</param>
        /// <param name="loopsPerSecond">Data read & send interval per second</param>
        /// <param name="speedCorectionSecondsInterval">Data speed correction interval in seconds</param>
        public void Stream(string fileName, double initialMegaBitsSpeed = 4.0, double loopsPerSecond = 10, double speedCorectionSecondsInterval = 5)
        {
            _loggingService.Info($"Streaming file: {fileName}");

            var PCRPID = FindPCRPID(fileName);
            _loggingService.Info($"...{PCRPID}");

            if (PCRPID == -1)
            {
                throw new Exception("No PCR PID found");
            }

            var bufferSize = GetCorrectedBufferSize(Convert.ToInt32((initialMegaBitsSpeed * 1000000 / 8) / loopsPerSecond));
            var buffer = new byte[MaxBufferSize];            // buffer size for every 1/loopsPerSecond per second
            int bytesRead = 0;

            var lastReadAndSendTime = DateTime.MinValue; // occurs every 1/loopsPerSecond per second
            var lastSpeedCorrectionTime = DateTime.MinValue; // occurs every speedCorectionSecondsInterval second

            var lastSpeedCalculationLogTime = DateTime.MinValue;
            var speedAndPosition = "";
            var PCR = "";

            var firstPCRTimeStamp = ulong.MinValue;
            var firstPCRTimeStampTime = DateTime.MinValue;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var streamStartTime = DateTime.Now;

                FindSyncBytePosition(fs);

                var totalBytesRead = 0;
                while (fs.CanRead && totalBytesRead < fs.Length)
                {
                    if ((DateTime.Now - lastReadAndSendTime).TotalMilliseconds > (1000/ loopsPerSecond))
                    {
                        // reading data

                        bytesRead = fs.Read(buffer, 0, bufferSize);
                        totalBytesRead += bytesRead;

                        // sending data to UDP

                        SendByteArray(buffer, bytesRead);

                        // logging progress

                        speedAndPosition = GetComputedSProgress(totalBytesRead, fs.Length) + " " + GetHumanReadableSize((bufferSize * loopsPerSecond) * 8, true) + "/sec";
                        speedAndPosition += $" (exec time: {((DateTime.Now - lastReadAndSendTime).TotalMilliseconds).ToString("N2")} ms)";
                        speedAndPosition += $" bytes per 1/{loopsPerSecond} sec: {GetHumanReadableSize(bytesRead)}";

                        lastReadAndSendTime = DateTime.Now;
                    }

                    if ((DateTime.Now - lastSpeedCorrectionTime).TotalSeconds >= speedCorectionSecondsInterval)
                    {
                        // speed correction

                        if (bytesRead > 0 && PCRPID >= 0)
                        {
                            var packets = MPEGTransportStreamPacket.Parse(buffer, 0, bytesRead);

                            var timeStamp = MPEGTransportStreamPacket.GetFirstPacketPCRTimeStamp(packets, PCRPID);
                            if (timeStamp != ulong.MinValue)
                            {
                                if (firstPCRTimeStamp == ulong.MinValue)
                                {
                                    firstPCRTimeStamp = timeStamp;
                                    firstPCRTimeStampTime = DateTime.Now;
                                }
                                else
                                {
                                    var streamTimeSpan = DateTime.Now - firstPCRTimeStampTime;
                                    var dataTime = timeStamp - firstPCRTimeStamp;
                                    var shift = (streamTimeSpan).TotalSeconds - (dataTime);
                                    //var speedCorrectionLShiftPerSec = shift / (streamTimeSpan).TotalSeconds;
                                    var missingBytesForWholeStream = Math.Round((shift / loopsPerSecond) * bufferSize, 2);
                                    PCR = $" (PCR time shift: {Math.Round(shift, 2).ToString("N2")} s, missingBytes: {GetHumanReadableSize(missingBytesForWholeStream)})";

                                    var newBufferSize = GetCorrectedBufferSize(Convert.ToInt32(bufferSize + missingBytesForWholeStream));

                                    if (newBufferSize > bufferSize)
                                    {
                                        if (newBufferSize > MaxBufferSize)
                                        {
                                            PCR += $" cannot increase buffer to {GetHumanReadableSize(newBufferSize)}";
                                            newBufferSize = MaxBufferSize;
                                        }

                                        PCR += $" >>> {GetHumanReadableSize(newBufferSize)}";

                                    } else
                                    if (newBufferSize < bufferSize)
                                    {
                                        if (newBufferSize < MinBufferSize)
                                        {
                                            PCR += $" cannot decrease buffer to {GetHumanReadableSize(newBufferSize)}";
                                            newBufferSize = MinBufferSize;
                                        }

                                        PCR += $" <<< {GetHumanReadableSize(newBufferSize)}";
                                    }

                                    bufferSize = GetCorrectedBufferSize(Convert.ToInt32(newBufferSize));
                                }
                            }
                        }

                        lastSpeedCorrectionTime = DateTime.Now;
                    }

                    if ((DateTime.Now - lastSpeedCalculationLogTime).TotalMilliseconds > 1000)
                    {
                        // logging

                        lastSpeedCalculationLogTime = DateTime.Now;
                        _loggingService.Debug($"Streaming {Path.GetFileName(fileName)}: {GetHumanReadableTimeSpan(DateTime.Now - streamStartTime)} {speedAndPosition} {PCR}");
                    }
                }
            }
        }

        public Dictionary<ulong,double> ScanBitRate(string fileName, Action<double> percentChanged = null,  int MBsToRead = 10)
        {
            _loggingService.Info($"Scanning bitrate: file: {fileName}");

            var PCRPID = FindPCRPID(fileName);
            _loggingService.Info($"...{PCRPID}");

            if (PCRPID == -1)
            {
                throw new Exception("No PCR PID found");
            }

            var res = new Dictionary<ulong, double>();

            var timeStampPcaketsCount = new Dictionary<ulong, int>();

            var bufferSize = 188 * 1000;
            var buffer = new byte[bufferSize];

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length == 0)
                    throw new Exception("No data");

                var maxStreamPos =Convert.ToInt64(MBsToRead) * 1000000;
                if (maxStreamPos>fs.Length)
                {
                    maxStreamPos = fs.Length;
                }

                ulong lastTimeStamp = ulong.MinValue;
                long currentTimeStampReadBytes = 0;

                FindSyncBytePosition(fs);

                while (fs.CanRead && fs.Position + bufferSize < maxStreamPos)
                {
                    if (percentChanged != null)
                    {
                        var perc = fs.Position / (maxStreamPos / 100.00);
                        percentChanged(perc);
                    }

                    // reading data
                    var bytesRead = fs.Read(buffer, 0, bufferSize);
                    currentTimeStampReadBytes += bytesRead;

                    if (bytesRead == bufferSize)
                    {
                        var packets = MPEGTransportStreamPacket.Parse(buffer, 0, bytesRead);
                        var timeStamp = MPEGTransportStreamPacket.GetFirstPacketPCRTimeStamp(packets, PCRPID);

                        if (timeStamp != ulong.MinValue && timeStamp != lastTimeStamp)
                        {
                            if (lastTimeStamp != ulong.MinValue)
                            {
                                var bitrate = currentTimeStampReadBytes * 8.0 / 1000000.0;
                                res.Add(lastTimeStamp, bitrate);
                                currentTimeStampReadBytes = 0;
                            }

                            lastTimeStamp = timeStamp;
                        }
                    } else
                    {
                        throw new Exception("No data");
                    }
                }
            }

            if (percentChanged != null)
            {
                percentChanged(100.00);
            }

            return res;
        }
    }
}
