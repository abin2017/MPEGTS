using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPEGTS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests
{
    [TestClass]
    public class PacketTests
    {
        [TestMethod]
        public void TestParsePacketWithAdaptationField()
        {
            using (var fs = new FileStream($"TestData{Path.DirectorySeparatorChar}Packet001.bin", FileMode.Open))
            {
                var buffer = new byte[188];

                fs.Read(buffer, 0, 188);

                var packet = new MPEGTransportStreamPacket();
                packet.ParseBytes(buffer);

                Assert.IsNotNull(packet);

                Assert.AreEqual(AdaptationFieldControlEnum.AdaptationFieldFollowedByPayload, packet.AdaptationFieldControl);
                Assert.IsTrue(packet.PCRFlag);
                Assert.AreEqual(1110, packet.PID);
                Assert.IsFalse(packet.AdaptationFieldExtensionFlag);

                Assert.IsNotNull(packet.Header);
                Assert.AreEqual(4, packet.Header.Count);

                Assert.IsNotNull(packet.AdaptationField);
                Assert.AreEqual(120, packet.AdaptationField.Count);

                Assert.IsNotNull(packet.Payload);
                Assert.AreEqual(188-120-4, packet.Payload.Count);
            }
        }

        [TestMethod]
        public void TestParsePacketWithoutAdaptationField()
        {
            using (var fs = new FileStream($"TestData{Path.DirectorySeparatorChar}NIT.bin", FileMode.Open))
            {
                var buffer = new byte[188];

                fs.Read(buffer, 0, 188);

                var packet = new MPEGTransportStreamPacket();
                packet.ParseBytes(buffer);

                Assert.IsNotNull(packet);

                Assert.AreEqual(AdaptationFieldControlEnum.NoAdaptationFieldPayloadOnly, packet.AdaptationFieldControl);
                Assert.AreEqual(0, packet.AdaptationFieldLength);
                Assert.IsFalse(packet.PCRFlag);
                Assert.AreEqual(16, packet.PID);
                Assert.IsFalse(packet.AdaptationFieldExtensionFlag);

                Assert.IsNotNull(packet.Header);
                Assert.AreEqual(4, packet.Header.Count);

                Assert.IsNotNull(packet.AdaptationField);
                Assert.AreEqual(0, packet.AdaptationField.Count);

                Assert.IsNotNull(packet.Payload);
                Assert.AreEqual(184, packet.Payload.Count);
            }
        }

        [TestMethod]
        public void TestParsePacketWithErrorIndicator()
        {
            using (var fs = new FileStream($"TestData{Path.DirectorySeparatorChar}Packet002.bin", FileMode.Open))
            {
                var buffer = new byte[188];

                fs.Read(buffer, 0, 188);

                var packet = new MPEGTransportStreamPacket();
                packet.ParseBytes(buffer);

                Assert.IsNotNull(packet);
                Assert.IsTrue(packet.TransportErrorIndicator);
            }
        }

        [TestMethod]
        public void TestParsePacketsWithErrorIndicator()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT3.bin");

            // total 33 packets, one with TransportErrorIndicator=1
            // - first 12 packets (1728 bytes => 12*184) belong together, but one of them has TransportErrorIndicator=1
            // - EIT should be created from next 7 packets (1210 bytes)
            var packets = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packets, 18);

            Assert.IsNotNull(EIT);
            Assert.AreEqual(1210, EIT.Data.Length);
        }
    }
}
