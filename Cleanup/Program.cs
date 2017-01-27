using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cleanup {

    class Program {
        public static string _programFiles {
            get {
                //string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                //ProgramFiles = Path.Combine(ProgramFiles, "ImgurSniper");
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
        private static string _docPath {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
                if(!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                return value;
            }
        }

        static void Main(string[] args) {
            Console.Title = "Imgur Sniper Uninstaller";

            //Remove Startmenu Shortcut
            try {
                string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                string shortcutLocation = Path.Combine(commonStartMenuPath, "ImgurSniper" + ".lnk");
                System.IO.File.Delete(shortcutLocation);
            } catch(Exception) { }

            //Remove Desktop Shortcut
            try {
                object shDesktop = (object)"Desktop";
                WshShell shell = new WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Imgur Sniper.lnk";
                System.IO.File.Delete(shortcutAddress);
            } catch(Exception) { }


            try {
                using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"image\shell")) {
                    baseKey.DeleteSubKeyTree("ImgurSniperUpload");
                }
            } catch(Exception) { }


            try {
                KillTasks();

                //Remove all files
                bool notRemoved = false;

                foreach(string filesPrograms in Directory.GetFiles(_programFiles)) {
                    try {
                        System.IO.File.Delete(filesPrograms);
                    } catch(Exception) {
                        notRemoved = true;
                    }
                }
                foreach(string filesDocuments in Directory.GetFiles(_docPath)) {
                    try {
                        System.IO.File.Delete(filesDocuments);
                    } catch(Exception) {
                        notRemoved = true;
                    }
                }

                //Remove Directories
                try {
                    Directory.Delete(_programFiles, true);
                } catch(Exception) { }
                try {
                    Directory.Delete(_docPath, true);
                } catch(Exception) { }


                if(notRemoved)
                    Console.WriteLine("Some Files were not successfully removed!");


                Console.WriteLine("Waiting for all ImgurSniper instances to exit...");

                KillTasks();

                if(Directory.Exists(_docPath)) {
                    Console.WriteLine("Cleaning up User Data...");
                    Directory.Delete(_docPath, true);
                }

                if(Directory.Exists(_programFiles)) {
                    Console.WriteLine("Removing Program Files...");
                    Directory.Delete(_programFiles, true);
                }


                Console.WriteLine("Sad to see you go! Bye :(");

                Console.Write("\n\nPress any key to continue...");
                Console.ReadKey();
            } catch(Exception ex) {
                Console.WriteLine("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message);
            }
        }

        private static void KillTasks() {
            try {
                List<Process> processes = new List<Process>(Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper")));
                foreach(Process p in processes) {
                    if(p.ProcessName != Process.GetCurrentProcess().ProcessName)
                        p.Kill();
                }
            } catch(Exception) { }
        }
    }
}
