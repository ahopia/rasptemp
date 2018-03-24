// This example demonstrates the 
//     Console.CursorLeft and 
//     Console.CursorTop properties, and the
//     Console.SetCursorPosition and 
//     Console.Clear methods.
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;

/// @author Arto Hopia
/// @version 22.3.2018
/// <summary>
/// Ohjelma lukee Raspberryssä 1-wire-anturin tietoa ja näyttää lämpötilan
/// </summary>
public class RaspTemp
{
    /// <summary>
    /// Testausta vielä
    /// </summary>
    public static void Main()
    {
        /// <summary>
        /// Tunnistetaan käyttöjärjestelmä, koska anturin lämpätilatiedon sisältämä tiedosto on eri polussa Raspberryllä ja Windowsilla
        /// Käyttöjärjestelmästä riippuen otetaan oikea polku käyttöön automaattisesti
        /// </summary>
        /// <param name="anturipolku">Anturin polku</param>
        int p = (int)Environment.OSVersion.Platform;
        String anturipolku;                                                  // Hakupolun alustus
        if ((p == 4) || (p == 6))                                            // Valinta Windowsin ja Rasbianin välillä automaattisesti
        {
            Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "1");
            //  Console.WriteLine("Käyttöjäjestelmä on Linux");                  // p=4 Linuxissa
            anturipolku = "/sys/devices/w1_bus_master1/";                    // Hakupolku Raspberryyn
        }
        else
        {
            //  Console.WriteLine("Käyttöjäjestelmä on Windows");                // p=2 Windowsissa 
            anturipolku = "C:/Ohjelmointi/harjoitustyo/RaspTemp/RaspTemp";   // Hakupolku Windowsiin
        }


        // Aluksi pyydetään käyttäjältä hälytysraja
        double halyRaja = annaHalyRaja();
        Console.WriteLine("\npaina jotain jatkaaksesi.");
        Console.ReadLine();


        // Tyhjennä näyttö, tallenna alkukoordinaatit, ylin ja vasemmanpuoleisin
        Console.Clear();
        // tuunaa nollarivi ja sarake sopivaan kohtaan
        nollaRivi = Console.CursorTop + 2;
        nollaSarake = Console.CursorLeft + 5;

        Console.WriteLine();

        // Sähköpostin lähetykseen liittyvät alustukset, säädä hälyRajalla sähköpostihälytyksen kynnystä
        // double halyRaja = 26;
        string halytysOtsikko = "HÄLYTYS, lämpötila yli " + halyRaja + " astetta";
        string halytysViesti = "HÄLYTYS, lämpötila on nyt yli " + halyRaja + " astetta.";

        string halytysPoistuiOtsikko = "POISTUI, HÄLYTYS, lämpötila yli " + halyRaja + " astetta";
        string halytysPoistuiViesti = "POISTUI, HÄLYTYS, lämpötila on nyt yli " + halyRaja + " astetta.";

        bool halyLähetetty = false;

        Console.WriteLine(" ");
        // Console.WriteLine("\r\n " + HaeLampo(anturipolku) + "\n");
        double listaanLisays = HaeLampo(anturipolku);               // Alustetaan listaanlisäys 
        List<double> lampotilat = new List<double>();               // Luodaan lista lämpötiloille
        string minPvm; // = DateTime.Now.ToString("dd.MM.yyyy");    // RASPBERRYSSÄ NÄMÄ EIVÄT AINA TOIMINEET, JATKA SELVITTÄMISTÄ
        string minKlo; // = DateTime.Now.ToString("HH:mm:ss");
        string maxPvm; // = DateTime.Now.ToString("dd.MM.yyyy");
        string maxKlo; // = DateTime.Now.ToString("HH:mm:ss");
        string aikaNyt;
        double maxArvoVanha = Double.MinValue;
        double minArvoVanha = Double.MaxValue;

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

            // TULOSTETAAN LÄMPÖTILAT
            Tulosta(minArvoMuotoiltu, 8, 5);
            Tulosta(keskiArvoMuotoiltu, 30, 5);
            Tulosta(maxArvoMuotoiltu, 50, 5);
            Tulosta(lampoNyt, 30, 15); // tähän nykyarvo

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
                Tulosta(minPvm, 5, 7);                          // Tulostetaan se pvm/klo kun min saavutettu -- TEE TALLENNUSFUNKTIO
                Tulosta(minKlo, 6, 8);
            }
            Tulosta(lampotilat.Count.ToString(), 31, 9);   // tulostaa lukujen määrän

            // Hälytyksen lähetys, jos ei ole jo lähetetty (false)
            if (double.Parse(lampoNyt) > halyRaja && halyLähetetty == false)
            {
                halyLähetetty = true;
                string lahetysKuittaus = lahetaPosti(halytysOtsikko, halytysViesti);
                Tulosta("                                                                            ", 1, 18);
                Tulosta("Hälytys, lämpötila yli " + halyRaja + " astetta: " + lahetysKuittaus, 9, 18);
            } // LISÄÄ UUDELLEEN LÄHETYS, JOS LÄHETYS EPÄONNISTUI

            // Hälyksen kuittaus
            // Hälytys pitää olla lähetetty (true), jotta voidaan lähetttää kuittaus
            if (double.Parse(lampoNyt) < halyRaja && halyLähetetty == true)
            {
                halyLähetetty = false;
                string lahetysKuittaus = lahetaPosti(halytysPoistuiOtsikko, halytysPoistuiViesti);
                Tulosta("                                                                            ", 1, 18);
                Tulosta("Hälytys poistui: " + lahetysKuittaus, 17, 18);
            }// LISÄÄ UUDELLEEN LÄHETYS, JOS LÄHETYS EPÄONNISTUI

        },
        null, 0, 1000);  // TIMER, aika millisekuntteina

        // lisätään lämpötila listaan
        lampotilat.Add(listaanLisays);

        // Console.WriteLine("\n" + anturipolku + "\n");

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

    // Lasketaan keski-arvo
    public static double LaskeKeskiArvo(List<double> lampotilat)
    {
        int jakaja = lampotilat.Count;
        double summa = 0.0;
        foreach (double luku in lampotilat) summa += luku;
        double keskiarvo = summa / jakaja;
        return keskiarvo;
    }

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

    // https://www.mikrocontroller.net/topic/415557 Pohja täältä
    public static string lahetaPosti(string otsikko, string sisalto)
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
            client.Credentials = new NetworkCredential("raspi.X@gmail.com",
    "SALASANA");
            MailMessage msg = new MailMessage();
            msg.To.Add("raspi.X@gmail.com");
            msg.From = new MailAddress("raspi.X@gmail.com");
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

    // Kysytään käyttäjältä hälyrajaa, jos tyhjä, ei rajoissa tai 
    // merkkiä ei tunnisteta, asetetaan oletus 1000 astetta
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
