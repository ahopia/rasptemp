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

        // bool lopeta = true;
        // while (lopeta == true)                                        // tarvitaanko silmukkaa
        // {                                                             // TEE PÄÄTTYMÄTÖN SILMUKKA 
        Console.WriteLine(" ");
        //Console.WriteLine();
        // Console.WriteLine("\r\n " + HaeLampo(anturipolku) + "\n");
        double listaanLisays = HaeLampo(anturipolku);               // Haetaan funktiolta lämpötila
        List<double> lampotilat = new List<double>();               // Luodaan lista lämpötiloille

        // var timer1 = new System.Threading.Timer(_ => lampotilat.Add(HaeLampo(anturipolku)), null, 0, 1000);  // TIMER, TESTATAAN TÄTÄ
        var timer2 = new System.Threading.Timer(_ => Console.WriteLine(HaeLampo(anturipolku)), null, 0, 1000);  // TIMER, TESTATAAN TÄTÄ


        // var timer1 = new System.Threading.Timer(_ => Console.WriteLine(HaeLampo(anturipolku)), null, 0, 1000);  // TIMER, TESTATAAN TÄTÄ
        var timer1 = new System.Threading.Timer(delegate
        {
            lampotilat.Add(HaeLampo(anturipolku));  // lisää: lämpötilan luku

            /// ...
        },
        null, 0, 1000);  // TIMER, TESTATAAN TÄTÄ


        //  var timer2 = new System.Threading.Timer(_ => Console.WriteLine(HaeLampo(anturipolku)), null, 0, 1000);  // TIMER, TESTATAAN TÄTÄ
        lampotilat.Add(listaanLisays);                              // lisätään lämpötila listaan

        List<double> koeLista = new List<double>();                 // TESTILISTA ALUSTETAAN  
        List<double> koeTulos = testiLista(koeLista);               // TESTILISTA LISÄTÄÄN TESTAUSTA VARTEN LUKUJA ALIOHJELMASSA  
        double koeArvojenTulos = LaskeKeskiArvo(koeLista);          // TESTILISTA, KÄYDÄÄN LASKEMASSA KESKIARVO

        double keskiArvo = LaskeKeskiArvo(koeLista);                // Kutsutaan keskiarvon laskevaa aliohjelmaa
        double maxArvo = LaskeMaxArvo(koeLista);                    // Kutsutaan Max lämpötilan laskevaa aliohjelmaa
        double minArvo = LaskeMinArvo(koeLista);                    // Kutsutaan Min lämpötilan laskevaa aliohjelmaa

        Console.WriteLine("\nTESTILISTAN KESKIARVO ON {0} \nTESTILISTAN LUKUJEN MÄÄRÄ ON {1}", koeArvojenTulos, koeLista.Count);    // TESTILISTA, TULOSTETAAN KESKIARVO
        Console.WriteLine("\nTESTILISTAN LUKUJEN MAX ARVO = {0} JA MIN ARVO = {1} ", maxArvo, minArvo);

        Console.WriteLine("\n" + anturipolku + "\n");
        // CreateFileWatcher(anturipolku + "28-0517027da1ff"); 
        Console.ReadLine();
        //}
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

    /// <summary>
    /// Lasketaan listan lukemien keskiarvo
    /// </summary>
    /// <param name="lampotilat">Lista lämpötiloista</param>
    /// <returns>Palauttaa lämpötilojen keskiarvon</returns> 
    /// <example>
    /// <pre name="test">
    ///  RaspTemp.LaskeKeskiArvo(new List<double>() {5.0, 15.0 }) ~~~ 10.0; 
    /// </pre>
    /// </example>
    public static double LaskeKeskiArvo(List<double> lampotilat)
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

    /// <summary>
    /// Laskee taulukon lukujen max arvon   
    /// </summary>
    /// <param name="lampotilat">Lista lämpötiloista</param>
    /// <returns>Palauttaa lämpötilojen maksimiarvon</returns> 
    /// <example>
    /// <pre name="test">
    /// RaspTemp.LaskeMaxArvo(new List<double>() {}) ~~~ Double.MinValue; 
    /// RaspTemp.LaskeMaxArvo(new List<double>() {1.0, 20.0, 25.0}) ~~~ 25.0;
    /// RaspTemp.LaskeMaxArvo(new List<double>() {23.0, 56.0, 12.0}) ~~~ 56.0;
    /// RaspTemp.LaskeMaxArvo(new List<double>() {14.0, 13.0, 8.0}) ~~~ 14.0;
    /// RaspTemp.LaskeMaxArvo(new List<double>() {3.0, 7.0, 1.8}) ~~~ 7.0;
    /// </pre>
    /// </example>
    public static double LaskeMaxArvo(List<double> lampotilat)
    {
        double max = Double.MinValue;
        if (lampotilat.Count > 0) max = lampotilat[0];                                 // alustetaan max arvo listan ensimmäisellä luvulla
        foreach (double luku in lampotilat)
        {
            if (max <= luku) max = luku;
        }
        return max;
    }

    /// <summary>
    /// Laskee taulukon lukujen min arvon   
    /// </summary>
    /// <param name="lampotilat">Lista lämpötiloista</param>
    /// <returns>Palauttaa lämpötilojen minimiarvon</returns>  
    /// <example>
    /// <pre name="test">
    /// RaspTemp.LaskeMinArvo(new List<double>() {}) ~~~ Double.MaxValue; 
    /// RaspTemp.LaskeMinArvo(new List<double>() {1.0, 20.0, 25.0}) ~~~ 1.0;
    /// RaspTemp.LaskeMinArvo(new List<double>() {23.0, 56.0, 12.0}) ~~~ 12.0;
    /// RaspTemp.LaskeMinArvo(new List<double>() {14.0, 13.0, 8.0}) ~~~ 8.0;
    /// RaspTemp.LaskeMinArvo(new List<double>() {3.0, 7.0}) ~~~ 3.0;
    /// RaspTemp.LaskeMinArvo(new List<double>() {3.0}) ~~~ 3.0;
    /// </pre>
    /// </example>
    public static double LaskeMinArvo(List<double> lampotilat)
    {
        double min = Double.MaxValue;
        if (lampotilat.Count > 0) min = lampotilat[0];                                 // alustetaan min arvo listan ensimmäisellä luvulla
        foreach (double luku in lampotilat)
        {
            if (min >= luku) min = luku;
        }
        return min;
    }


}
