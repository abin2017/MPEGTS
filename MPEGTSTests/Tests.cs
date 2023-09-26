using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPEGTS;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestPSI()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}PSI.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);
            var PSITable = DVBTTable.CreateFromPackets<PSITable>(packet, 0);

            Assert.IsNotNull(PSITable);

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
        public void TestPMT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}PMT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var PMT = DVBTTable.CreateFromPackets<PMTTable>(packet, 2100);

            Assert.IsNotNull(PMT);

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

        [TestMethod]
        public void TestTDT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}TDT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var TDT = DVBTTable.CreateFromPackets<TDTTable>(packet, 20);

            Assert.IsNotNull(TDT);

            Assert.AreEqual(new DateTime(2023, 09, 03, 12, 31, 29), TDT.UTCTime);
        }

        [TestMethod]
        public void TestEIT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EIT);

            Assert.AreEqual(4, EIT.EventItems.Count);

            var eventsDict = new Dictionary<int, EventItem>();
            foreach (var ev in EIT.EventItems)
            {
                eventsDict.Add(ev.EventId, ev);
            }

            Assert.AreEqual("Krimi zprávy", eventsDict[175].EventName);
            Assert.AreEqual("Aktuální události z oblasti kriminality. Zločiny očima profesionálů, zkušených reportérů i diváků. (Premiéra)", eventsDict[175].Text);
            Assert.AreEqual(new DateTime(2023, 09, 07, 20, 40, 0), eventsDict[175].StartTime);
            Assert.AreEqual(new DateTime(2023, 09, 07, 20, 55, 0), eventsDict[175].FinishTime);

            Assert.AreEqual("SHOWTIME", eventsDict[176].EventName);
            Assert.AreEqual("", eventsDict[176].Text);
            Assert.AreEqual(new DateTime(2023, 09, 07, 20, 55, 0), eventsDict[176].StartTime);
            Assert.AreEqual(new DateTime(2023, 09, 07, 21, 15, 0), eventsDict[176].FinishTime);

            Assert.AreEqual("ZOO (130)", eventsDict[177].EventName);
            Assert.AreEqual("Epizoda: Samá překvapení Sid řeší situaci s novou žirafou. Viky a Filip čelí důsledkům svých nečekaných závazků. Raul potřebuje pomoc s vedením firmy. Kristýna vezme Alberta na milost. Charlie se spojuje s Břéťou proti Ovčíkovi. Hrají M. Pecháčková, E. Burešová, T. Klus, B. Suchánková, D. Gránský, M. Němec, B. Černá, L. Černá, J. Švandová, V. Kratina a další. Režie L. Kodad. (Premiéra)", eventsDict[177].Text);
            Assert.AreEqual(new DateTime(2023, 09, 07, 21, 15, 0), eventsDict[177].StartTime);
            Assert.AreEqual(new DateTime(2023, 09, 07, 22, 35, 0), eventsDict[177].FinishTime);

            Assert.AreEqual("Inkognito", eventsDict[178].EventName);
            Assert.AreEqual("V zábavné show Inkognito se čtveřice osobností snaží uhodnout profesi jednotlivých hostů nebo jejich identitu. Hádejte společně s nimi a pobavte se nečekanými myšlenkami. Moderuje Libor Bouček. (Premiéra)", eventsDict[178].Text);
            Assert.AreEqual(new DateTime(2023, 09, 07, 22, 35, 0), eventsDict[178].StartTime);
            Assert.AreEqual(new DateTime(2023, 09, 07, 23, 45, 0), eventsDict[178].FinishTime);
        }
    }
}
