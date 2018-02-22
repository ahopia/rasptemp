using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;

/// @author Arto Hopia
/// @version 16.2.2018
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
        /// Käyttöjärjestelmän tunnistus, jotta voidaan valita oikea hakemistopolku automaattisesti
        /// </summary>
        /// <param name="anturipolku">Hakemistopolku, josta löytyy 1-wireanturi hakemistosta /28*</param>
        /// <param name="w1_slave">Luettava tiedosto</param>
        /// <returns>Valitsee hakemistopolun käyttöjärjestelmän mukaan</returns> 
        int p = (int)Environment.OSVersion.Platform;
        String anturipolku;                             // Hakupolun alustus
        if ((p == 4) || (p == 6) || (p == 128))         // Valinta Windowsin ja Rasbianin välillä automaattisesti
        {
            Console.WriteLine("Käyttöjäjestelmä on Linux");                  // p=4 Linuxissa
            anturipolku = "/sys/devices/w1_bus_master1/";                    // Hakupolku Raspberryyn
        }
        else
        {
            Console.WriteLine("\r\n Käyttöjäjestelmä on Windows");           // p=2 Windowsissa 
            anturipolku = "C:/Ohjelmointi/harjoitustyo/RaspTemp/RaspTemp";   // Hakupolku Windowsiin
        }

        Console.WriteLine("\r\n\r\n");
        //Console.WriteLine();
        Console.WriteLine("\r\n " + HaeLampo(anturipolku));
        double listaanLisays = HaeLampo(anturipolku);   // Haetaan funktiolta lämppötila
        List<double> lampotilat = new List<double>();   // Luodaan lista lämpötiloille
        lampotilat.Add(listaanLisays);                  // lisätään lämpötila listaan

        CreateFileWatcher(anturipolku);
        Console.ReadLine();
    }

    /// <summary>
    /// Lukee tiedoston w1_slave sisällön ja parsii sieltä lämpötilan "t=" merkkijonon jälkeen
    /// </summary>
    /// <param name="lampoC">Lämpötila Celsius asteina</param>
    /// <param name="w1_slave">Luettava tiedosto</param>
    /// <param name="kokoSisalto">Tiedoston sisältö luetaan tähän muuttujaan</param>
    /// <returns>Valitsee hakemistopolun käyttöjärjestelmän mukaan</returns> 
    public static double HaeLampo(string anturipolku)
    {
        double lampoC = 0.0;
        DirectoryInfo hakemistoPolku = new DirectoryInfo(anturipolku);
        foreach (var tiedostoPolku in hakemistoPolku.EnumerateDirectories("28*")) // Luetaan jokainen taulukon alkio kaikista /28* alkavista hakemistoista
        {
            var kokoSisalto = tiedostoPolku.GetFiles("w1_slave").FirstOrDefault().OpenText().ReadToEnd();

            // Jaetaan lämpötilan sisältävä merkkijono kahteen
            string[] tiedostonSisalto = kokoSisalto.Split(new string[] { "t=" }, StringSplitOptions.RemoveEmptyEntries);
            string lampoTeksti = tiedostonSisalto[1];       // Poimitaan merkkijonosta jälkimmäinen, eli lämpötilan sisältävä merkkijono

            lampoC = double.Parse(lampoTeksti) / 1000;      // Muutetaan lämpötila numeeriseksi ja jaetaan tuhannella

            Console.WriteLine(string.Format(" 1-wire-anturin {0} lämpötila {1}C", tiedostoPolku.Name, lampoC));
        }

        return lampoC;
    }

    /// *** https://stackoverflow.com/questions/721714/notification-when-a-file-changes
    public static void CreateFileWatcher(string path)
    {
        // Create a new FileSystemWatcher and set its properties.
        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = path;
        /* Watch for changes in LastAccess and LastWrite times, and 
           the renaming of files or directories. */
        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        // Only watch text files.
        watcher.Filter = "w1_slave";

        // Add event handlers.
        watcher.Changed += new FileSystemEventHandler(OnChanged);
        watcher.Created += new FileSystemEventHandler(OnChanged);
        watcher.Deleted += new FileSystemEventHandler(OnChanged);
        watcher.Renamed += new RenamedEventHandler(OnRenamed);

        // Begin watching.
        watcher.EnableRaisingEvents = true;
    }

    // Define the event handlers.
    private static void OnChanged(object source, FileSystemEventArgs e)
    {
        // Specify what is done when a file is changed, created, or deleted.
        Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
    }

    private static void OnRenamed(object source, RenamedEventArgs e)
    {
        // Specify what is done when a file is renamed.
        Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
    }


}
