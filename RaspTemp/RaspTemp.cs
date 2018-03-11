using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;

/// @author Arto Hopia
/// @version 25.2.2018
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
            Console.WriteLine("Käyttöjäjestelmä on Linux");                  // p=4 Linuxissa
            anturipolku = "/sys/devices/w1_bus_master1/";                    // Hakupolku Raspberryyn
        }
        else
        {
            Console.WriteLine("Käyttöjäjestelmä on Windows");                // p=2 Windowsissa 
            anturipolku = "C:/Ohjelmointi/harjoitustyo/RaspTemp/RaspTemp";   // Hakupolku Windowsiin
        }


        /* TEE NÄMÄ: 
         * lisää mittausajankohta
         * lisää tieto arvojen kokonaismäärästä
         * mittauksen alkuajankohta
         * lisää keskitys
         * tyhjennä näyttö 
         */

        Console.WriteLine(" ");
        //Console.WriteLine();
        // Console.WriteLine("\r\n " + HaeLampo(anturipolku) + "\n");
        double listaanLisays = HaeLampo(anturipolku);               // Haetaan funktiolta lämpötila
        List<double> lampotilat = new List<double>();               // Luodaan lista lämpötiloille


        var timer1 = new System.Threading.Timer(delegate
        {
            lampotilat.Add(HaeLampo(anturipolku));                        // lisää: lämpötilan luku
            Console.WriteLine("Timer1 sisältä  " + HaeLampo(anturipolku));
            // Console.WriteLine("Timer1 sisältä lampötilat:  " + string.Join(" ", lampotilat[0]));

            double keskiArvo = LaskeKeskiArvo(lampotilat);                // Kutsutaan keskiarvon laskevaa aliohjelmaa
            double maxArvo = LaskeMaxArvo(lampotilat);                    // Kutsutaan Max lämpötilan laskevaa aliohjelmaa
            double minArvo = LaskeMinArvo(lampotilat);                    // Kutsutaan Min lämpötilan laskevaa aliohjelmaa

            Console.WriteLine("\nTimer1: Lukujen määrä on {0}", lampotilat.Count);
            Console.WriteLine("\nTimer1: MIN = {0} KESKIARVO = {1}  MAX  = {2} ", minArvo, keskiArvo, maxArvo);

            /// ...
        },
        null, 0, 1000);  // TIMER, TESTATAAN TÄTÄ


        lampotilat.Add(listaanLisays);                              // lisätään lämpötila listaan

        Console.WriteLine("\n" + anturipolku + "\n");
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

    // LISTAN VAIN TESTAUKSEEN, tekee listan                             
    public static List<double> testiLista(List<double> koeLista)    // LUODAAN TESTILISTA TESTAUKSEEN
    {
        for (int i = 50; i <= 100; i++)
        {
            koeLista.Add(i);
        }
        return koeLista;
    }

    public static double LaskeKeskiArvo(List<double> lampotilat)    // Laskee keskiarvon
    {
        int jakaja = lampotilat.Count;                              // listan lukujen määrä
        double summa = 0.0;
        foreach (double luku in lampotilat)                         // käydään luvut läpi
        {
            summa += luku;                                          // lasketaan luvut yhteen
        }
        double keskiarvo = summa / jakaja;                          // lasketan keskiarvo
        return keskiarvo;
    }

    public static double LaskeMaxArvo(List<double> lampotilat)      // laskee Max arvon
    {
        double max = Double.MinValue;
        if (lampotilat.Count > 0) max = lampotilat[0];               // alustetaan max arvo listan ensimmäisellä luvulla
        foreach (double luku in lampotilat)
        {
            if (max <= luku) max = luku;
        }
        return max;
    }

    public static double LaskeMinArvo(List<double> lampotilat)      // laskee Max arvon
    {
        double min = Double.MaxValue;
        if (lampotilat.Count > 0) min = lampotilat[0];              // alustetaan min arvo listan ensimmäisellä luvulla
        foreach (double luku in lampotilat)
        {
            if (min >= luku) min = luku;
        }
        return min;
    }


}
