using LoggerService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPEGTS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Text;


namespace Tests
{
    [TestClass]
    public class EITTests
    {
        [TestMethod]
        public void TestEIT()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}EIT.bin");

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

        /// <summary>
        /// EIT with non standard encoding 0x10
        /// </summary>
        [TestMethod]
        public void TestEIT2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}EIT2.bin");

            var packets = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packets, 18);

            Assert.IsNotNull(EIT);
            Assert.IsTrue(EIT.CRCIsValid());
        }

        /// <summary>
        /// Test EITScan method
        /// </summary>
        [TestMethod]
        public void TestEITScan()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}EITScan.bin");

            var packets = MPEGTransportStreamPacket.Parse(packetBytes);

            var eitService = new EITService(new DummyLoggingService());
            var res = eitService.Scan(packets);

            Assert.IsNotNull(res);

            Assert.IsNotNull(res.CurrentEvents);
            Assert.IsNotNull(res.ScheduledEvents);
            Assert.AreEqual(0, res.CurrentEvents.Count);
            Assert.AreEqual(8, res.ScheduledEvents.Count);
        }

        [TestMethod]
        public void TestEITCrete()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Crete{Path.DirectorySeparatorChar}EIT.Crete.bin");

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

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Crete{Path.DirectorySeparatorChar}EIT.Crete2.bin");

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
        public void TestEITGR()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var eitBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Germany{Path.DirectorySeparatorChar}EIT.GR.bin");

            var EIT = new EITTable();
            EIT.Parse(new List<byte>(eitBytes));

            Assert.IsNotNull(EIT);
            Assert.IsTrue(EIT.CRCIsValid());

            Assert.AreEqual(1, EIT.EventItems.Count);

            Assert.AreEqual("Die Diebin & der General", EIT.EventItems[0].EventName);
            Assert.AreEqual("Fernsehfilm Deutschland 2005Es ist schon eine ungewöhnliche Freundschaft zwischen Walter Voss, genannt \"Der General\", und seiner Pflegerin Jessie. Die alleinerziehende Mutter wurde wegen Ladendiebstahls und Sachbeschädigung zu gemeinnütziger Arbeit in einem Altenpflegeheim verurteilt. Doch als die kostbare Taschenuhr des Generals plötzlich verschwindet, gerät Jessie in Verdacht: einmal Diebin, immer Diebin.\u008a\"Die Diebin & der General\" ist eine lebensnahe, gesellschaftskritische Komödie mit Humor und Gefühl. Katja Riemann in der Rolle der chaotischen Mutter und Jürgen Hentsch als liebenswürdiger Querkopf zeigen eine bewegende Darstellung.", EIT.EventItems[0].Text);
            Assert.AreEqual(new DateTime(2024, 03, 12, 14, 30, 0), EIT.EventItems[0].StartTime);
            Assert.AreEqual(new DateTime(2024, 03, 12, 16, 00, 0), EIT.EventItems[0].FinishTime);
        }

        [TestMethod]
        public void TestEITIt()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Italy{Path.DirectorySeparatorChar}EIT.IT.bin");

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

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Italy{Path.DirectorySeparatorChar}EIT.IT2.bin");

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

        /// <summary>
        /// Polish EIT with UTF8 encoding
        /// </summary>
        [TestMethod]
        public void TestEITPL()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Poland{Path.DirectorySeparatorChar}EIT.PL.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EIT);

            Assert.AreEqual(5, EIT.EventItems.Count);

            var ev = EIT.EventItems[0];

            Assert.AreEqual("Zakup kontrolowany: odc.355", ev.EventName);
            Assert.AreEqual(" magazyn motoryzacyjny (Polska, 2018) odc.355 Wojtek niedawno sprzedał Volvo V40 i teraz ma 25 tysięcy złotych na mocniejszy i nowszy samochód. Adam Kornacki przedstawi mu kilka modeli kombi, sedana i hatchbacka. Od lat: 12", ev.Text);
            Assert.AreEqual(new DateTime(2023, 10, 15, 05, 25, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2023, 10, 15, 06, 20, 0), ev.FinishTime);
        }

        /// <summary>
        /// Polish EIT with ISO-8859-13 encoding
        /// </summary>
        [TestMethod]
        public void TestEITPL2()
        {
            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Poland{Path.DirectorySeparatorChar}EIT.PL2.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EIT = DVBTTable.CreateFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EIT);

            Assert.AreEqual(1, EIT.EventItems.Count);

            var ev = EIT.EventItems[0];

            Assert.AreEqual("Francuskie śniadanie - Thierry Śmiałek sezon 1 odc. 8", ev.EventName);
            Assert.AreEqual("", ev.Text);
            Assert.AreEqual(new DateTime(2024, 3, 31, 11, 55, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2024, 3, 31, 12, 20, 0), ev.FinishTime);
        }

        /// <summary>
        /// Magyar EIT with dynamically selected part of ISO/IEC 8859
        /// </summary>
        [TestMethod]
        public void TestEITHungary()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Hungary{Path.DirectorySeparatorChar}EIT.Hungary.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EITs = DVBTTable.CreateAllFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EITs);

            Assert.AreEqual(15, EITs.Count);

            Assert.AreEqual(1, EITs[11].EventItems.Count);

            var ev = EITs[11].EventItems[0];

            Assert.AreEqual(new DateTime(2024, 4, 19, 9, 00, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2024, 4, 19, 9, 30, 0), ev.FinishTime);

            Assert.AreEqual("Élő népzene", ev.EventName);
            Assert.AreEqual("(magyar zenés műsor, 2007) - Üsztürü zenekar - Népzene és néptánc a Duna Televízió műsorán. rendező:  Sztanó Hédi, Nagy Anikó Mária", ev.Text);
        }

        /// <summary>
        /// Austria EIT
        /// </summary>
        [TestMethod]
        public void TestEITAustria()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Austria{Path.DirectorySeparatorChar}EIT.AU.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EITs = DVBTTable.CreateAllFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EITs);

            Assert.AreEqual(24, EITs.Count);

            Assert.AreEqual(1, EITs[3].EventItems.Count);

            var ev = EITs[3].EventItems[0];

            Assert.AreEqual(new DateTime(2024, 7, 29, 13, 00, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2024, 7, 29, 14, 00, 0), ev.FinishTime);

            Assert.AreEqual("FM4 Hot", ev.EventName);
            Assert.AreEqual("Musik für die heißeste Zeit des Jahres.", ev.Text);

            Assert.AreEqual("Musik zum Träumen", EITs[5].EventItems[0].TextValue);
            Assert.AreEqual("Unser Österreich (Seenland Österreich - Neusiedler See und SeewinkelDer Neusiedler See ist einer der wenigen Steppenseen Europas, der größte See Österreichs und in mehrfacher Hinsicht ungewöhnlich: er ist im Schnitt nur einen Meter tief, wird vor allem aus Niederschlägen gespeist und hat nur einen einzigen, künstlich angelegten Abfluss.\u008aDer liebevoll als \"Meer der Wiener\" bezeichnete See, erstreckt sich von den Hängen des Leithagebirges, den Klippen eines urzeitlichen Meers, bis in die Weite der Puszta. Die Region beheimatet eine einzigartige Flora und Fauna im und rund um den Steppensee und bietet ein einfaches, aber genussreiches Leben.\u008aRosa Maria Plattner hat sich auf eine filmische Erkundungstour begeben und unkonventionelle Menschen, die hier ihr Leben bestreiten, besucht. So auch einen Biobauern und Paradeiser-Pionier, der vor der Herausforderung steht \"Wie lassen sich Tomaten züchten, ohne zu gießen?\", einen Duftbauern, der den ehemaligen elterlichen Schweinestall in eine Duftoase umgewandelt hat, oder auch Josef Haubenwallner, der ein privates Museumsdorf aus traditionellen Häusern des Heidebodens aufgebaut hat, und viele der Bauwerke so vor dem Verfall gerettet hat.)", EITs[19].EventItems[0].TextValue);

        }

        /// <summary>
        /// Austria EIT test 2
        /// </summary>
        [TestMethod]
        public void TestEITAustria2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Austria{Path.DirectorySeparatorChar}EIT.AU.2.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EITs = DVBTTable.CreateAllFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EITs);

            Assert.AreEqual(23, EITs.Count);

            Assert.AreEqual(0, EITs[3].EventItems.Count);

            Assert.AreEqual(3, EITs[7].EventItems.Count);

            var ev = EITs[7].EventItems[2];

            Assert.AreEqual(new DateTime(2024, 7, 29, 16, 00, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2024, 7, 29, 17, 00, 0), ev.FinishTime);

            Assert.AreEqual("Das Duell - Zwischen Tüll und Tränen (Manuela Kriewen vs. Jana SchmitterZwei Expert:innen können den Traum einer Braut vom perfekten Hochzeitskleid erfüllen. Entscheidet sich die Braut für eines der Kleider, kürt sie damit auch den Gewinner:in des Duells. Heute im Duell: Manuela Kriewen vs. Jana Schmitter)", EITs[7].EventItems[2].TextValue);
        }

        [TestMethod]
        public void TestEITSlovakia1()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Slovakia{Path.DirectorySeparatorChar}EIT.SK.1.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EITs = DVBTTable.CreateAllFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EITs);

            Assert.AreEqual(16, EITs.Count);

            Assert.AreEqual(12, EITs[0].EventItems.Count);

            var ev = EITs[0].EventItems[0];

            Assert.AreEqual(new DateTime(2024, 7, 27, 14, 15, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2024, 7, 27, 14, 25, 0), ev.FinishTime);

            Assert.AreEqual("Fíha tralala (10)", ev.EventName);

            Assert.AreEqual("Naše prasiatko (43)", EITs[0].EventItems[1].TextValue);
            Assert.AreEqual("MalinyJamológia 2 (1) (Lucka Siposová a medveď)", EITs[0].EventItems[4].TextValue);
        }

        [TestMethod]
        public void TestEITSlovakia2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Slovakia{Path.DirectorySeparatorChar}EIT.SK.2.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EITs = DVBTTable.CreateAllFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EITs);

            Assert.AreEqual(13, EITs.Count);

            Assert.AreEqual(13, EITs[2].EventItems.Count);

            var ev = EITs[2].EventItems[10];

            Assert.AreEqual(new DateTime(2024, 7, 28, 13, 20, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2024, 7, 28, 13, 35, 0), ev.FinishTime);

            Assert.AreEqual("Hanička a Murko pre najmenších (5)", ev.EventName);
            Assert.AreEqual("Hanička a Murko pre najmenších (5) (Hasiči)", ev.TextValue);
        }


        [TestMethod]
        public void TestEITSlovakia3()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var packetBytes = File.ReadAllBytes($"TestData{Path.DirectorySeparatorChar}EIT{Path.DirectorySeparatorChar}Slovakia{Path.DirectorySeparatorChar}EIT.SK.3.bin");

            var packet = MPEGTransportStreamPacket.Parse(packetBytes);

            var EITs = DVBTTable.CreateAllFromPackets<EITTable>(packet, 18);

            Assert.IsNotNull(EITs);

            Assert.AreEqual(26, EITs.Count);

            Assert.AreEqual(2, EITs[5].EventItems.Count);

            var ev = EITs[5].EventItems[0];

            Assert.AreEqual(new DateTime(2024, 7, 21, 05, 00, 0), ev.StartTime);
            Assert.AreEqual(new DateTime(2024, 7, 21, 06, 00, 0), ev.FinishTime);

            Assert.AreEqual("Najlepšie ľudovky a dychovky", ev.EventName);
            Assert.AreEqual("Deväťdesiatky (Najúspešnejsí a najsledovanejší kriminálny seriál Českej televízie, ktorý dejovo o dvadsať rokov predchádza Prípady 1. oddelenia.Je všeobecne známe, že situácia po nežnej revolúcii priniesla v deväťdesiatych rokoch strmý nárast kriminality. Kriminalita v tej dobe  bola nesmierne násilná a krutá. Polícia na tieto podmienky nebola vôbec pripravená ani vybavená a jej úloha bola nesmierne náročná. Seriál je dramatizáciou skutočných kriminálnych prípadov, ktoré vyšetrovalo 1. oddelenie pražskej kriminálnej polície a ktoré sa stali po rozdelení Československa, v rozmedzí rokov 1993 až 1995. Osobné linky protagonistov sa nezakladajú na realite, ale skutková podstata činov a spôsob ich odhalenia ostávajú autentické. Na tvorbe scenára sa podieľali tvorcovia Josef Mareš a Matěj Podzimek. Josef Mareš je bývalý vedúci 1. oddelenia a na niektorých prípadoch osobne pracoval. Na 1. odd nastupuje mladý, ešte neskúsený detektív Tomáš Kozák. Už ostrieľaný ?operatívec Václav Plíšek, nie je z nováčika nadšený, avšak na riešenie takýchto  malicherností nebude čas, nakoľko obaja budú spoločne čeliť bezprecedentnému návalu vrážd. Česká republika 2022)", EITs[7].EventItems[1].TextValue);
        }
    }
}
