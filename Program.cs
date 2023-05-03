using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;

namespace ConsoleApp
{
    [Serializable]
    public class Colection : IComparer<String>
    {
        public int Compare(string? x, string? y)
        {
            if (x.Length < y.Length)
                return -1;
            if (y.Length < x.Length)
                return 1;
            return string.Compare(x, y);
        }
    }
    public static class Extensions
    {
        public static DateTime findOldest(this DirectoryInfo directory)
        {
            DateTime check = oldestFile(directory, DateTime.Now);
            DateTime oldest = oldestDirectory(directory, check);
            if (check < oldest)
                oldest = check;
            return oldest;

        }

        static DateTime oldestFile(this DirectoryInfo directory, DateTime oldest)
        {
            foreach(var file in directory.GetFiles())
            {
                if(file.CreationTime < oldest)
                    oldest = file.CreationTime;
            }
            return oldest;
        }
        static DateTime oldestDirectory(this DirectoryInfo directory, DateTime oldest)
        {
            DateTime check;
            foreach (var dir in directory.GetDirectories())
            {
                if (dir.CreationTime < oldest)
                    oldest = dir.CreationTime;

                // sprawdzenie rekurencyjne katalogu
                check = oldestDirectory(dir, oldest);
                if (check < oldest)
                    oldest = check;

                check = oldestFile(dir, oldest);
                if (check < oldest)
                    oldest = check;
            }
            return oldest;
        }

        public static string getRAHS(this FileSystemInfo item)
        {
            string rahs = "";
            rahs += item.Attributes.HasFlag(FileAttributes.ReadOnly) ? "r" : "-";
            rahs += item.Attributes.HasFlag(FileAttributes.Archive) ? "a" : "-";
            rahs += item.Attributes.HasFlag(FileAttributes.Hidden) ? "h" : "-";
            rahs += item.Attributes.HasFlag(FileAttributes.System) ? "s" : "-";
            return rahs;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo directory = new DirectoryInfo(args[0]);
            showDirectories(directory, 0);
            showFiles(directory, 0);

            // wypisanie daty najstarszego pliku
            Console.WriteLine("\nNajstarszy plik: " + Extensions.findOldest(directory) + "\n");

            // struktura
            var structure = new SortedDictionary<string, string>(new Colection());
            addDirectories(directory, structure);
            addFiles(directory, structure);

            // serializacja
            var formatter = new BinaryFormatter();
            FileStream fs = new FileStream("DataFile.dat", FileMode.OpenOrCreate);
            formatter.Serialize(fs, structure);
            fs.Close();

            // deserializacja
            fs = new FileStream("DataFile.dat", FileMode.OpenOrCreate);
            SortedDictionary<string, string> dictionary = (SortedDictionary<string, string>)formatter.Deserialize(fs);
            fs.Close();

            foreach(var de in dictionary)
            {
                Console.WriteLine("{0} -> {1}", de.Key, de.Value);
            }
        }

        static void addFiles(DirectoryInfo directory, SortedDictionary<string, string> structure)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                structure.Add(file.Name, file.Length.ToString());
            }
        }

        static void addDirectories(DirectoryInfo directory, SortedDictionary<string, string> structure)
        {
            foreach (var dir in directory.EnumerateDirectories())
            {
                structure.Add(dir.Name, dir.EnumerateFileSystemInfos().Count().ToString());
            }
        }

        static void showFiles(DirectoryInfo directory, int shift)
        {
            foreach(var file in directory.EnumerateFiles())
            {
                for (int i = 0; i < shift; i++)
                    Console.Write("    ");
                Console.Write(file.Name + " " + file.Length + " bajtów " + Extensions.getRAHS(file));
                Console.WriteLine();
            }
        }

        static void showDirectories(DirectoryInfo directory, int shift)
        {
            foreach (var dir in directory.EnumerateDirectories())
            {
                for (int i = 0; i < shift; i++)
                    Console.Write("    ");
                Console.Write(dir.Name + " (" + dir.EnumerateFileSystemInfos().Count() + ") " + Extensions.getRAHS(dir));
                Console.WriteLine();

                showDirectories(dir, shift + 1);
                showFiles(dir, shift + 1);
            }
        }
    }
}