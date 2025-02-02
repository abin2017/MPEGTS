using LoggerService;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPEGTS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests
{
    [TestClass]
    public class DVBTTablesTests
    {
        [TestMethod]
        public void TestPSI()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}PSI.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);
            var PSITable = DVBTTable.CreateFromPackets<PSITable>(packet, 0);

            Assert.IsNotNull(PSITable);
            Assert.IsTrue(PSITable.CRCIsValid());

            Assert.AreEqual(PSITable.ProgramAssociations.Count, 20);

            var programAssociationDict = new Dictionary<int, int>();
            foreach (var programAssociation in PSITable.ProgramAssociations)
            {
                programAssociationDict.Add(programAssociation.ProgramMapPID, programAssociation.ProgramNumber);
            }
            Assert.AreEqual(0, programAssociationDict[16]);
            Assert.AreEqual(268, programAssociationDict[2100]);
            Assert.AreEqual(270, programAssociationDict[2200]);
            Assert.AreEqual(272, programAssociationDict[2300]);
            Assert.AreEqual(274, programAssociationDict[2400]);
            Assert.AreEqual(276, programAssociationDict[2500]);
            Assert.AreEqual(280, programAssociationDict[2700]);
            Assert.AreEqual(282, programAssociationDict[2800]);
            Assert.AreEqual(284, programAssociationDict[2900]);
            Assert.AreEqual(286, programAssociationDict[3000]);
            Assert.AreEqual(16651, programAssociationDict[7010]);
            Assert.AreEqual(16652, programAssociationDict[7020]);
            Assert.AreEqual(16653, programAssociationDict[7030]);
            Assert.AreEqual(16654, programAssociationDict[7040]);
            Assert.AreEqual(16655, programAssociationDict[7050]);
            Assert.AreEqual(16656, programAssociationDict[7060]);
            Assert.AreEqual(16657, programAssociationDict[7070]);
            Assert.AreEqual(16658, programAssociationDict[7080]);
            Assert.AreEqual(16659, programAssociationDict[7090]);
            Assert.AreEqual(16660, programAssociationDict[7100]);
        }

        [TestMethod]
        public void TestNIT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}NIT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);
            var NITTable = DVBTTable.CreateFromPackets<NITTable>(packet, 16);

            Assert.IsNotNull(NITTable);
            Assert.IsTrue(NITTable.CRCIsValid());

            Assert.AreEqual("CT, MUX 21", NITTable.NetworkName);
            Assert.AreEqual(18, NITTable.ServiceList.Services.Count);
            Assert.AreEqual(18, NITTable.ServiceList.ServiceTypes.Count);

            Assert.AreEqual(ServiceTypeEnum.HEVCDigitalTelevisionService, NITTable.ServiceList.ServiceTypes[268]);
            Assert.AreEqual(ServiceTypeEnum.HEVCDigitalTelevisionService, NITTable.ServiceList.ServiceTypes[270]);
            Assert.AreEqual(ServiceTypeEnum.HEVCDigitalTelevisionService, NITTable.ServiceList.ServiceTypes[272]);
            Assert.AreEqual(ServiceTypeEnum.HEVCDigitalTelevisionService, NITTable.ServiceList.ServiceTypes[274]);
            Assert.AreEqual(ServiceTypeEnum.HEVCDigitalTelevisionService, NITTable.ServiceList.ServiceTypes[276]);
            Assert.AreEqual(ServiceTypeEnum.HEVCDigitalTelevisionService, NITTable.ServiceList.ServiceTypes[280]);
            Assert.AreEqual(ServiceTypeEnum.HEVCDigitalTelevisionService, NITTable.ServiceList.ServiceTypes[282]);

            Assert.AreEqual(ServiceTypeEnum.DigitalTelevisionService, NITTable.ServiceList.ServiceTypes[284]);
            Assert.AreEqual(ServiceTypeEnum.DigitalTelevisionService, NITTable.ServiceList.ServiceTypes[286]);

            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16651]);
            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16652]);
            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16653]);
            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16654]);
            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16655]);
            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16656]);
            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16657]);
            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16658]);
            Assert.AreEqual(ServiceTypeEnum.DigitalRadioSoundService, NITTable.ServiceList.ServiceTypes[16659]);
        }

        [TestMethod]
        public void TestSDT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}SDT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);
            var SDT = DVBTTable.CreateFromPackets<SDTTable>(packet, 17);

            Assert.IsNotNull(SDT);
            Assert.IsTrue(SDT.CRCIsValid());

            Assert.AreEqual(SDT.ServiceDescriptors.Count, 19);

            var descriptorsDict = new Dictionary<int, ServiceDescriptor>();
            foreach (var decriptor in SDT.ServiceDescriptors)
            {
                descriptorsDict.Add(decriptor.ProgramNumber, decriptor);
            }

            Assert.AreEqual("CESKA TELEVIZE", descriptorsDict[268].ProviderName);

            Assert.AreEqual(31, descriptorsDict[268].ServisType);
            Assert.AreEqual(31, descriptorsDict[270].ServisType);
            Assert.AreEqual(31, descriptorsDict[272].ServisType);
            Assert.AreEqual(31, descriptorsDict[274].ServisType);
            Assert.AreEqual(31, descriptorsDict[276].ServisType);
            Assert.AreEqual(31, descriptorsDict[280].ServisType);
            Assert.AreEqual(31, descriptorsDict[282].ServisType);
            Assert.AreEqual(31, descriptorsDict[284].ServisType);
            Assert.AreEqual(31, descriptorsDict[286].ServisType);

            Assert.AreEqual("CT 1 HD T2", descriptorsDict[268].ServiceName);
            Assert.AreEqual("CT 2 HD T2", descriptorsDict[270].ServiceName);
            Assert.AreEqual("CT 24 HD T2", descriptorsDict[272].ServiceName);
            Assert.AreEqual("CT sport HD T2", descriptorsDict[274].ServiceName);
            Assert.AreEqual("CT :D/art HD T2", descriptorsDict[276].ServiceName);
            Assert.AreEqual("CT 1 SM HD T2", descriptorsDict[280].ServiceName);
            Assert.AreEqual("CT 1 JM HD T2", descriptorsDict[282].ServiceName);
            Assert.AreEqual("CT 1 SVC HD T2", descriptorsDict[284].ServiceName);
            Assert.AreEqual("CT 1 JZC HD T2", descriptorsDict[286].ServiceName);

            Assert.AreEqual("CESKY ROZHLAS", descriptorsDict[16651].ProviderName);

            Assert.AreEqual(2, descriptorsDict[16651].ServisType);
            Assert.AreEqual(2, descriptorsDict[16652].ServisType);
            Assert.AreEqual(2, descriptorsDict[16653].ServisType);
            Assert.AreEqual(2, descriptorsDict[16654].ServisType);
            Assert.AreEqual(2, descriptorsDict[16655].ServisType);
            Assert.AreEqual(2, descriptorsDict[16656].ServisType);
            Assert.AreEqual(2, descriptorsDict[16657].ServisType);
            Assert.AreEqual(2, descriptorsDict[16658].ServisType);
            Assert.AreEqual(2, descriptorsDict[16659].ServisType);
            Assert.AreEqual(2, descriptorsDict[16660].ServisType);

            Assert.AreEqual("CRo RADIOZURNAL T2", descriptorsDict[16651].ServiceName);
            Assert.AreEqual("CRo DVOJKA T2", descriptorsDict[16652].ServiceName);
            Assert.AreEqual("CRo VLTAVA T2", descriptorsDict[16653].ServiceName);
            Assert.AreEqual("CRo RADIO WAVE T2", descriptorsDict[16654].ServiceName);
            Assert.AreEqual("CRo D-DUR T2", descriptorsDict[16655].ServiceName);
            Assert.AreEqual("CRo RADIO JUNIOR T2", descriptorsDict[16656].ServiceName);
            Assert.AreEqual("CRo PLUS T2", descriptorsDict[16657].ServiceName);
            Assert.AreEqual("CRo JAZZ T2", descriptorsDict[16658].ServiceName);
            Assert.AreEqual("CRo RZ SPORT T2", descriptorsDict[16659].ServiceName);
            Assert.AreEqual("CRo POHODA T2", descriptorsDict[16660].ServiceName);

        }

        [TestMethod]
        public void TestSDTCA1()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}SDT.CA1.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);
            var SDT = DVBTTable.CreateFromPackets<SDTTable>(packet, 17);

            Assert.IsNotNull(SDT);
            Assert.IsTrue(SDT.CRCIsValid());

            Assert.AreEqual(SDT.ServiceDescriptors.Count, 18);

            var descriptorsDict = new Dictionary<int, ServiceDescriptor>();
            foreach (var decriptor in SDT.ServiceDescriptors)
            {
                descriptorsDict.Add(decriptor.ProgramNumber, decriptor);
            }

            Assert.AreEqual("Towercom", descriptorsDict[4001].ProviderName);
            Assert.AreEqual("JOJ HD", descriptorsDict[4001].ServiceName);
            Assert.IsFalse(descriptorsDict[4001].Free);

            Assert.AreEqual("Towercom", descriptorsDict[4008].ProviderName);
            Assert.AreEqual("Prima Cool SK", descriptorsDict[4008].ServiceName);
            Assert.IsTrue(descriptorsDict[4008].Free);
        }

        [TestMethod]
        public void TestSDTCA2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}SDT.CA2.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);
            var SDT = DVBTTable.CreateFromPackets<SDTTable>(packet, 17);

            Assert.IsNotNull(SDT);
            Assert.IsTrue(SDT.CRCIsValid());

            Assert.AreEqual(SDT.ServiceDescriptors.Count, 16);

            var descriptorsDict = new Dictionary<int, ServiceDescriptor>();
            foreach (var decriptor in SDT.ServiceDescriptors)
            {
                descriptorsDict.Add(decriptor.ProgramNumber, decriptor);
            }

            Assert.AreEqual("ORF", descriptorsDict[14107].ProviderName);
            Assert.AreEqual("ORF1", descriptorsDict[14107].ServiceName);
            Assert.IsTrue(descriptorsDict[14107].Free);

            Assert.AreEqual("ORF", descriptorsDict[14101].ProviderName);
            Assert.AreEqual("ORF1 HD", descriptorsDict[14101].ServiceName);
            Assert.IsFalse(descriptorsDict[14101].Free);
        }

        [TestMethod]
        public void TestSDTCA3()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}SDT.CA3.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);
            var SDT = DVBTTable.CreateFromPackets<SDTTable>(packet, 17);

            Assert.IsNotNull(SDT);
            Assert.IsTrue(SDT.CRCIsValid());

            Assert.AreEqual(SDT.ServiceDescriptors.Count, 13);

            var descriptorsDict = new Dictionary<int, ServiceDescriptor>();
            foreach (var decriptor in SDT.ServiceDescriptors)
            {
                descriptorsDict.Add(decriptor.ProgramNumber, decriptor);
            }

            Assert.AreEqual("ORF", descriptorsDict[14801].ProviderName);
            Assert.AreEqual("ProSieben HD", descriptorsDict[14801].ServiceName);
            Assert.IsFalse(descriptorsDict[14801].Free);

            Assert.AreEqual("ORS", descriptorsDict[14806].ProviderName);
            Assert.AreEqual("4MEDIATHEK", descriptorsDict[14806].ServiceName);
            Assert.IsTrue(descriptorsDict[14806].Free);
        }

        [TestMethod]
        public void TestPMT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}PMT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var PMT = DVBTTable.CreateFromPackets<PMTTable>(packet, 2100);

            Assert.IsNotNull(PMT);
            Assert.IsTrue(PMT.CRCIsValid());

            Assert.AreEqual(2110, PMT.PCRPID);
            Assert.AreEqual(8, PMT.Streams.Count);

            var streamPIDsDict = new Dictionary<int, ElementaryStreamSpecificData>();
            foreach (var stream in PMT.Streams)
            {
                streamPIDsDict.Add(stream.PID, stream);
            }

            Assert.AreEqual(36, streamPIDsDict[2110].StreamType);
            Assert.AreEqual(17, streamPIDsDict[2120].StreamType);

            Assert.AreEqual(17, streamPIDsDict[2121].StreamType);
            Assert.AreEqual(6, streamPIDsDict[2122].StreamType);
            Assert.AreEqual(17, streamPIDsDict[2123].StreamType);

            Assert.AreEqual(6, streamPIDsDict[2130].StreamType);
            Assert.AreEqual(6, streamPIDsDict[2150].StreamType);

            Assert.AreEqual(5, streamPIDsDict[2160].StreamType);
        }
        /*
        [TestMethod]
        public void TestPMT2()
        {
            var fileNames = new List<String>() { "mux_475000000_8000000_beinsport.ts", "mux_491000000_8000000_7daysEpgSramble.ts" };
            var pmtpids = new List<long>() { 1001, 4286 };
            var index = 0;
            foreach (var f in fileNames) { 
                var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}{f}");

                var packet = MPEGTransportStreamPacket.Parse(packetBytes);

                var PMT = DVBTTable.CreateFromPackets<PMTTable>(packet, pmtpids[index++]);

                Assert.IsNotNull(PMT);
                Assert.IsTrue(PMT.CRCIsValid());

                foreach (var stream in PMT.Audio)
                {   
                    Console.WriteLine($"Audio {stream.PID}"); 
                }

                foreach (var stream in PMT.Video)
                {
                    Console.WriteLine($"Video {stream.PID}");
                }

                Console.WriteLine($"CasInfo Count {PMT.CasInfo.GetCount()}");
                if (PMT.CasInfo.GetCount() > 0) {
                    foreach (var stream in PMT.CasInfo.CasDesc) {
                        Console.WriteLine($"CasInfo CasID {stream.Key} PID {stream.Value}");
                    }
                }
            }
        }
        */
        [TestMethod]
        public void TestTDT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}TDT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var TDT = DVBTTable.CreateFromPackets<TDTTable>(packet, 20);

            Assert.IsNotNull(TDT);
            Assert.IsTrue(TDT.CRCIsValid());

            Assert.AreEqual(new DateTime(2023, 09, 03, 18, 31, 29), TDT.UTCTime);
        }

        /*
        [TestMethod]
        public void TestCAT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}mux_491000000_8000000_7daysEpgSramble.ts");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var CAT = DVBTTable.CreateFromPackets<CATTable>(packet, 1);

            Assert.IsNotNull(CAT);
            Assert.IsTrue(CAT.CRCIsValid());


            CAT.WriteToConsole();
        }
        */
    }

}
