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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Toast;

namespace ImgurSniper.UI {
    public class InstallerHelper {
        private static string _path;
        private static string _docPath;
        private static string _downloads;
        private static string _updateZipPath;
        private readonly Toasty _error;
        private readonly MainWindow _invoker;
        private readonly Toasty _success;

        public InstallerHelper(string path, string docPath, Toasty errorToast, Toasty successToast, MainWindow invoker) {
            _path = path;
            _docPath = docPath;
            try {
                SHGetKnownFolderPath(KnownFolder.Downloads, 0, IntPtr.Zero, out _downloads);
            } catch {
                // ignored
            }

            _invoker = invoker;
            _error = errorToast;
            _success = successToast;

            Clean();
        }

        public void Clean() {
            try {
                _updateZipPath = Directory.Exists(_downloads) ? _downloads : _docPath;
                string file = Path.Combine(_updateZipPath, "ImgurSniperSetup.zip");
                string directory = Path.Combine(_updateZipPath, "ImgurSniperInstaller");

                if (File.Exists(file)) {
                    File.Delete(file);
                }
                if (Directory.Exists(directory)) {
                    Directory.Delete(directory, true);
                }
            } catch { }
        }

        public void Update(StackPanel panel) {
            Download(panel);
        }

        public void AddToContextMenu() {
            string addPath = Path.Combine(_path, "AddToContextMenu.exe");

            if (!File.Exists(addPath)) {
                return;
            }

            Process add = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = addPath
                }
            };
            add.Start();
            add.WaitForExit();
        }

        /// <summary>
        ///     Download the ImgurSniper Archive from github
        /// </summary>
        /// <param name="panel">The Panel for the Progressbar</param>
        private void Download(Panel panel) {
            _updateZipPath = Directory.Exists(_downloads) ? _downloads : _docPath;
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
                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(_path, "ImgurSniper.exe");
                p.StartInfo.Arguments = "-autostart";
                p.Start();
            }
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e) {
            string file = Path.Combine(_updateZipPath, "ImgurSniperSetup.zip");
            string extractTo = Path.Combine(_updateZipPath, "ImgurSniperInstaller");
            string msiPath = Path.Combine(extractTo, "ImgurSniperSetup.msi");

            if (!File.Exists(file)) {
                _error.Show(strings.couldNotDownload,
                    TimeSpan.FromSeconds(5));
                Process.Start("https://mrousavy.github.io/ImgurSniper");
                _invoker.ChangeButtonState(true);
            } else {
                Extract(file, extractTo);

                FileVersionInfo versionInfoMSI = null;
                FileVersionInfo versionInfoThis = null;
                try {
                    versionInfoMSI = FileVersionInfo.GetVersionInfo(msiPath);
                    versionInfoThis = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                } catch { }

                if (versionInfoMSI != null && versionInfoThis != null &&
                    versionInfoMSI.ProductVersion != versionInfoThis.ProductVersion) {
                    Process.Start(msiPath);
                }

                KillImgurSniper(true);
            }
        }

        /// <summary>
        ///     Extract the downloaded ImgurSniper Messenger Archive
        /// </summary>
        /// <param name="file">The path of the Archive</param>
        /// <param name="path">The path of the Folder</param>
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
                string path = Path.Combine(_path, "ImgurSniper.exe -autostart");

                using (
                    RegistryKey baseKey =
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")) {
                    if (boxIsChecked == true) {
                        baseKey.SetValue("ImgurSniper", path);
                    } else {
                        baseKey.DeleteValue("ImgurSniper");
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
