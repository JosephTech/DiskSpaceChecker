using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DiskSpaceChecker
{
    public class Program
    {
        private static string logFile = @"DiskSpaceSummary";
        private static bool headerAdded = false;

        static void Main(string[] args)
        {
            logFile = $"{logFile}-{DateTime.Now.ToString("yyyyMMddHHMMss")}.csv";
            var path = args[0];
            var targetDirectory = new DirectoryInfo(path);
            ScanDirectory(targetDirectory);
            
            Console.WriteLine("Finished. Press any key to exit");
            Console.ReadLine();
        }

        static void ScanDirectory(DirectoryInfo directory)
        {
            try
            {
                var files = directory.EnumerateFiles();
                var totalFiles = files.Count();
                var totalSize = files.Sum(f => f.Length);
                var earliestFile = files.Any() ? files.Min(f => f.LastWriteTime).ToString("dd-MMM-yyyy") : string.Empty;
                var latestFile = files.Any() ? files.Max(f => f.LastWriteTime).ToString("dd-MMM-yyyy") : string.Empty;

                if (!headerAdded)
                {
                    var header = "Path,Files,Size,First,Last";
                    Log(header);
                    headerAdded = true;
                }

                var row = $"{directory.FullName},{totalFiles},{ConvertToFormattedMegaBytes(totalSize)},{earliestFile},{latestFile}";
                Log(row);
                foreach (var dir in directory.EnumerateDirectories())
                {
                    ScanDirectory(dir);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                var row = string.Format("{0},{1},{2},{3},{4}", directory.FullName, "na", "na", "na", "na");
                Log(row);
            }
        }


        static string ConvertToFormattedMegaBytes(long bytes)
        {
            decimal megaBytes = ((decimal) bytes) / 1000 / 1000;
            return megaBytes.ToString("0.00");
        }

        static void Log(string text)
        {
            Console.WriteLine(text);
            StreamWriter outputFile;
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
