using System;
using System.IO;
using AddToContextMenu.Properties;
using Microsoft.Win32;

namespace AddToContextMenu {
    internal class Program {
        private static void Main(string[] args) {
            try {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(dir, "ImgurSniper.exe");

                Console.WriteLine(path);

                using (RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"*\shell\ImgurSniperUpload")) {
                    baseKey.SetValue("Icon", path);
                    baseKey.SetValue(string.Empty, "Upload to Imgur");
                    using (RegistryKey key = baseKey.CreateSubKey("command")) {
                        //TODO: Handle multiple Paths (instead of %1, do %* | not yet implemented in ImgurSniper)
                        key.SetValue(string.Empty, "\"" + path + "\" upload \"%1\"");
                    }
                }

                Console.Write(strings.addContextMenu);
            }
            catch {
                Console.Write(strings.errorContext);
                Console.ReadKey();
            }
        }
    }
}