using ImgurSniper.UI.Properties;
using Microsoft.Win32;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Toast;
using Application = System.Windows.Application;
using FileMode = System.IO.FileMode;

namespace ImgurSniper.UI {
    public class InstallerHelper {
        private static string _updateZipPath;
        private readonly Toasty _error;
        private static string Downloads {
            get {
                string ret;
                try {
                    SHGetKnownFolderPath(KnownFolder.DownloadsGuid, 0, IntPtr.Zero, out ret);
                } catch {
                    ret = ConfigHelper.ConfigPath;
                }

                return ret;
            }
        }

        public static int TotalCommits = ConfigHelper.TotalCommits;
        public static List<GitHubCommit> Commits { get; set; }

        public InstallerHelper(Toasty errorToast) {
            _error = errorToast;
            _updateZipPath = Directory.Exists(Downloads) ? Downloads : ConfigHelper.ConfigPath;

            Clean();
        }

        public void Clean() {
            try {
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

        //Download the ImgurSniper Archive from github
        public void Update(Panel panel) {
            _updateZipPath = Directory.Exists(Downloads) ? Downloads : ConfigHelper.ConfigPath;
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
                    new Uri(@"https://github.com/mrousavy/ImgurSniper/blob/master/Downloads/ImgurSniperSetup.zip?raw=true"),
                    file);
            }
        }

        public static void KillImgurSniper(bool killSelf) {
            List<Process> processes =
                new List<Process>(Process.GetProcesses().Where(p =>
                (p.ProcessName.Contains("ImgurSniper")) &&
                (!p.ProcessName.Contains("Setup"))));

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

        private async void DownloadCompleted(object sender, AsyncCompletedEventArgs e) {
            string file = Path.Combine(_updateZipPath, "ImgurSniperSetup.zip");
            string extractTo = Path.Combine(_updateZipPath, "ImgurSniperInstaller");
            string installerPath = Path.Combine(extractTo, "ImgurSniperSetup.exe");

            if (!File.Exists(file)) {
                _error.Show(strings.couldNotDownload,
                    TimeSpan.FromSeconds(5));
                await Task.Delay(1000);
                Process.Start("https://mrousavy.github.io/ImgurSniper");
            } else {
                Extract(file, extractTo);

                //Update current version
                ConfigHelper.CurrentCommits = Commits.Count;
                ConfigHelper.UpdateAvailable = false;
                ConfigHelper.Save();

                //Remove /VERYSILENT ?
                Process.Start(installerPath, "/VERYSILENT");

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

        public static void Autostart(bool enableAutostart) {
            try {
                string path = Path.Combine(ConfigHelper.ConfigPath, "ImgurSniper.exe -autostart");

                using (RegistryKey baseKey =
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")) {
                    if (baseKey != null) {
                        if (enableAutostart) {
                            baseKey.SetValue("ImgurSniper", path);
                            StartImgurSniper();
                        } else {
                            baseKey.DeleteValue("ImgurSniper");
                        }
                    }
                }
            } catch {
                //Not authorized
            }
        }

        //forceSearch = true if should search for updates even if Last Checked is not longer than 1 Day ago
        public static async Task<bool> CheckForUpdates(MainWindow window, bool forceSearch) {
            try {
                //Last update Check
                DateTime lastChecked = ConfigHelper.LastChecked;

                //Update Available?
                bool updateAvailable = ConfigHelper.UpdateAvailable;

                //Last Update Content for Label
                window.SetProgressStatus(string.Format(strings.updateLast, $"{lastChecked:dd.MM.yyyy HH:mm}"));

                //If AutoUpdate is disabled and the User does not manually search, exit Method
                if (!ConfigHelper.AutoUpdate && !forceSearch) {
                    return false;
                }

                //Update Loading Indicator
                window.SetProgressStatus(strings.checkingUpdate);

                //Check for Update, if last update is longer than 1 Day ago
                if (forceSearch || DateTime.Now - lastChecked > TimeSpan.FromDays(1) || updateAvailable) {
                    //Retrieve info from github
                    GitHubClient github = new GitHubClient(new ProductHeaderValue("ImgurSniper"));
                    IReadOnlyList<GitHubCommit> commitsRaw = await github.Repository.Commit.GetAll("mrousavy",
                        "ImgurSniper");
                    //All Commits where a new ImgurSniper Version is available start with "R:"
                    Commits = new List<GitHubCommit>(commitsRaw.Where(c => c.Commit.Message.StartsWith("R:")));
                    TotalCommits = commitsRaw.Count;
                    ConfigHelper.TotalCommits = TotalCommits;
                    ConfigHelper.LastChecked = DateTime.Now;
                    ConfigHelper.Save();

                    //Last Update Content for Label
                    window.SetProgressStatus(string.Format(strings.updateLast, $"{DateTime.Now:dd.MM.yyyy HH:mm}"));

                    int currentCommits = ConfigHelper.CurrentCommits;
                    //999 = value is unset
                    if (currentCommits == 999) {
                        ConfigHelper.CurrentCommits = Commits.Count;
                        ConfigHelper.Save();
                    } else if (updateAvailable || Commits.Count > currentCommits) {
                        //Newer Version is available
                        ConfigHelper.UpdateAvailable = true;
                        ConfigHelper.Save();
                        window.SuccessToast.Show(string.Format(strings.updateAvailable, currentCommits, Commits.Count),
                            TimeSpan.FromSeconds(4));

                        return true;
                    } else {
                        //No Update available
                        ConfigHelper.UpdateAvailable = false;
                        ConfigHelper.Save();
                    }
                }
            } catch {
                window.ErrorToast.Show(strings.failedUpdate, TimeSpan.FromSeconds(3));
            }
            //Any other way than return true = no update
            return false;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
            IntPtr hToken, out string pszPath);

        public static class KnownFolder {
            public static readonly Guid DownloadsGuid = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        }
    }
}
