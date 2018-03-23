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
        // TODO: PÄIVÄYKSEN TULOSTUS MIN / MAX LÄMPÖJEN ALLE
        // TODO: RASPBERRYSSÄ EI TOIMI DateTime.Now --LÖYTYYKÖ PATCHI
        // TODO: SÄHKÖPOSTIIN HÄLYTYS KUN RAJA-ARVO ON YLITETTY / ALITETTU

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


        // Tyhjennä näyttö, tallenna alkukoordinaatit, ylin ja vasemmanpuoleisin
        Console.Clear();
        nollaRivi = Console.CursorTop + 2;      // tuunaa nollarivi sopivaan kohtaan
        nollaSarake = Console.CursorLeft + 5;   // tuunaa nollasarake sopivaan kohtaan

        //
        // Tulosta("Toimii", 10, 16);
        // Tulosta("ARTON TESTITULOSTUS", 8, -1);
        Console.WriteLine();

        Console.WriteLine(" ");
        //Console.WriteLine();
        // Console.WriteLine("\r\n " + HaeLampo(anturipolku) + "\n");
        double listaanLisays = HaeLampo(anturipolku);               // Haetaan funktiolta lämpötila
        List<double> lampotilat = new List<double>();               // Luodaan lista lämpötiloille
        string minPvm; // = DateTime.Now.ToString("dd.MM.yyyy");        // RASPBERRYSSÄ NÄMÄ EIVÄT TOIMINEET, JATKA SELVITTÄMISTÄ
        string minKlo; // = DateTime.Now.ToString("HH:mm:ss");
        string maxPvm; // = DateTime.Now.ToString("dd.MM.yyyy");
        string maxKlo; // = DateTime.Now.ToString("HH:mm:ss");
        string aikaNyt;
        double maxArvoVanha = Double.MinValue;
        double minArvoVanha = Double.MaxValue;

        var timer1 = new System.Threading.Timer(delegate
        {
            lampotilat.Add(HaeLampo(anturipolku));                        // lisää: lämpötilan luku
                                                                          //  Console.WriteLine("Timer1 sisältä  " + HaeLampo(anturipolku));
            string nykyArvoMuotoiltu = string.Format("{0:0.0}", HaeLampo(anturipolku));                                                             // Console.WriteLine("Timer1 sisältä lampötilat:  " + string.Join(" ", lampotilat[0]));

            double minArvo = LaskeMinArvo(lampotilat);                    // Kutsutaan Min lämpötilan laskevaa aliohjelmaa
            string minArvoMuotoiltu = string.Format("{0:0.0}", minArvo);
            double keskiArvo = LaskeKeskiArvo(lampotilat);                // Kutsutaan keskiarvon laskevaa aliohjelmaa
            string keskiArvoMuotoiltu = string.Format("{0:0.0}", keskiArvo);
            double maxArvo = LaskeMaxArvo(lampotilat);                    // Kutsutaan Max lämpötilan laskevaa aliohjelmaa
            string maxArvoMuotoiltu = string.Format("{0:0.0}", maxArvo);

            // jos listan mittausten merkkimäärä kasvaa riittävän suureksi, 
            // rajoitetaan mitausarvot tuhanteen mittaukseen poistamalla vanhin merkki listan alusta
            if (lampotilat.Count >= 1001) lampotilat.RemoveAt(0);

            Tulosta("L Ä M P Ö T I L A T", 24, 0);          // TULOSTETAAN OTSIKOT
            Tulosta("Min", 9, 3);
            Tulosta("Keskiarvo", 28, 3);
            Tulosta("Max", 51, 3);
            Tulosta("Mittauksia kpl", 26, 7);
            Tulosta("Reaaliaikanen", 26, 12);

            Tulosta(minArvoMuotoiltu, 8, 5);                // TULOSTETAAN LÄMPÖTILAT
            Tulosta(keskiArvoMuotoiltu, 30, 5);
            Tulosta(maxArvoMuotoiltu, 50, 5);
            Tulosta(nykyArvoMuotoiltu, 30, 15); // tähän nykyarvo

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

        },
        null, 0, 1000);  // TIMER, aika millisekuntteina


        lampotilat.Add(listaanLisays);                              // lisätään lämpötila listaan

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


    public static double LaskeKeskiArvo(List<double> lampotilat)    // Laskee keskiarvon
    {
        int jakaja = lampotilat.Count;                              // listan lukujen määrä
        double summa = 0.0;
        foreach (double luku in lampotilat) summa += luku;                         // käydään luvut läpi// lasketaan luvut yhteen
        double keskiarvo = summa / jakaja;                          // lasketan keskiarvo
        return keskiarvo;
    }

    public static double LaskeMaxArvo(List<double> lampotilat)      // laskee Max arvon
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

    public static double LaskeMinArvo(List<double> lampotilat)      // laskee Max arvon
    {
        double min = Double.MaxValue;
        if (lampotilat.Count > 0) min = lampotilat[0];              // alustetaan min arvo listan ensimmäisellä luvulla
        foreach (double luku in lampotilat) if (min >= luku) min = luku;
        return min;
    }



    public static int nollaRivi;
    public static int nollaSarake;

    public static void Tulosta(string viesti, int x, int y)                 // TULOSTUS
    {
        try
        {
            Console.SetCursorPosition(nollaSarake + x, nollaRivi + y);        // NÄILLÄ SAA TULOSTUMAAN OIKEAAN KOHTAAN
                                                                              // Console.ForegroundColor = ConsoleColor.DarkYellow;
                                                                              // Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(viesti);
        }
        catch (ArgumentOutOfRangeException e)
        {
            Console.Clear();
            Console.WriteLine(e.Message);
        }

    }


}
