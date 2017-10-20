using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace DiskSpaceChecker
{
    public class Program
    {
        private static string logFile = @"DiskSpaceSummary";
        private static Dictionary<string, long> pathSizeSummary;

        static void Main(string[] args)
        {
            pathSizeSummary = new Dictionary<string, long>();
            logFile = $"{logFile}-{DateTime.Now.ToString("yyyyMMddHHMMss")}.csv";
            var path = args[0];
            var targetDirectory = new DirectoryInfo(path);

            WriteHeader();
            ScanDirectory(targetDirectory);
            SortAndWriteToFile(pathSizeSummary);

            Console.WriteLine("Finished. Press any key to exit");
            Console.ReadLine();
        }

        static void ScanDirectory(DirectoryInfo directory)
        {
            try
            {
                Console.WriteLine($"Scanning {directory.FullName}");
                var files = directory.EnumerateFiles();
                var totalFiles = files.Count();
                var totalSize = files.Sum(f => f.Length);
                var earliestFile = files.Any() ? files.Min(f => f.LastWriteTime).ToString("dd-MMM-yyyy") : string.Empty;
                var latestFile = files.Any() ? files.Max(f => f.LastWriteTime).ToString("dd-MMM-yyyy") : string.Empty;
                
                var row = $"{directory.FullName},{totalFiles},{ConvertToFormattedMegaBytes(totalSize)},{earliestFile},{latestFile}";
                pathSizeSummary.Add(row, totalSize);
                foreach (var dir in directory.EnumerateDirectories())
                {
                    ScanDirectory(dir);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                var row = string.Format("{0},{1},{2},{3},{4}", directory.FullName, "Unauthorized Access", "na", "na", "na");
                Log(row);
            }
        }


        static string ConvertToFormattedMegaBytes(long bytes)
        {
            decimal megaBytes = ((decimal) bytes) / 1000 / 1000;
            return megaBytes.ToString("0.00");
        }

        static void SortAndWriteToFile(Dictionary<string, long> data)
        {
            foreach (var item in data.OrderByDescending(x => x.Value))
            {
                Log(item.Key);
            }
        }

        static void WriteHeader()
        {
            var header = "Path,Files,Size,First,Last";
            Log("Path,Files,Size,First,Last");
        }

        static void Log(string text)
        {
            Console.WriteLine(text);
            if (!File.Exists(logFile))
            {
                using (StreamWriter sw = File.CreateText(logFile))
                {
                    sw.WriteLine(text);
                }
                return;
            }

            using (StreamWriter sw = File.AppendText(logFile))
            {
                sw.WriteLine(text);
            }
        }
    }
}
