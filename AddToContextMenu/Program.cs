using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;

namespace AddToContextMenu {
    class Program {
        static void Main(string[] args) {
            string lang = Thread.CurrentThread.CurrentCulture.EnglishName;
            
            try {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(dir, "ImgurSniper.exe");

                Console.WriteLine(path);

                using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"*\shell\ImgurSniperUpload")) {
                    baseKey.SetValue("Icon", path);
                    baseKey.SetValue(string.Empty, "Upload Image to Imgur");
                    using(RegistryKey key = baseKey.CreateSubKey("command")) {
                        key.SetValue(string.Empty, "\"" + path + "\" upload \"%1\"");
                    }
                }

                if(lang.Contains("German"))
                    Console.Write(Properties.german.addContextMenu);
                else
                    Console.Write(Properties.english.addContextMenu);
            } catch {
                if(lang.Contains("German"))
                    Console.Write(Properties.german.errorContext);
                else
                    Console.Write(Properties.english.errorContext);

                Console.ReadKey();
            }
        }
    }
}
