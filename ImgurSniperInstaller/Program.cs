using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace ImgurSniperInstaller {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Extracting...\n\n");

            bool error = false;

            string ProgramFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ImgurSniper");

            if(!Directory.Exists(ProgramFiles))
                Directory.CreateDirectory(ProgramFiles);

            try {
                foreach(string file in Directory.GetFiles(ProgramFiles)) {
                    Console.WriteLine("Removing " + file + "...");

                    //Delete every File except original ImgurSniper.exe
                    if(!file.EndsWith("ImgurSniper.exe"))
                        File.Delete(file);
                }
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
                error = true;
            }

            try {
                Extract(Properties.Resources.ImgurSniper_UI, ProgramFiles);
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
                error = true;
            }

            try {
                Process.Start(Path.Combine(ProgramFiles, "ImgurSniper.UI.exe"));
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
                error = true;
            }

            if(error) {
                Console.WriteLine("An unknown Error occured while trying to extract." +
                    "\nPlease ensure that there is no instance of ImgurSniper currently open, " +
                    "and that there are no problems regarding this Path: \"" + ProgramFiles + "\".");
                Console.ReadKey();
            }
        }


        /// <summary>
        /// Extract the bytes
        /// </summary>
        /// <param name="file">The path of the Archive</param>
        /// <param name="path">The path of the Folder</param>
        private static void Extract(byte[] bytes, string path) {
            using(ZipArchive archive = new ZipArchive(new MemoryStream(bytes))) {
                archive.ExtractToDirectory(path);
            }
        }
    }
}
