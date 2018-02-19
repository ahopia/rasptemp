﻿using System;
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
        double lampoC= 0.0;
        DirectoryInfo hakemistoPolku = new DirectoryInfo(anturipolku);
        foreach (var tiedostoPolku in hakemistoPolku.EnumerateDirectories("28*")) // Luetaan jokainen taulukon alkio kaikista /28* alkavista hakemistoista
        {
            var kokoSisalto = tiedostoPolku.GetFiles("w1_slave").FirstOrDefault().OpenText().ReadToEnd();

            //string lampoTeksti = kokoSisalto.Split(new string[] { "t=" }, StringSplitOptions.RemoveEmptyEntries)[1];
            string[] tiedostonSisalto = kokoSisalto.Split(new string[] { "t=" }, StringSplitOptions.RemoveEmptyEntries); 
            string lampoTeksti = tiedostonSisalto[1];       

            lampoC = double.Parse(lampoTeksti) / 1000;

            Console.WriteLine(string.Format(" 1-wire-anturin {0} lämpötila {1}C", tiedostoPolku.Name, lampoC));
        }

        return lampoC;
    }


}
