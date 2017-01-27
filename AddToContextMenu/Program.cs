using Microsoft.Win32;
using System;
using System.IO;

namespace AddToContextMenu {
    class Program {
        static void Main(string[] args) {
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

                Console.Write("Successfully added ImgurSniper to Context Menu!");
            } catch(Exception) {
                Console.Write("\n\nError: Could not add to Context Menu");
                Console.ReadKey();
            }
        }
    }
}
