using ImgurSniper.UI.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Toast;

namespace ImgurSniper.UI {
    public class InstallerHelper {

        private static string _path;
        private static string _docPath;
        private static string _downloads;
        private static string _updateZipPath;
        private readonly Toasty _error;
        private readonly Toasty _success;
        private readonly MainWindow _invoker;

        public InstallerHelper(string path, string docPath, Toasty errorToast, Toasty successToast, MainWindow invoker) {
            _path = path;
            _docPath = docPath;
            try {
                SHGetKnownFolderPath(KnownFolder.Downloads, 0, IntPtr.Zero, out _downloads);
            } catch { }

            _invoker = invoker;
            _error = errorToast;
            _success = successToast;
        }

        public void Update(StackPanel panel) {
            Download(panel);
        }

        public void AddToContextMenu() {
            string addPath = Path.Combine(_path, "AddToContextMenu.exe");

            if(!File.Exists(addPath))
                return;

            Process add = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = addPath
                }
            };
            add.Start();
            add.WaitForExit();
        }

        /// <summary>
        /// Download the ImgurSniper Archive from github
        /// </summary>
        /// <param name="panel">The Panel for the Progressbar</param>
        private void Download(Panel panel) {
            _updateZipPath = Directory.Exists(_downloads) ? _downloads : _docPath;
            string file = Path.Combine(_updateZipPath, "ImgurSniperSetup.zip");

            if(File.Exists(file)) {
                File.Delete(file);
            }

            using(WebClient client = new WebClient()) {
                client.DownloadFileCompleted += DownloadCompleted;

                client.DownloadProgressChanged += (o, e) => {
                    //sender.Content = strings.update + " (" + e.ProgressPercentage + "%)";
                    ((ProgressBar)panel.Children[1]).Value = e.ProgressPercentage;
                };

                _success.Show(strings.downloadingGitHub, TimeSpan.FromSeconds(2));

                client.DownloadFileAsync(new Uri(@"https://github.com/mrousavy/ImgurSniper/blob/master/Downloads/ImgurSniperSetup.zip?raw=true"),
                    file);
            }
        }


        public static void KillImgurSniper(bool killSelf) {
            List<Process> processes =
                new List<Process>(Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper")));
            foreach(Process p in processes) {
                if(p.Id != Process.GetCurrentProcess().Id)
                    p.Kill();
            }

            if(killSelf) {
                System.Windows.Application.Current.Shutdown(0);
                Process.GetCurrentProcess().Kill();
            }
        }
        public static void StartImgurSniper() {
            List<Process> processes =
                new List<Process>(Process.GetProcesses().Where(p =>
                                                               p.ProcessName.Contains("ImgurSniper")
                                                               && p.Id != Process.GetCurrentProcess().Id));

            if(processes.Count == 0) {
                Process.Start(Path.Combine(_path, "ImgurSniper.exe"));
            }
        }

        private void DownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
            string file = Path.Combine(_updateZipPath, "ImgurSniperSetup.zip");
            string extractTo = Path.Combine(_updateZipPath, "ImgurSniperInstaller");

            if(!File.Exists(file)) {
                _error.Show(strings.couldNotDownload,
                    TimeSpan.FromSeconds(5));
                Process.Start("https://mrousavy.github.io/ImgurSniper");
                _invoker.ChangeButtonState(true);
            } else {
                Extract(file, extractTo);
                Process.Start(Path.Combine(extractTo, "ImgurSniperSetup.msi"));

                KillImgurSniper(true);
            }
        }

        /// <summary>
        /// Extract the downloaded ImgurSniper Messenger Archive
        /// </summary>
        /// <param name="file">The path of the Archive</param>
        /// <param name="path">The path of the Folder</param>
        private static void Extract(string file, string path) {
            using(ZipArchive archive = new ZipArchive(new FileStream(file, FileMode.Open))) {
                if(Directory.Exists(path))
                    Directory.Delete(path, true);

                archive.ExtractToDirectory(path);
            }
        }

        //public void Uninstall() {
        //await _success.ShowAsync("Removing ImgurSniper and Cleaning up junk...",
        //    TimeSpan.FromSeconds(2.5));

        //string path = Path.Combine(Path.GetTempPath(), "Cleanup.exe");
        //File.WriteAllBytes(path, Properties.Resources.Cleanup);
        //Process.Start(path);

        //Application.Current.Shutdown();
        //}

        //private void CreateUninstaller() {
        //    try {
        //        using(RegistryKey parent = Registry.LocalMachine.OpenSubKey(
        //                     @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true)) {
        //            if(parent == null) {
        //                return;
        //            }
        //            try {
        //                RegistryKey key = null;

        //                try {
        //                    string appName = "ImgurSniper";

        //                    key = parent.CreateSubKey(appName);

        //                    Assembly asm = GetType().Assembly;
        //                    Version v = asm.GetName().Version;
        //                    string exe = "\"" + asm.CodeBase.Substring(8).Replace("/", "\\\\") + "\"";

        //                    key.SetValue("DisplayName", "ImgurSniper");
        //                    key.SetValue("ApplicationVersion", v.ToString());
        //                    key.SetValue("Publisher", "mrousavy");
        //                    key.SetValue("DisplayIcon", exe);
        //                    key.SetValue("DisplayVersion", v.ToString(2));
        //                    key.SetValue("URLInfoAbout", "http://www.github.com/mrousavy/ImgurSniper");
        //                    key.SetValue("Contact", "");
        //                    key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
        //                    key.SetValue("UninstallString", exe + " /uninstall");
        //                } finally {
        //                    key?.Close();
        //                }
        //            } catch  {
        //                _error.Show("Could not create Uninstaller for ImgurSniper! You will have to remove the Files manually (from " + _docPath + ")",
        //                    TimeSpan.FromSeconds(5));
        //            }
        //        }
        //    } catch  { }
        //}

        public void Autostart(bool? boxIsChecked) {
            try {
                string path = Path.Combine(_path, "ImgurSniper.exe -autostart");

                using(RegistryKey baseKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")) {
                    if(boxIsChecked == true) {
                        baseKey.SetValue("ImgurSniper", path);
                    } else {
                        baseKey.DeleteValue("ImgurSniper");
                    }
                }
            } catch {
                //Not authorized
            }
        }

        public static class KnownFolder {
            public static readonly Guid Downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);
    }
}
