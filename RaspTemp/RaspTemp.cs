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
/// @version 21.3.2018
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


        // Tyhjennä näyttö, tallenna alkukoordinaatit, ylin ja vasemmanpuoleisin
        // Console.Clear();
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
        string minPvm = DateTime.Now.ToString("dd.MM.yyyy");
        string minKlo = DateTime.Now.ToString("HH:mm:ss");
        string maxPvm = DateTime.Now.ToString("dd.MM.yyyy");
        string maxKlo = DateTime.Now.ToString("HH:mm:ss");

        var timer1 = new System.Threading.Timer(delegate
        {
            lampotilat.Add(HaeLampo(anturipolku));                        // lisää: lämpötilan luku
                                                                          //  Console.WriteLine("Timer1 sisältä  " + HaeLampo(anturipolku));
                                                                          // Console.WriteLine("Timer1 sisältä lampötilat:  " + string.Join(" ", lampotilat[0]));

            double keskiArvo = LaskeKeskiArvo(lampotilat);                // Kutsutaan keskiarvon laskevaa aliohjelmaa
            double maxArvo = LaskeMaxArvo(lampotilat);                    // Kutsutaan Max lämpötilan laskevaa aliohjelmaa
            double minArvo = LaskeMinArvo(lampotilat);                    // Kutsutaan Min lämpötilan laskevaa aliohjelmaa

            Tulosta("L Ä M P Ö T I L A T", 24, 0);
            Tulosta("Min", 9, 3);
            Tulosta("Keskiarvo", 28, 3);
            Tulosta("Max", 51, 3);

            Tulosta(minArvo.ToString(), 7, 5);
            Tulosta(keskiArvo.ToString(), 29, 5);
            Tulosta(maxArvo.ToString(), 50, 5);

            Tulosta("Lukujen määrä", 26, 8);
            Tulosta(lampotilat.Count.ToString(), 31, 10);       // tulostaa lukujen määrän

            Tulosta(minPvm, 6, 7);
            Tulosta(minKlo, 7, 8);
            Tulosta(maxPvm, 48, 7);
            Tulosta(maxKlo, 49, 8);


        },
        null, 0, 1000);  // TIMER, TESTATAAN TÄTÄ


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
        foreach (double luku in lampotilat) if (max <= luku) max = luku;
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
