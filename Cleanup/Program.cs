using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Cleanup {

    class Program {
        public static string _programFiles => AppDomain.CurrentDomain.BaseDirectory;

        private static string _docPath {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
                if(!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                return value;
            }
        }

        static void Main(string[] args) {
            Console.Title = Properties.strings.uninstallTitle;

            //Remove Startmenu Shortcut
            try {
                string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                string shortcutLocation = Path.Combine(commonStartMenuPath, "ImgurSniper" + ".lnk");
                System.IO.File.Delete(shortcutLocation);
            } catch { }

            //Remove Desktop Shortcut
            try {
                object shDesktop = (object)"Desktop";
                WshShell shell = new WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Imgur Sniper.lnk";
                System.IO.File.Delete(shortcutAddress);
            } catch { }


            try {
                using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"image\shell")) {
                    baseKey.DeleteSubKeyTree("ImgurSniperUpload");
                }
            } catch { }


            try {
                using(
                    RegistryKey baseKey =
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")) {
                    baseKey.DeleteValue("ImgurSniper");
                }
            } catch { }

            try {
                KillTasks();

                //Remove all files
                bool notRemoved = false;

                foreach(string filesPrograms in Directory.GetFiles(_programFiles)) {
                    try {
                        System.IO.File.Delete(filesPrograms);
                    } catch {
                        notRemoved = true;
                    }
                }
                foreach(string filesDocuments in Directory.GetFiles(_docPath)) {
                    try {
                        System.IO.File.Delete(filesDocuments);
                    } catch {
                        notRemoved = true;
                    }
                }

                //Remove Directories
                try {
                    Directory.Delete(_programFiles, true);
                } catch { }
                try {
                    Directory.Delete(_docPath, true);
                } catch { }


                if(notRemoved)
                    Console.WriteLine(Properties.strings.wrongUninstall);


                Console.WriteLine(Properties.strings.closeInstance);

                KillTasks();

                if(Directory.Exists(_docPath)) {
                    Console.WriteLine(Properties.strings.cleanUserData);
                    Directory.Delete(_docPath, true);
                }

                if(Directory.Exists(_programFiles)) {
                    Console.WriteLine(Properties.strings.cleanProgramFiles);
                    Directory.Delete(_programFiles, true);
                }


                Console.WriteLine(Properties.strings.byeMSG);

                Console.Write("\n\n" + Properties.strings.continueMSG);
                Console.ReadKey();
            } catch(Exception ex) {
                Console.WriteLine(Properties.strings.error + ex.Message);
            }
        }

        private static void KillTasks() {
            try {
                List<Process> processes = new List<Process>(Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper")));
                foreach(Process p in processes) {
                    if(p.ProcessName != Process.GetCurrentProcess().ProcessName)
                        p.Kill();
                }
            } catch { }
        }
    }
}
