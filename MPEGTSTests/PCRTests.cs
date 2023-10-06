using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPEGTS;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tests
{
    [TestClass]
    public class PCRTests
    {
        // https://stackoverflow.com/questions/58716586/how-to-generate-timestamps-from-pcr-in-hex
        [TestMethod]
        public void TestPCRCalculation()
        {
            var PCR = new List<byte> { 0x00, 0x00, 0x0D, 0xB0, 0x7E, 0x4E };

            var pcrValue = MPEGTransportStreamPacket.GetPCRClock(PCR);

            Assert.AreEqual(Convert.ToUInt64(0x2014CE), pcrValue);
        }

        [TestMethod]
        public void TestPacketPCRCalculation()
        {
            var packet = new MPEGTransportStreamPacket();
            packet.PCRFlag = true;
            packet.PCR = new List<byte> { 0x00, 0x00, 0x0D, 0xB0, 0x7E, 0x4E };

            var pcrValue = packet.GetPCRClock();

            Assert.AreEqual(Convert.ToUInt64(0x2014CE), pcrValue);
        }

        [TestMethod]
        public void TestPCRLoadFromByteArray()
        {
            using (var fs = new FileStream($"TestData{Path.DirectorySeparatorChar}PCRPackets.bin", FileMode.Open))
            {
                var bufferSize = 188 * 10;
                var buffer = new byte[bufferSize];

                fs.Read(buffer, 0, bufferSize);

                var pcrClock = MPEGTransportStreamPacket.GetFirstPCRClock(4041, buffer, 0);

                Assert.AreEqual((ulong)41815, pcrClock);
            }
        }

        [TestMethod]
        public void TestPCRFromPacketWithErrorIndicator()
        {
            var buffer = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}Packet002.bin");
            var pcrClock = MPEGTransportStreamPacket.GetFirstPCRClock(1110, buffer, 0);

            Assert.IsNull(pcrClock);
        }
    }
}
