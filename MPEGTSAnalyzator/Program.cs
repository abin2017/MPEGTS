using System;
using System.Collections.Generic;
using System.IO;
using MPEGTS;
using LoggerService;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace MPEGTSAnalyzator
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //var packetBytes = File.ReadAllBytes($@"C:\Users\Jean Luc Picard\source\repos\MPEGTS\MPEGTSTests\TestData\EIT.IT.bin");
            //var packets = MPEGTransportStreamPacket.Parse(packetBytes);
            //var EIT = DVBTTable.CreateFromPackets<EITTable>(packets, 18);

            if (args != null &&
                args.Length >= 0 &&
                FileNameParamValue(args) != null)
            {
                var fName = FileNameParamValue(args);
                AnalyzeMPEGTSPackets(fName, EITParamValue(args));
            }
            else
            {
                Console.WriteLine("MPEGTSAnalyzator");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine();
                Console.WriteLine("MPEGTSAnalyzator.exe file.ts [--eit]");
                Console.WriteLine();
                Console.WriteLine();

#if DEBUG
                AnalyzeMPEGTSPackets("TestData.CZ" + Path.DirectorySeparatorChar + "CT2.ts", false);

                Console.WriteLine("Press Enter");
                Console.ReadLine();
#endif

            }
        }

        public static bool EITParamValue(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.ToLower() == "--eit")
                {
                    return true;
                }
            }

            return false;
        }

        public static string FileNameParamValue(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.ToLower() != "--eit")
                {
                    return arg;
                }
            }

            return null;
        }

        public static void AnalyzeMPEGTSPackets(string path, bool includeEIT = true)
        {
            var logger = new FileLoggingService(LoggingLevelEnum.Debug);
            logger.LogFilename = "Log.log";

            Console.Write($"Reading data... ");

            var bytes = LoadBytesFromFile(path);

            Console.WriteLine();
            Console.Write($"Parsing packets... ");

            var packets = MPEGTransportStreamPacket.Parse(bytes);

            Console.WriteLine($" {packets.Count} packets found");


            var packetsByPID = MPEGTransportStreamPacket.SortPacketsByPID(packets);

            Console.WriteLine();
            Console.WriteLine($"PID:             Packets count");
            Console.WriteLine("-------------------------------");

            SDTTable sDTTable = null;
            PSITable psiTable = null;
            PMTTable pmtTable = null;

            foreach (var kvp in packetsByPID)
            {
                Console.WriteLine($"{kvp.Key,6} ({"0x" + Convert.ToString(kvp.Key, 16),6}): {kvp.Value.Count,8}");
            }

            if (packetsByPID.ContainsKey(17))
            {
                Console.WriteLine();
                Console.WriteLine($"Service Description Table(SDT):");
                Console.WriteLine($"------------------------------");

                sDTTable = DVBTTable.CreateFromPackets<SDTTable>(packetsByPID[17], 17);  // PID 0x11, Service Description Table (SDT)

                if (sDTTable != null)
                    sDTTable.WriteToConsole();
            }

            if (packetsByPID.ContainsKey(16))
            {
                Console.WriteLine();
                Console.WriteLine($"Network Information Table (NIT):");
                Console.WriteLine($"--------------------------------");


                var niTable = DVBTTable.CreateFromPackets<NITTable>(packetsByPID[16], 16);

                if (niTable != null)
                    niTable.WriteToConsole();
            }

            if (packetsByPID.ContainsKey(20))
            {
                Console.WriteLine();
                Console.WriteLine($"First/last Time and Date Table (TDT):");
                Console.WriteLine($"--------------------------------");

                var tdtTables = DVBTTable.CreateAllFromPackets<TDTTable>(packetsByPID[20], 20);

                if (tdtTables != null &&
                    tdtTables.Count > 0)
                {
                    var first = tdtTables[0];
                    var last = tdtTables[tdtTables.Count - 1];

                    first.WriteToConsole();
                    last.WriteToConsole();

                    var timeSpan = last.UTCTime - first.UTCTime;
                    Console.WriteLine($"TimeSpan: {timeSpan.ToString()}");
                }
            }

            if (packetsByPID.ContainsKey(0))
            {
                Console.WriteLine();
                Console.WriteLine($"Program Specific Information(PSI):");
                Console.WriteLine($"----------------------------------");

                psiTable = DVBTTable.CreateFromPackets<PSITable>(packetsByPID[0], 0);

                if (psiTable != null)
                    psiTable.WriteToConsole();
            }

            if ((psiTable != null) &&
                (sDTTable != null))
            {
                Console.WriteLine();
                Console.WriteLine($"Program Map Table (PMT):");
                Console.WriteLine($"----------------------------------");
                Console.WriteLine();

                var servicesMapPIDs = MPEGTransportStreamPacket.GetAvailableServicesMapPIDs(sDTTable, psiTable);

                Console.WriteLine($"{"Program name".PadRight(40,' '),40} {"Program number",14} {"     PID",8}");
                Console.WriteLine($"{"------------".PadRight(40,' '),40} {"--------------",14} {"--------"}");

                // scan PMT for each program number
                foreach (var kvp in servicesMapPIDs)
                {
                    Console.WriteLine($"{kvp.Key.ServiceName.PadRight(40, ' ')} {kvp.Key.ProgramNumber,14} {kvp.Value,8}");

                    // stream contains this Map PID

                    if (packetsByPID.ContainsKey(kvp.Value))
                    {
                        pmtTable = DVBTTable.CreateFromPackets<PMTTable>(packetsByPID[kvp.Value], kvp.Value);
                        if (pmtTable != null)
                        {
                            pmtTable.WriteToConsole();
                        }
                    }

                }
            }

            if (pmtTable != null && pmtTable.PCRPID != 0)
            {
                Console.WriteLine();
                Console.WriteLine($"PCR - Program Clock Reference:");
                Console.WriteLine($"--------------------------------");
                ulong minPCR = ulong.MaxValue;
                ulong maxPCR = ulong.MinValue;
                ulong pcrPacketsCount = 0;

                foreach (var packet in packets)
                {
                    if (packet.PID == pmtTable.PCRPID && packet.PCRFlag)
                    {
                        pcrPacketsCount++;
                        var msTime = packet.GetPCRClock().Value / 27000000;

                        MPEGTransportStreamPacket.SavePacketsToFile(new List<MPEGTransportStreamPacket>() { packet }, @"c:\temp\packet001.bin");

                        Console.WriteLine($"PCR: {msTime}");

                        if (msTime < minPCR)
                        {
                            minPCR = msTime;
                        }
                        if (msTime > maxPCR)
                        {
                            maxPCR = msTime;
                        }
                    }
                }
                Console.WriteLine($"PCR PID:         {pmtTable.PCRPID}");
                Console.WriteLine($"Packets count:   {pcrPacketsCount}");
                Console.WriteLine($"Min:             {minPCR}");
                Console.WriteLine($"Max:             {maxPCR}");
                Console.WriteLine($"Duration:        {(maxPCR - minPCR) / 60}:{(maxPCR - minPCR)-((maxPCR-minPCR)/60)*60}");
            }

            if (includeEIT)
            {
                if (packetsByPID.ContainsKey(18))
                {
                    Console.WriteLine();
                    Console.WriteLine($"Event Information Table (EIT):");
                    Console.WriteLine($"------------------------------");

                    var eitPackets = MPEGTransportStreamPacket.GetFilteredPackets(packetsByPID[18], 18);

                    foreach (var kvp in eitPackets)
                    {
                        var eitTable = DVBTTable.CreateFromPackets<EITTable>(kvp.Value, 18);

                        if (eitTable != null)
                        {

                            foreach (var ev in eitTable.EventItems)
                            {
                                Console.WriteLine(ev.WriteToString());
                            }
                        }
                    }
                }
            }
        }

        public static List<byte> LoadBytesFromFile(string path)
        {
            var buffSize = 1024 * 1024;
            byte[] buffer = new byte[buffSize];
            byte[] packetBuffer = new byte[188];
            var streamBytes = new List<byte>();

            using (var fs = new FileStream(path, FileMode.Open))
            {
                while (fs.Position + buffSize < fs.Length)
                {
                    fs.Read(buffer, 0, buffSize);
                    streamBytes.AddRange(buffer);
                }
                while (fs.Position + 188 < fs.Length)
                {
                    fs.Read(packetBuffer, 0, 188);
                    streamBytes.AddRange(packetBuffer);
                }
                fs.Close();
            }

            return streamBytes;
        }
    }
}
