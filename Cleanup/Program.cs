using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Cleanup {

    class Program {
        public static string _path {
            get {
                string Documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(Documents, "ImgurSniper");
            }
        }
        public static string _programFiles {
            get {
                string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                return Path.Combine(ProgramFiles, "ImgurSniper");
            }
        }

        static void Main(string[] args) {
            Console.WriteLine("Waiting for all ImgurSniper instances to exit...");

            while(Process.GetProcesses().Where(process =>
            process.ProcessName.Contains("ImgurSniper")).Count() > 0)
                Thread.Sleep(100);

            if(Directory.Exists(_path)) {
                Console.WriteLine("Cleaning up User Data...");
                Directory.Delete(_path, true);
            }

            if(Directory.Exists(_programFiles)) {
                Console.WriteLine("Removing Program Files...");
                Directory.Delete(_programFiles, true);
            }
        }
    }
}
