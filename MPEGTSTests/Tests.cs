using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPEGTS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        [TestMethod]
        public void TestTDT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}TDT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var TDT = DVBTTable.CreateFromPackets<TDTTable>(packet, 20);

            Assert.IsNotNull(TDT);
            Assert.IsTrue(TDT.CRCIsValid());

            Assert.AreEqual(new DateTime(2023, 09, 03, 12, 31, 29), TDT.UTCTime);
        }

        [TestMethod]
        public void TestEIT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EIT);
            Assert.IsTrue(EIT.CRCIsValid());

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

        [TestMethod]
        public void TestEITCrete()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT.Crete.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EIT);

            Assert.AreEqual(1, EIT.EventItems.Count);

            var ev = EIT.EventItems[0];

            Assert.AreEqual("Κάνε ότι Κοιμάσαι (E)", ev.EventName);
            Assert.AreEqual("Ψυχαγωγία (Ελληνική σειρά)Ο Χριστόφορος χτυπάει βίαια τον τραπεζικό σύμβουλο  της αδελφής του, κατηγορώντας τον για εμπλοκή στην  υπόθεσή της. Τίνος την εκτέλεση ζητάει ο Βανδώρος;     Παίζουν οι ηθοποιοί: Σπύρος Παπαδόπουλος (Νικόλας Καλίρης), Έμιλυ Κολιανδρή (Βικτόρια Λεσιώτη), Φωτεινή Μπαξεβάνη (Μπετίνα Βορίδη), Μαρίνα Ασλάνογλου (Ευαγγελία Καλίρη), Νικολέτα Κοτσαηλίδου (Άννα Γραμμικού), Δημήτρης Καπετανάκος (Κωνσταντίνος Ίσσαρης), Βασίλης Ευταξόπουλος (Στέλιος Κασδαγλής), Τάσος Γιαννόπουλος (Ηλίας Βανδώρος), Γιάννης Σίντος (Χριστόφορος Στρατάκης), Γεωργία Μεσαρίτη (Νάσια Καλίρη), Αναστασία Στυλιανίδη (Σοφία Μαδούρου), Βασίλης Ντάρμας (Μάκης Βελής), Μαρία Μαυρομμάτη (Ανθή Βελή), Αλέξανδρος Piechowiak (Στάθης Βανδώρος), Χρήστος Διαμαντούδης (Χρήστος Γιδάς), Βίκυ Μαϊδάνογλου (Ζωή Βορίδη), Χρήστος Ζαχαριάδης (Στράτος Φρύσας), Δημήτρης Γεροδήμος (Μιχάλης Κουλεντής), Λευτέρης Πολυχρόνης (Μηνάς Αργύρης), Ζωή Ρηγοπούλου (Λένα Μιχελή), Δημήτρης Καλαντζής (Παύλος, δικηγόρος Βανδώρου), Έλενα Ντέντα (Κατερίνα Σταφυλίδου), Καλλιόπη Πετροπούλου (Στέλλα Κρητικού), Αλέξανδρος Βάρθης (Λευτέρης), Λευτέρης Ζαμπετάκης (Μάνος Αναστασίου), Γιώργος Δεπάστας (Λάκης Βορίδης), Ανανίας Μητσιόπουλος (Γιάννης Φυτράκης), Χρήστος Στεφανής (Θοδωρής Μπίτσιος), Έφη Λιάλιου (Αμάρα), Στέργιος Αντουλάς (Πέτρος).  Guests: Ναταλία Τσαλίκη (ανακρίτρια Βάλβη), Ντόρα Μακρυγιάννη (Πολέμη, δικηγόρος Μηνά), Αλέξανδρος Καλπακίδης (Βαγγέλης Λημνιός, προϊστάμενος της Υπηρεσίας Πληροφοριών), Βασίλης Ρίσβας (Ματθαίος), Θανάσης Δισλής (Γιώργος, καθηγητής), Στέργιος Νένες (Σταμάτης) και Ντάνιελ Νούρκα (Τάκης, μοντέλο).  Σενάριο: Γιάννης Σκαραγκάς  Σκηνοθεσία: Αλέξανδρος Πανταζούδης, Αλέκος Κυράνης                           Διεύθυνση φωτογραφίας: Κλαούντιο Μπολιβάρ, Έλτον Μπίφσα  Σκηνογραφία: Αναστασία Χαρμούση  Κοστούμια: Ελίνα Μαντζάκου  Πρωτότυπη μουσική: Γιάννης Χριστοδουλόπουλος  Μοντάζ : Νίκος Στεφάνου  Οργάνωση Παραγωγής: Ηλίας Βογιατζόγλου, Αλέξανδρος Δρακάτος  Παραγωγή: Silverline Media Productions Α.Ε.", ev.Text);
            Assert.AreEqual(new DateTime(2023, 07, 27, 22, 00, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2023, 07, 27, 23, 00, 0), ev.FinishTime);
        }

        [TestMethod]
        public void TestEITCrete2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT.Crete2.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EIT);
            Assert.IsTrue(EIT.CRCIsValid());

            Assert.AreEqual(6, EIT.EventItems.Count);

            var eventsDict = new Dictionary<int, EventItem>();
            foreach (var ev in EIT.EventItems)
            {
                eventsDict.Add(ev.EventId, ev);
            }

            Assert.AreEqual("Γεύσεις από Ελλάδα (E)", eventsDict[464].EventName);
            Assert.AreEqual("Ψυχαγωγία (Γαστρονομία) «Αβγοτάραχο»Ενημερωνόμαστε για την ιστορία του ελληνικού  αβγοτάραχου.  Ο σεφ Γιάννης Λιάκου μαγειρεύει μαζί με την  Ολυμπιάδα Μαρία Ολυμπίτη λιγκουίνι με αβγοτάραχο  και ταρτάρ μανιταριών με αβγοτάραχο.   Παρουσίαση: Ολυμπιάδα Μαρία Ολυμπίτη  Eπιμέλεια: Ολυμπιάδα Μαρία Ολυμπίτη  Αρχισυνταξία: Μαρία Πολυχρόνη  Σκηνογραφία: Ιωάννης Αθανασιάδης  Διεύθυνση φωτογραφίας: Ανδρέας Ζαχαράτος  Διεύθυνση παραγωγής: Άσπα Κουνδουροπούλου - Δημήτρης Αποστολίδης  Σκηνοθεσία: Χρήστος Φασόης", eventsDict[464].Text);
            Assert.AreEqual(new DateTime(2023, 07, 28, 11, 00, 0), eventsDict[464].StartTime);
            Assert.AreEqual(new DateTime(2023, 07, 28, 11, 50, 0), eventsDict[464].FinishTime);

            Assert.AreEqual("Ένα Μήλο την Ημέρα (E)", eventsDict[465].EventName);
            Assert.AreEqual("Ντοκιμαντέρ (Διατροφή)Έτος παραγωγής: 2014 Τρεις συγκάτοικοι με εντελώς διαφορετικές απόψεις περί διατροφής - ο Θοδωρής Αντωνιάδης, η Αγγελίνα Παρασκευαΐδη και η Ιωάννα Πιατά- και ο Μιχάλης Μητρούσης στο ρόλο του από μηχανής... διατροφολόγου, μας μιλούν για τη διατροφή μας Σενάριο: Κωστής Ζαφειράκης Έρευνα- δημοσιογραφική επιμέλεια: Στέλλα Παναγιωτοπούλου Διεύθυνση φωτογραφίας: Νίκος Κανέλλος Μουσική τίτλων: Κώστας Γανωτής Ερμηνεύει η Ελένη Τζαβάρα Μοντάζ: Λάμπης Χαραλαμπίδης Ηχοληψία: Ορέστης Καμπερίδης Σκηνικά: Θοδωρής Λουκέρης Ενδυματόλογος: Στέφανι Λανιέρ Μακιγιάζ-κομμώσεις: Έλια Κανάκη Οργάνωση παραγωγής: Βάσω Πατρούμπα Διεύθυνση παραγωγής: Αναστασία Καραδήμου Εκτέλεση παραγωγής: ΜΙΤΟΣ Σκηνοθεσία: Νίκος Κρητικός", eventsDict[465].Text);
            Assert.AreEqual(new DateTime(2023, 07, 28, 11, 50, 0), eventsDict[465].StartTime);
            Assert.AreEqual(new DateTime(2023, 07, 28, 12, 00, 0), eventsDict[465].FinishTime);

            Assert.AreEqual("Η Καλύτερη Τούρτα Κερδίζει (E)", eventsDict[466].EventName);
            Assert.AreEqual("Παιδικά-Νεανικά «Ο υδάτινος κόσμος της Κλόι»[Best Cake Wins]  Έτος παραγωγής: 2019  Κάθε παιδί αξίζει την τέλεια τούρτα. Και σε αυτή την εκπομπή μπορεί να την έχει.  Αρκεί να ξεδιπλώσει τη φαντασία του χωρίς όρια, να αποτυπώσει την τούρτα στο  χαρτί και να να δώσει το σχέδιό του σε δύο κορυφαίους ζαχαροπλάστες.", eventsDict[466].Text);
            Assert.AreEqual(new DateTime(2023, 07, 28, 12, 00, 0), eventsDict[466].StartTime);
            Assert.AreEqual(new DateTime(2023, 07, 28, 12, 30, 0), eventsDict[466].FinishTime);

            Assert.AreEqual("KooKooLand (E)", eventsDict[467].EventName);
            Assert.AreEqual("Παιδικά-Νεανικά «Άκου τον ωκεανό»Έτος παραγωγής: 2022 Στην Kookooland δεν υπάρχει Ωκεανός! Ούτε ένας! Το Ρομπότ και τι δεν θα έδινε να δει από κοντά τον απέραντο μπλε ωκεανό! Ακούγονται οι φωνές των: Αντώνης Κρόμπας - Λάμπρος ο Δεινόσαυρος + voice director Λευτέρης Ελευθερίου - Koobot το Ρομπότ Κατερίνα Τσεβά - Χρύσα η Μύγα Μαρία Σαμάρκου - Γάτα η Σουριγάτα Τατιάννα Καλατζή - Βάγια η Κουκουβάγια Σκηνοθεσία: Ivan Salfa Υπεύθυνη μυθοπλασίας - σενάριο: Θεοδώρα Κατσιφή Σεναριακή Ομάδα: Λίλια Γκούνη-Σοφικίτη, Όλγα Μανάρα Επιστημονική Συνεργάτης, Αναπτυξιακή Ψυχολόγος: Σουζάνα Παπαφάγου Μουσική και ενορχήστρωση: Σταμάτης Σταματάκης Στίχοι: Άρης Δαβαράκης Χορογράφος: Ευδοκία Βεροπούλου Χορευτές: Δανάη Γρίβα Άννα Δασκάλου Ράνια Κολιού Χριστίνα Μάρκου Κατερίνα Μήτσιου Νατάσσα Νίκου Δημήτρης Παπακυριαζής Σπύρος Παυλίδης Ανδρόνικος Πολυδώρου Βασιλική Ρήγα Ενδυματολόγος: Άννα Νομικού Κατασκευή 3D Unreal engine: WASP STUDIO Creative Director: Γιώργος Ζάμπας, ROOFTOP IKE Τίτλοι - Γραφικά: Δημήτρης Μπέλλος, Νίκος Ούτσικας Τίτλοι-Art Direction Supervisor: Άγγελος Ρούβας Line Producer: Ευάγγελος Κυριακίδης Βοηθός Οργάνωσης Παραγωγής: Νίκος Θεοτοκάς Εταιρεία Παραγωγής: Feelgood Productions Executive Producers: Φρόσω Ράλλη - Γιάννης Εξηντάρης Παραγωγός: Ειρήνη Σουγανίδου", eventsDict[467].Text);
            Assert.AreEqual(new DateTime(2023, 07, 28, 12, 30, 0), eventsDict[467].StartTime);
            Assert.AreEqual(new DateTime(2023, 07, 28, 13, 00, 0), eventsDict[467].FinishTime);

            Assert.AreEqual("Γεια σου, Ντάγκι - Γ Κύκλος (E)", eventsDict[468].EventName);
            Assert.AreEqual("Παιδικά-Νεανικά (Κινούμενα σχέδια) Επεισόδια 17ο: «Το σήμα της μοιρασιάς» & 18ο: «Το σήμα της Ιστορίας» & 19ο: «Το σήμα της Τέχνης» & 20ο: Το σήμα..[Hey, Duggee]  Έτος παραγωγής: 2016", eventsDict[468].Text);
            Assert.AreEqual(new DateTime(2023, 07, 28, 13, 00, 0), eventsDict[468].StartTime);
            Assert.AreEqual(new DateTime(2023, 07, 28, 13, 30, 0), eventsDict[468].FinishTime);

            Assert.AreEqual("Με Οικολογική Ματιά (E)", eventsDict[469].EventName);
            Assert.AreEqual("Ντοκιμαντέρ (Οικολογία) «Τα παιδιά της επανάστασης»[Eco-Eye: Sustainable Solutions]  Έτος παραγωγής: 2016  Η νέα παρουσιάστρια της σειράς Κλερ Καμπαμέτου  ερευνά τα αίτια πίσω από τις απεργίες για την  κλιματική αλλαγή που πραγματοποιεί η νέα γενιά, η  οποία φέρει τη μικρότερη ευθύνη. Συναντά σχολικούς  απεργούς για να μάθει πώς νιώθουν.", eventsDict[469].Text);
            Assert.AreEqual(new DateTime(2023, 07, 28, 13, 30, 0), eventsDict[469].StartTime);
            Assert.AreEqual(new DateTime(2023, 07, 28, 14, 00, 0), eventsDict[469].FinishTime);
        }

        [TestMethod]
        public void TestEITIt()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT.IT.bin");

            var packets = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packets, 18);

            Assert.IsNotNull(EIT);
            Assert.IsTrue(EIT.CRCIsValid());

            Assert.AreEqual(1, EIT.EventItems.Count);

            var ev = EIT.EventItems[0];

            Assert.AreEqual("Distretto di polizia", ev.EventName);
            Assert.AreEqual("S7 Ep7 Genitori sbagliati/angelo della morteRosanna e' violentata in casa propria da un uomo mascherato che sembra non aver lasciato tracce: la giovane donna, sconvolta, ricorda solo che l'aggressore   aveva una lieve zoppia...Anna (Giulia Bevilacqua) e Irene (Francesca Inaudi) indagano sul caso, che presenta analogie con altri stupri rimasti irrisolti.\r\n    L'agente Guerra (Daniela Morozzi) ed Ingargiola (Gianni Ferreri) hanno invece il compito di rintracciare un detenuto evaso, strana coincidenza,   il giorno stesso che viene denunciato il furto di un prezioso pianoforte...   \r\n  Anna (Giulia Bevilacqua) e Irene (Francesca Inaudi)  sono  in un locale dove una donna ubriaca, Gabriella, sembra aver bisogno del loro aiuto. Le due agenti la accompagnano a casa e li' scoprono il corpo  senza  vita del quattordicenne Enrico, il figlio autistico della donna, immediatamente arrestata.\r\n  Ingargiola (Gianni Ferreri) e Vittoria (Daniela  Morozzi)  sono invece  sulle tracce di un celebre ipnotizzatore che spinge le sue vittime a consegn...\r\nVISIONE CONSIGLIATA CON LA PRESENZA DI UN ADULTO.", ev.Text);
            Assert.AreEqual(new DateTime(2023, 08, 16, 17, 13, 54), ev.StartTime);
            Assert.AreEqual(new DateTime(2023, 08, 16, 19, 14, 31), ev.FinishTime);
        }

        [TestMethod]
        public void TestEITIt2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT.IT2.bin");

            var packets = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packets, 18);

            Assert.IsNotNull(EIT);
            Assert.IsTrue(EIT.CRCIsValid());

            Assert.AreEqual(4, EIT.EventItems.Count);

            var eventsDict = new Dictionary<int, EventItem>();
            foreach (var ev in EIT.EventItems)
            {
                eventsDict.Add(ev.EventId, ev);
            }

            Assert.AreEqual("Un altro domani - PrimaTv", eventsDict[31936].EventName);
            Assert.AreEqual("S1 Ep252Julia e Leo vanno avanti con i preparativi del matrimonio, contro tutto e tutti. E se Diana era contraria, i genitori di Leo non sembrano essere da meno.\r\nVISIONE CONSIGLIATA CON LA PRESENZA DI UN ADULTO.\r\nQUESTO PROGRAMMA E' SOTTOTITOLATO.", eventsDict[31936].Text);
            Assert.AreEqual(new DateTime(2023, 08, 16, 17, 40, 5), eventsDict[31936].StartTime);
            Assert.AreEqual(new DateTime(2023, 08, 16, 18, 46, 41), eventsDict[31936].FinishTime);

            Assert.AreEqual("The wall estate", eventsDict[31937].EventName);
            Assert.AreEqual("The wall '23  estate '23", eventsDict[31937].Text);
            Assert.AreEqual(new DateTime(2023, 08, 16, 18, 46, 41), eventsDict[31937].StartTime);
            Assert.AreEqual(new DateTime(2023, 08, 16, 19, 35, 25), eventsDict[31937].FinishTime);

            Assert.AreEqual("Tg5 anticipazione,", eventsDict[31938].EventName);
            Assert.AreEqual("", eventsDict[31938].Text);
            Assert.AreEqual(new DateTime(2023, 08, 16, 19, 35, 25), eventsDict[31938].StartTime);
            Assert.AreEqual(new DateTime(2023, 08, 16, 19, 54, 01), eventsDict[31938].FinishTime);

            Assert.AreEqual("Tg5 Prima Pagina", eventsDict[31939].EventName);
            Assert.AreEqual("TELEGIORNALE 4 - Italia, 2023Sintesi delle notizie principali del giorno.", eventsDict[31939].Text);
            Assert.AreEqual(new DateTime(2023, 08, 16, 19, 54, 01), eventsDict[31939].StartTime);
            Assert.AreEqual(new DateTime(2023, 08, 16, 20, 00, 51), eventsDict[31939].FinishTime);
        }

        [TestMethod]
        public void TestEITUA()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT.UA.bin");

            var packets = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packets, 18);

            Assert.IsNotNull(EIT);
            Assert.IsTrue(EIT.CRCIsValid());
        }
    }
}
