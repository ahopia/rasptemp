using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;

/// @author arto
/// @version 11.2.2018
/// <summary>
/// 
/// </summary>
public class RaspTemp
{
    /// <summary>
    /// Testausta vielä
    /// </summary>
    public static void Main()
    {
        Console.WriteLine();
        Console.WriteLine("Kakkosversio");
        String anturipolku = "C:/Ohjelmointi/harjoitustyo/RaspTemp/RaspTemp";
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
