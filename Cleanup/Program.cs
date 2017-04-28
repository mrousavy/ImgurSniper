using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace Cleanup {
    internal class Program {
        private static string DocPath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");

        private static void Main(string[] args) {
            Console.Title = "Uninstalling ImgurSniper";

            try {
                KillTasks();

                //Remove all files
                bool notRemoved = false;

                //Remove Documents/ImgurSniper/[files]
                foreach (string filesDocuments in Directory.GetFiles(DocPath)) {
                    try {
                        File.Delete(filesDocuments);
                    } catch {
                        notRemoved = true;
                    }
                }

                //Remove Documents/ImgurSniper/[directories]
                foreach (string dirs in Directory.GetDirectories(DocPath)) {
                    try {
                        Directory.Delete(dirs, true);
                    } catch {
                        notRemoved = true;
                    }
                }

                //Remove Documents/ImgurSniper
                try {
                    Directory.Delete(DocPath, true);
                } catch { }

                //Remove Documents/ImgurSniperImages
                try {
                    Directory.Delete(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            "ImgurSniperImages"), true);
                } catch { }


                if (notRemoved) {
                    Console.WriteLine("Error");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Could not remove all Files ({ex.Message})! Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void KillTasks() {
            try {
                List<Process> processes =
                    new List<Process>(Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper")));
                foreach (Process p in processes) {
                    if (p.Id != Process.GetCurrentProcess().Id) {
                        p.Kill();
                    }
                }
            } catch { }
        }
    }
}