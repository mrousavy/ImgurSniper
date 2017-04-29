using ImgurSniper.UI.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Toast;

namespace ImgurSniper.UI {
    public class InstallerHelper {
        private static string _downloads;
        private static string _updateZipPath;
        private readonly Toasty _error;
        private readonly MainWindow _invoker;

        public InstallerHelper(Toasty errorToast, MainWindow invoker) {
            try {
                SHGetKnownFolderPath(KnownFolder.Downloads, 0, IntPtr.Zero, out _downloads);
            } catch {
                // ignored
            }

            _invoker = invoker;
            _error = errorToast;

            Clean();
        }

        public void Clean() {
            try {
                _updateZipPath = Directory.Exists(_downloads) ? _downloads : ConfigHelper.ConfigPath;
                string file = Path.Combine(_updateZipPath, "ImgurSniperSetup.zip");
                string directory = Path.Combine(_updateZipPath, "ImgurSniperInstaller");

                if (File.Exists(file)) {
                    File.Delete(file);
                }
                if (Directory.Exists(directory)) {
                    Directory.Delete(directory, true);
                }
            } catch {
                // ignored
            }
        }

        public void Update(StackPanel panel) {
            Download(panel);
        }

        //Download the ImgurSniper Archive from github
        private void Download(Panel panel) {
            _updateZipPath = Directory.Exists(_downloads) ? _downloads : ConfigHelper.ConfigPath;
            string file = Path.Combine(_updateZipPath, "ImgurSniperSetup.zip");

            if (File.Exists(file)) {
                File.Delete(file);
            }

            using (WebClient client = new WebClient()) {
                client.DownloadFileCompleted += DownloadCompleted;

                client.DownloadProgressChanged += (o, e) => {
                    if (panel != null) {
                        ((ProgressBar)panel.Children[1]).Value = e.ProgressPercentage;
                    }
                };

                client.DownloadFileAsync(
                    new Uri(
                        @"https://github.com/mrousavy/ImgurSniper/blob/master/Downloads/ImgurSniperSetup.zip?raw=true"),
                    file);
            }
        }

        public static void KillImgurSniper(bool killSelf) {
            List<Process> processes =
                new List<Process>(Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper")));

            foreach (Process p in processes) {
                try {
                    if (p.Id != Process.GetCurrentProcess().Id) {
                        p.Kill();
                        p.WaitForExit();
                    }
                } catch {
                    // ignored
                }
            }

            if (killSelf) {
                Application.Current.Shutdown(0);
                Process.GetCurrentProcess().Kill();
            }
        }

        public static void StartImgurSniper() {
            List<Process> processes =
                new List<Process>(Process.GetProcesses().Where(p =>
                    (p.ProcessName.Contains("ImgurSniper")
                    && p.Id != Process.GetCurrentProcess().Id)));

            if (processes.Count < 1) {
                Process p = new Process {
                    StartInfo = {
                        FileName = Path.Combine(ConfigHelper.InstallDir, "ImgurSniper.exe"),
                        Arguments = "-autostart"
                    }
                };
                p.Start();
            }
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e) {
            string file = Path.Combine(_updateZipPath, "ImgurSniperSetup.zip");
            string extractTo = Path.Combine(_updateZipPath, "ImgurSniperInstaller");
            string msiPath = Path.Combine(extractTo, "ImgurSniperSetup.exe");

            if (!File.Exists(file)) {
                _error.Show(strings.couldNotDownload,
                    TimeSpan.FromSeconds(5));
                Process.Start("https://mrousavy.github.io/ImgurSniper");
                _invoker.ChangeButtonState(true);
            } else {
                Extract(file, extractTo);
                
                Process.Start(msiPath);

                KillImgurSniper(true);
            }
        }

        //Extract the downloaded ImgurSniper Messenger Archive
        private static void Extract(string file, string path) {
            using (ZipArchive archive = new ZipArchive(new FileStream(file, FileMode.Open))) {
                if (Directory.Exists(path)) {
                    Directory.Delete(path, true);
                }

                archive.ExtractToDirectory(path);
            }
        }

        public void Autostart(bool? boxIsChecked) {
            try {
                string path = Path.Combine(ConfigHelper.ConfigPath, "ImgurSniper.exe -autostart");

                using (RegistryKey baseKey =
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")) {
                    if (baseKey != null) {
                        if (boxIsChecked == true) {
                            baseKey.SetValue("ImgurSniper", path);
                        } else {
                            baseKey.DeleteValue("ImgurSniper");
                        }
                    }
                }
            } catch {
                //Not authorized
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
            IntPtr hToken, out string pszPath);

        public static class KnownFolder {
            public static readonly Guid Downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        }
    }
}
