using System;
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

        private static string GetComputedSpeed(int bitsPerSec)
        {
            if (bitsPerSec > 1000000)
            {
                return $" {Math.Round((bitsPerSec / 1000000.0), 2).ToString("N2")} Mb/sec";
            }
            else if (bitsPerSec > 1000)
            {
                return $" {Math.Round((bitsPerSec / 1000.0), 2).ToString("N2")} Kb/sec";
            }
            else
            {
                return $" {bitsPerSec} b/sec";
            }
        }

        private static string GetComputedSProgress(int totalBytesRead, long totalLength)
        {
            return $"{Math.Round(totalBytesRead / (totalLength / 100.00), 2)}% ";
        }

        private int FindSyncBytePosition(FileStream fs)
        {
            if (!fs.CanRead)
                return -1;

            if (fs.Length<188*2)
                return -1;

            var buff = new byte[188*2];
            var bytesRead = fs.Read(buff, 0, 188*2);

            if (bytesRead<188*2)
                return -1;

            var pos = 0;

            while (pos < 188)
            {
                if (
                        (buff[pos] == MPEGTransportStreamPacket.MPEGTSSyncByte) &&
                        (buff[pos+188] == MPEGTransportStreamPacket.MPEGTSSyncByte)
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

        private int CalculateNewBufferSize(int bufferSize, TimeSpan timeShift, DateTime TDTTime, DateTime lastTDTTime)
        {
            var expectedTime = TDTTime.Add(timeShift);
            var timeDiff = DateTime.Now - expectedTime;

            if (timeDiff.TotalMilliseconds != 0)
            {
                var timeSpanFromLastTDT = DateTime.Now - lastTDTTime;
                if (timeSpanFromLastTDT.TotalSeconds > 1)
                {
                    var missingBytes = (timeDiff.TotalSeconds / 5.0) * bufferSize;
                    var bytesTransferedFromLastTDT = (timeSpanFromLastTDT.TotalSeconds / 5.0) * bufferSize;

                    var bytesTransferedFromLastTDTWithMissingBytes = bytesTransferedFromLastTDT + missingBytes;

                    var newBufferSize = Convert.ToInt32(bytesTransferedFromLastTDTWithMissingBytes / (timeSpanFromLastTDT.TotalSeconds / 5));

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
                        var newBufferSpeed = GetComputedSpeed((newBufferSize * 5) * 8);

                        if (newBufferSize > bufferSize)
                        {
                            _loggingService.Debug($" .. >>> increasing buffer size to: {bufferSize / 1024} KB  (~{newBufferSpeed}) [timeDiff: {timeDiff.TotalMilliseconds.ToString("N2")}]");
                        }
                        else
                        if (newBufferSize < bufferSize)
                        {
                            _loggingService.Debug($" .. <<< decreasing buffer size to: {bufferSize / 1024} KB  (~{newBufferSpeed}) [timeDiff: {timeDiff.TotalMilliseconds.ToString("N2")}]");
                        }

                        bufferSize = GetCorrectedBufferSize(newBufferSize);
                    }
                }
            }

            return bufferSize;
        }

        public void Stream(string fileName, double initialMegaBitsSpeed = 4.0)
        {
            _loggingService.Info($"Streaming file: {fileName}");

            var bufferSize = GetCorrectedBufferSize(Convert.ToInt32((initialMegaBitsSpeed*1000000/8)/5));

            var buffer = new byte[MaxBufferSize];
            var lastSpeedCalculationTime = DateTime.MinValue;
            var lastSpeedCalculationTimeLog = DateTime.MinValue;
            var lastTDTTime = DateTime.MinValue;
            var speedAndPosition = "";
            var timeShift = TimeSpan.MinValue;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                FindSyncBytePosition(fs);

                var totalBytesRead = 0;
                while (fs.CanRead && totalBytesRead < fs.Length)
                {
                    if ((DateTime.Now - lastSpeedCalculationTime).TotalMilliseconds > 200)
                    {
                        // calculate speed & progress

                        lastSpeedCalculationTime = DateTime.Now;

                        speedAndPosition = GetComputedSProgress(totalBytesRead, fs.Length) + " " + GetComputedSpeed((bufferSize * 5) * 8);

                        var bytesRead = fs.Read(buffer, 0, bufferSize);

                        totalBytesRead += bytesRead;

                        SendByteArray(buffer, bytesRead);

                        // calculating buffer size for balancing bitrate

                        if (bytesRead > 0)
                        {
                            var packets = MPEGTransportStreamPacket.Parse( buffer, 0, bytesRead);
                            var tdtTable = DVBTTable.CreateFromPackets<TDTTable>(packets, 20);
                            if (tdtTable != null && tdtTable.UTCTime != DateTime.MinValue)
                            {
                                _loggingService.Debug($" .. !!!!!!!! TDT table time: {tdtTable.UTCTime}");

                                if (timeShift == TimeSpan.MinValue)
                                {
                                    timeShift = DateTime.Now - tdtTable.UTCTime;
                                    lastTDTTime = DateTime.Now;
                                }
                                else
                                {
                                    bufferSize = CalculateNewBufferSize(bufferSize, timeShift, tdtTable.UTCTime, lastTDTTime);
                                }

                                lastTDTTime = DateTime.Now;
                            }
                        }

                        speedAndPosition += $" (time for parse & send: {(DateTime.Now - lastSpeedCalculationTime).TotalMilliseconds} ms)";
                    }

                    if ((DateTime.Now - lastSpeedCalculationTimeLog).TotalMilliseconds > 1000)
                    {
                        lastSpeedCalculationTimeLog = DateTime.Now;
                        //_loggingService.Debug($"Streaming data: {speedAndPosition}");
                        Console.Out.WriteLine($"Streaming data: {speedAndPosition}");
                    }
                }
            }
        }
    }
}
