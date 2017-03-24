using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cleanup {

    class Program {
        private static string DocPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");

        static void Main(string[] args) {
            Console.Title = "Uninstalling ImgurSniper";

            //Remove Startmenu Shortcut
            try {
                string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                string shortcutLocation = Path.Combine(commonStartMenuPath, "ImgurSniper" + ".lnk");
                System.IO.File.Delete(shortcutLocation);

                shortcutLocation = Path.Combine(commonStartMenuPath, "ImgurSniper Settings" + ".lnk");
                System.IO.File.Delete(shortcutLocation);

                Console.WriteLine("Removed Start Menu Shortcut..");
            } catch {
                Console.WriteLine("Could not remove Start Menu Shortcut! Press any key to continue...");
                Console.ReadKey();
            }

            //Remove Desktop Shortcut
            try {
                object shDesktop = (object)"Desktop";
                WshShell shell = new WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Imgur Sniper.lnk";
                System.IO.File.Delete(shortcutAddress);
                shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Imgur Sniper Settings.lnk";
                System.IO.File.Delete(shortcutAddress);

                Console.WriteLine("Removed Desktop Shortcut..");
            } catch {
                Console.WriteLine("Could not remove Desktop Shortcut! Press any key to continue...");
                Console.ReadKey();
            }

            //Remove Context Menu Shortcut
            try {
                using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"*\shell")) {
                    baseKey.DeleteSubKeyTree("ImgurSniperUpload");
                }
            } catch {
                Console.WriteLine("Could not remove Context Menu Shortcut! Press any key to continue...");
                Console.ReadKey();
            }

            //Remove Autostart
            try {
                using(RegistryKey baseKey =
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")) {
                    baseKey.DeleteValue("ImgurSniper");
                }
            } catch {
                Console.WriteLine("Could not remove Autostart/Run Registry Entry! Press any key to continue...");
                Console.ReadKey();
            }

            try {
                KillTasks();

                //Remove all files
                bool notRemoved = false;

                //Remove Documents/ImgurSniper/[files]
                foreach(string filesDocuments in Directory.GetFiles(DocPath)) {
                    try {
                        System.IO.File.Delete(filesDocuments);
                    } catch {
                        notRemoved = true;
                    }
                }

                //Remove Documents/ImgurSniper/[directories]
                foreach(string dirs in Directory.GetDirectories(DocPath)) {
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
                    Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniperImages"), true);
                } catch { }


                if(notRemoved)
                    Console.WriteLine("Error");
            } catch(Exception ex) {
                Console.WriteLine($"Could not remove all Files ({ex.Message})! Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void KillTasks() {
            try {
                List<Process> processes = new List<Process>(Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper")));
                foreach(Process p in processes) {
                    if(p.Id != Process.GetCurrentProcess().Id)
                        p.Kill();
                }
            } catch { }
        }
    }
}
