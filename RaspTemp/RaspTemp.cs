using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;

/// @author Arto Hopia
/// @version 26.3.2018
/// 
/// <summary>
/// Ohjelma lukee Raspberry Pi:n 1-wire-anturin tietoa ja näyttää lämpötilan minimi-
/// maksimi- ja keskiarvon sekä lähettää hälytyksen sähköpostiin kun käyttäjän
/// antama maksimilämpätila on saavutettu.
/// </summary>
public class RaspTemp
{
    // TODO: Olen testannut aliohjelmia, kts Demo 6, teht 2a ja teht 5 sekä Demo 7, teht 1 ja Demo 8, teht 4a ja Demo 9, teht 1 ja teht 3
    
    /// <summary>
    /// Pääohjelma Raspberry Pi:n 1-Wire-anturin lukemiseksi
    /// </summary>
    public static void Main()
    {
        /// <summary>
        /// Tunnistetaan käyttöjärjestelmä, koska anturin lämpätilatiedon sisältämä 
        /// tiedosto on eri polussa Raspberryllä ja Windowsilla.
        /// Käyttöjärjestelmästä riippuen otetaan oikea polku käyttöön automaattisesti.
        /// </summary>
        /// <param name="anturipolku">1-Wire-anturin polku</param>
        int p = (int)Environment.OSVersion.Platform;
        String anturipolku;
        if ((p == 4) || (p == 6))
        {
            // Käyttöjäjestelmä on Linux. p=4 Raspberryssä/Linuxissa
            Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "1");
            // Hakupolku Raspberryyn
            anturipolku = "/sys/devices/w1_bus_master1/";
        }
        else
        {
            // Käyttöjäjestelmä on Windows. p=2 Windowsissa 
            anturipolku = "C:/Ohjelmointi/harjoitustyo/RaspTemp/RaspTemp";
        }

        // Aluksi pyydetään käyttäjältä hälytysraja
        double halyRaja = annaHalyRaja();
        Console.WriteLine("\npaina jotain jatkaaksesi.");
        Console.ReadLine();

        // Tyhjennä näyttö, tallenna alkukoordinaatit, ylin ja vasemmanpuoleisin
        // Aseta nollarivi ja -sarake sopivaan kohtaan
        // Muuttamalla numeroarvoja Console.CursorTop ja Console.CursorLeft perässä,
        // voidaan säätää koko näkymän sijaintia konsolinäytöllä
        Console.Clear();
        nollaRivi = Console.CursorTop + 2;
        nollaSarake = Console.CursorLeft + 5;

        // Sähköpostin lähetykseen liittyvät alustukset
        // Hälytys- ja poistumisviestien otsikot ja viestit
        string halytysOtsikko = "HÄLYTYS, lämpötila yli " + halyRaja + " astetta";
        string halytysViesti = "HÄLYTYS, lämpötila on nyt yli " + halyRaja + " astetta.";

        string halytysPoistuiOtsikko = "POISTUI, HÄLYTYS, lämpötila yli " + halyRaja + " astetta";
        string halytysPoistuiViesti = "POISTUI, HÄLYTYS, lämpötila on nyt yli " + halyRaja + " astetta.";

        // Käytetään idikoimaan onko viesti lähetetty, jotta lähetetään 
        // poistumisviesti vain, jos hälytys on lähetetty
        bool halyLähetetty = false;

        // Alustetaan listaanlisäys polkutiedolla
        double listaanLisays = HaeLampo(anturipolku);
        List<double> lampotilat = new List<double>();               // Luodaan lista lämpötiloille

        // Alustetaan lämpötilojen aikaleimat
        string minPvm;
        string minKlo;
        string maxPvm;
        string maxKlo;
        string aikaNyt;

        // Alustetaan min- ja max lämpötilat äärilaitaan
        double maxArvoVanha = Double.MinValue;
        double minArvoVanha = Double.MaxValue;

        /// <summary>
        /// Tehtävänä hoitaa kaikki ajastukseen liittyvät asiat, ajastus asetettu 1000ms
        /// Täältä luetaan lämpötilan sisältävää tiedostoa ja kutsutaan kaikkia muita aliohjelmia.
        /// </summary>
        /// <param name="lampoNyt">Reaaliaikainen lämpötila</param>
        /// <param name="aikaNyt">Reaaliaikainen aika</param>
        /// <param name="minArvo">Lämpötilan minimiarvo</param>
        /// <param name="minArvoMuotoiltu">Muotoiltu minimiarvo tulostusta varten</param>
        /// <param name="keskiArvo">Lämpötilojen keskiarvo</param>
        /// <param name="keskiArvoMuotoiltu">Muotoiltu lämpötilojen keskiarvo tulostusta varten</param>
        /// <param name="maxArvo">Lämpötilan maksimiarvo</param>
        /// <param name="maxArvoMuotoiltu">Muotoiltu lämpötilan maksimiarvo tulostusta varten</param>
        var timer1 = new System.Threading.Timer(delegate
        {
            // Haetun lämpötilan lisäys listaan
            lampotilat.Add(HaeLampo(anturipolku));

            string lampoNyt = string.Format("{0:0.0}", HaeLampo(anturipolku));

            // Lämpötilojen haku ja muotoilu tulostusformaattiin
            double minArvo = LaskeMinArvo(lampotilat);
            string minArvoMuotoiltu = string.Format("{0:0.0}", minArvo);
            double keskiArvo = LaskeKeskiArvo(lampotilat);
            string keskiArvoMuotoiltu = string.Format("{0:0.0}", keskiArvo);
            double maxArvo = LaskeMaxArvo(lampotilat);
            string maxArvoMuotoiltu = string.Format("{0:0.0}", maxArvo);

            // jos listan mittausten merkkimäärä kasvaa riittävän suureksi, 
            // rajoitetaan mitausarvot tuhanteen mittaukseen poistamalla vanhin merkki listan alusta
            if (lampotilat.Count >= 1001) lampotilat.RemoveAt(0);

            // TULOSTETAAN OTSIKOT
            Tulosta("L Ä M P Ö T I L A T", 24, 0);
            Tulosta("Min", 9, 3);
            Tulosta("Keskiarvo", 28, 3);
            Tulosta("Max", 51, 3);
            Tulosta("Mittauksia kpl", 26, 7);
            Tulosta("Reaaliaikanen", 26, 12);
            Tulosta("Hälyraja", 6, 12);
            Tulosta("> " + halyRaja, 7, 14);

            // Tulostetaan lämpötilat
            Tulosta(minArvoMuotoiltu, 8, 5);
            Tulosta(keskiArvoMuotoiltu, 30, 5);
            Tulosta(maxArvoMuotoiltu, 50, 5);
            Tulosta(lampoNyt, 30, 15);
            // Muotoillaan ja tulostetaan reaaliaikainen aikaleima
            aikaNyt = DateTime.Now.ToString("HH:mm:ss");
            Tulosta(aikaNyt, 28, 13);

            // Tulostaa max-arvon ja päivittää päiväyksen
            if (maxArvoVanha < double.Parse(maxArvoMuotoiltu))
            {
                maxArvoVanha = double.Parse(maxArvoMuotoiltu);
                maxPvm = DateTime.Now.ToString("dd.MM.yyyy");
                maxKlo = DateTime.Now.ToString("HH:mm:ss");
                Tulosta(maxPvm, 47, 7);                         // Tulostetaan se pvm/klo kun max saavutettu -- TEE TALLENNUSFUNKTIO
                Tulosta(maxKlo, 48, 8);
            }

            // Tulostaa min-arvon ja päivittää päiväyksen
            if (minArvoVanha > double.Parse(minArvoMuotoiltu))
            {
                minArvoVanha = double.Parse(minArvoMuotoiltu);
                minPvm = DateTime.Now.ToString("dd.MM.yyyy");
                minKlo = DateTime.Now.ToString("HH:mm:ss");
                Tulosta(minPvm, 5, 7);
                Tulosta(minKlo, 6, 8);
            }

            // Tulostaa mittaustulosten lukujen määrän
            Tulosta(lampotilat.Count.ToString(), 31, 9);

            // Lähetetään hälytys, jos ei ole jo lähetetty (=false)
            if (double.Parse(lampoNyt) > halyRaja && halyLähetetty == false)
            {
                halyLähetetty = true;
                string lahetysKuittaus = LahetaPosti(halytysOtsikko, halytysViesti);
                Tulosta("                                                                            ", 1, 18);
                Tulosta("Hälytys, lämpötila yli " + halyRaja + " astetta: " + lahetysKuittaus, 9, 18);
            }

            // Hälyksen kuittaus
            // Hälytys pitää olla lähetetty (=true), jotta voidaan lähetttää kuittaus
            if (double.Parse(lampoNyt) < halyRaja && halyLähetetty == true)
            {
                halyLähetetty = false;
                string lahetysKuittaus = LahetaPosti(halytysPoistuiOtsikko, halytysPoistuiViesti);
                Tulosta("                                                                            ", 1, 18);
                Tulosta("Hälytys poistui: " + lahetysKuittaus, 17, 18);
            }


        },
        null, 0, 1000);  // TIMER, aika millisekuntteina


        // Lisätään haettu lämpötila listaan
        lampotilat.Add(listaanLisays);

        // Tärkeä rivi, ohjelma jää lukemaan ajastuksen hoitavaa 
        // riviä "var timer1 = new System.Threading.Timer(delegate..."
        Console.ReadLine();
    }


    /// <summary>
    /// Lukee tiedoston w1_slave sisällön ja parsii sieltä lämpötilan "t=" merkkijonon jälkeen
    /// </summary>
    /// <param name="anturipolku">Anturipolku</param>
    /// <returns>Palauttaa lämpötilan</returns> 
    public static double HaeLampo(string anturipolku)
    {
        double lampoC = 0.0;
        DirectoryInfo hakemistoPolku = new DirectoryInfo(anturipolku);
        foreach (var tiedostoPolku in hakemistoPolku.EnumerateDirectories("28*")) // Luetaan jokainen taulukon alkio kaikista /28* alkavista hakemistoista
        {
            var kokoSisalto = tiedostoPolku.GetFiles("w1_slave").FirstOrDefault().OpenText().ReadToEnd();

            // Jaetaan lämpötilan sisältävä merkkijono kahteen
            string[] tiedostonSisalto = kokoSisalto.Split(new string[] { "t=" }, StringSplitOptions.RemoveEmptyEntries);
            string lampoTeksti = tiedostonSisalto[1];                           // Poimitaan merkkijonosta jälkimmäinen, eli lämpötilan sisältävä merkkijono

            lampoC = double.Parse(lampoTeksti) / 1000;                          // Muutetaan lämpötila numeeriseksi ja jaetaan tuhannella

            // Console.WriteLine(string.Format(" 1-wire-anturin {0} lämpötila {1}C", tiedostoPolku.Name, lampoC));
        }

        return lampoC;

    }


    /// <summary>
    /// Laskee listan lämpötilojen keskiarvon
    /// </summary>
    /// <param name="lampotilat"></param>
    /// <returns>Palauttaa keskiarvon</returns>
    public static double LaskeKeskiArvo(List<double> lampotilat)
    {
        int jakaja = lampotilat.Count;
        double summa = 0.0;
        foreach (double luku in lampotilat) summa += luku;
        double keskiarvo = summa / jakaja;
        return keskiarvo;
    }


    /// <summary>
    /// Etsiin ylimmän lämpötilan
    /// </summary>
    /// <param name="lampotilat"></param>
    /// <returns>Palautetaan ylin lämpötila</returns>
    public static double LaskeMaxArvo(List<double> lampotilat)
    {
        double max = Double.MinValue;
        if (lampotilat.Count > 0) max = lampotilat[0];               // alustetaan max arvo listan ensimmäisellä luvulla
        foreach (double luku in lampotilat)
        {
            if (max <= luku)
            {
                max = luku;
            }
        }
        return max;
    }


    /// <summary>
    /// Etsii alimman lämpötilan
    /// </summary>
    /// <param name="lampotilat"></param>
    /// <returns>Palauttaa alimman lämpötilan</returns>
    public static double LaskeMinArvo(List<double> lampotilat)
    {
        double min = Double.MaxValue;
        if (lampotilat.Count > 0) min = lampotilat[0];              // alustetaan min arvo listan ensimmäisellä luvulla
        foreach (double luku in lampotilat) if (min >= luku) min = luku;
        return min;
    }


    public static int nollaRivi;
    public static int nollaSarake;

    // Tulostus x ja y koordinaattien mukaan

    /// <summary>
    /// Tämä funtio tulostaa annetun viestin, annettuihin x ja y koordinaatteihin 
    /// </summary>
    /// <param name="viesti"></param>
    /// <param name="x">Tulostuksen x-koordinaatti</param>
    /// <param name="y">Tulostuksen y-koordinaatti</param>
    public static void Tulosta(string viesti, int x, int y)
    {
        try
        {
            Console.SetCursorPosition(nollaSarake + x, nollaRivi + y);
            // Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(viesti);
        }
        catch (ArgumentOutOfRangeException e)
        {
            Console.Clear();
            Console.WriteLine(e.Message);
        }
    }


    // https://www.mikrocontroller.net/topic/415557 Pohja täältä, "Windows-pohja" ei toimi Raspberryssä
    /// <summary>
    /// Lähettää sähköpostia parametreina viestin otsikk ja sisältö
    /// </summary>
    /// <param name="otsikko">Viestin otsikko</param>
    /// <param name="sisalto">Viestin sisältö</param>
    /// <returns>Palauttaa tiedon, onnistuiko lähetys</returns>
    public static string LahetaPosti(string otsikko, string sisalto)
    {
        string poikkeus = string.Empty;
        try
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
    (o, certificate, chain, errors) => true;
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;//465  587
            client.EnableSsl = true;
            client.Timeout = 20000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            // MUUTA TILI JA SALASANA OIKEIKSI
            client.Credentials = new NetworkCredential("raspi.varasto@gmail.com",
    "SALASANA");
            MailMessage msg = new MailMessage();
            msg.To.Add("raspi.varasto@gmail.com");
            msg.From = new MailAddress("raspi.varasto@gmail.com");
            msg.Subject = otsikko;
            msg.Body = sisalto;
            client.Send(msg);
            //  Console.WriteLine("Lähetys onnistui");
            return "lähetys onnistui";
        }
        catch (Exception ex)
        {
            poikkeus = ex.Message;
            // Console.WriteLine(poikkeus);
            // Console.WriteLine("Lähetys epäonnistui");
            return "lähetys epäonnistui";
        }

    }


    /// <summary>
    /// Pyytää käyttäjältä ylilämmön hälytysrajaa, 
    /// - jos rajaa ei anneta, asetetaan raja 1000 astetta
    /// - jos lukua ei tunnisteta, asetetaan raja 1000 astetta
    /// - jos luku ei ole väliltä -40 - +40, asetetaan raja 1000 astetta
    /// - jos luku ok, asetetaan se raja-arvoksi
    /// </summary>
    /// <returns>Palautetaan raja-arvo</returns>
    public static double annaHalyRaja()
    {
        double parsittuRaja;
        double rajaArvo = 1000;
        Console.WriteLine("\nAnna ylilämmön hälytysraja -40 - + 40 Celsiusasteen väliltä.\n");
        string annaLuku = Console.ReadLine();
        if (annaLuku.Length == 0)          // jos tyhjä jono
        {
            Console.WriteLine("\nHälytysrajaa ei asetettu.");
            parsittuRaja = 1000;
        }
        else if (double.TryParse(annaLuku, out parsittuRaja))
        {
            if (parsittuRaja < -40 || parsittuRaja > 40)  // jos ei sallituissa rajoissa
            {
                Console.WriteLine("\nLuku ei ole sallituissa rajoissa, hälytystä ei asetettu.");
                parsittuRaja = 1000;
            }
            else
            {
                Console.WriteLine("\nHälytys annetaan kun lämpötila on suurempi kuin " + parsittuRaja + " astetta");
                rajaArvo = parsittuRaja;
            }
        }
        else
        {
            Console.WriteLine("\nLukua ei tunnistettu, hälytys ei käytössä.");
            rajaArvo = 1000;
        }
        return rajaArvo;

    }


}
