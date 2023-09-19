using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPEGTS;
using System;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class BasicTests
    {
        // https://stackoverflow.com/questions/58716586/how-to-generate-timestamps-from-pcr-in-hex
        [TestMethod]
        public void TestPCRCalculation()
        {
            var packet = new MPEGTransportStreamPacket();
            packet.PCRFlag = true;
            packet.PCR = new List<byte> { 0x00, 0x00, 0x0D, 0xB0, 0x7E, 0x4E };

            var pcrValue = packet.GetPCRClock();

            Assert.AreEqual(Convert.ToUInt64(0x2014CE), pcrValue);
        }
    }
}
