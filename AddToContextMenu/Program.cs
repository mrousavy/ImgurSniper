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

                Console.Write(Properties.strings.addContextMenu);
            } catch {
                Console.Write("\n\n" + Properties.strings.errorContext);
                Console.ReadKey();
            }
        }
    }
}
