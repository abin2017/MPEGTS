using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPEGTS;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tests
{
    [TestClass]
    public class PacketTests
    {
        [TestMethod]
        public void TestPacketParse()
        {
            using (var fs = new FileStream($"TestData{Path.DirectorySeparatorChar}Packet001.bin", FileMode.Open))
            {
                var buffer = new byte[188];

                fs.Read(buffer, 0, 188);

                var packet = new MPEGTransportStreamPacket();
                packet.ParseBytes(buffer);

                Assert.IsNotNull(packet);

                Assert.AreEqual(AdaptationFieldControlEnum.AdaptationFieldFollowedByPayload, packet.AdaptationFieldControl);
                Assert.AreEqual(120, packet.AdaptationFieldLength);
                Assert.IsTrue(packet.PCRFlag);
                Assert.AreEqual(1110, packet.PID);
                Assert.IsFalse(packet.AdaptationFieldExtensionFlag);

                Assert.IsNotNull(packet.Header);
                Assert.AreEqual(4, packet.Header.Count);

                Assert.IsNotNull(packet.Payload);
                Assert.AreEqual(184, packet.Payload.Count);

                Assert.IsNotNull(packet.AdaptationFiled);
                Assert.AreEqual(120, packet.AdaptationFiled.Count);
            }
        }
    }
}
