using System;
using System.IO;
using System.Text;
using LoggerService;

namespace MPEGTSStreamer
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Help();
                return;
            }

            var fileName = args[0];

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"File {fileName} not found!");
                return;
            }

            var streamer = new MPEGTSStreamer(new NLogLoggingService("NLog.config"));

            if (args.Length == 2)
            {
                streamer.SetEndpoint(args[1]);
            }
            else
            {
                streamer.SetEndpoint("127.0.0.1:1234");
            }

            streamer.Stream(args[0]);

            /*
            var bitRate = streamer.ScanBitRate(args[0], (percent) =>
            {
                Console.WriteLine($"Scanning bitrate: {percent:0.##}%");
            }, 100);


            foreach (var kvp in bitRate)
            {
                Console.WriteLine($"timestamp:{kvp.Key}, bitrate: {kvp.Value.ToString("N2")} Mb/s");
            }
            */

            Console.WriteLine();
            Console.WriteLine($"press ENTER");
            Console.ReadLine();
        }

        public static void Help()
        {
            Console.WriteLine("MPEGTSStreamer");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("MPEGTSStreamer.exe file.ts [IP:Port]");
            Console.WriteLine();
            Console.WriteLine("   default endpoint is 127.0.0.1:1234");
            Console.WriteLine();
        }
    }
}
