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


        int p = (int)Environment.OSVersion.Platform;    // Käyttöjärjestelmän tunnistus, jotta voidaan valita oikea hakemistopoku
        if ((p == 4) || (p == 6) || (p == 128))         //  Windowsin ja Rasbianin välillä automaattisesti
        {
            Console.WriteLine("Unix");
        }
        else
        {
            Console.WriteLine("Windows");
        }

        Console.WriteLine();
        Console.WriteLine();
        String anturipolku = "C:/Ohjelmointi/harjoitustyo/RaspTemp/RaspTemp";   // Hakupolku Windowsiin
        // String anturipolku = /sys/devices/w1_bus_master1/28-0517027da1ff     // Hakupolku Raspberryyn
        Console.WriteLine(HaeLampo(anturipolku));
        Console.ReadLine();
    }
    public static double HaeLampo(string anturipolku)
    {
        double lampoC= 0.0;
        DirectoryInfo hakemistoPolku = new DirectoryInfo(anturipolku);
        foreach (var tiedostoPolku in hakemistoPolku.EnumerateDirectories("28*"))
        {
            var kokoSisalto = tiedostoPolku.GetFiles("w1_slave").FirstOrDefault().OpenText().ReadToEnd();

            string lampoTeksti = kokoSisalto.Split(new string[] { "t=" }, StringSplitOptions.RemoveEmptyEntries)[1];

            lampoC = double.Parse(lampoTeksti) / 1000;

            Console.WriteLine(string.Format("1-wire-anturin {0} lämpötila {1}C",
                tiedostoPolku.Name, lampoC));
        }

        return lampoC;
    }


}
