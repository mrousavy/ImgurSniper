using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Toast;

namespace ImgurSniper.UI {
    public class InstallerHelper {

        private string _path;
        private Toasty _error, _success;
        private MainWindow invoker;

        private string _docPath {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
                if(!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                return value;
            }
        }

        public InstallerHelper(string path, Toasty errorToast, Toasty successToast, MainWindow invoker) {
            _path = path;
            if(!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            this.invoker = invoker;
            _error = errorToast;
            _success = successToast;
        }

        public void Install(object senderButton) {
            RemoveOldFiles();
            Download(senderButton);
        }

        public void AddToStartmenu(object sender) {
            CreateStartMenu(sender, null);
            invoker.ChangeButtonState(true);
        }

        public void AddToDesktop(object sender) {
            CreateDesktop(sender, null);
            invoker.ChangeButtonState(true);
        }


        /// <summary>
        /// Download the Kern Messenger Archive from github
        /// </summary>
        /// <param name="path">The path to save the zip to</param>
        private void Download(object sender) {
            string file = Path.Combine(_path, "ImgurSniperArchive.zip");
            using(WebClient client = new WebClient()) {
                client.DownloadFileCompleted += DownloadCompleted;

                client.DownloadFileCompleted += delegate {
                    (sender as Button).Content = "Done!";
                    (sender as Button).IsEnabled = false;
                    (sender as Button).Tag = new object();
                    _success.Show("Successfully Installed ImgurSniper!", TimeSpan.FromSeconds(3));
                };

                _success.Show("Downloading from github.com/mrousavy/ImgurSniper...", TimeSpan.FromSeconds(2));

                client.DownloadFileAsync(new Uri(@"https://github.com/mrousavy/ImgurSniper/blob/master/ImgurSniper/bin/Release/ImgurSniper.zip?raw=true"),
                    file);
            }
        }

        private void DownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
            string file = Path.Combine(_path, "ImgurSniperArchive.zip");

            if(!System.IO.File.Exists(file)) {
                _error.Show("Could not download ZIP Archive from github.com!",
                    TimeSpan.FromSeconds(5));
            } else {
                Extract(file, _path);
                Finalize(Path.Combine(_path, "ImgurSniper.exe"));
            }
        }


        /// <summary>
        /// Remove all old Kern Files
        /// </summary>
        private void RemoveOldFiles() {
            foreach(string file in Directory.GetFiles(_path)) {
                try {
                    System.IO.File.Delete(file);
                } catch(Exception) { }
            }
        }

        /// <summary>
        /// Extract the downloaded Kern Messenger Archive
        /// </summary>
        /// <param name="file">The path of the Archive</param>
        /// <param name="path">The path of the Folder</param>
        private void Extract(string file, string path) {
            using(ZipArchive archive = new ZipArchive(new FileStream(file, FileMode.Open))) {
                archive.ExtractToDirectory(path);
            }
        }

        public async void Uninstall() {
            _success.Show("Removing ImgurSniper and Cleaning up junk...",
                TimeSpan.FromSeconds(2.5));

            //Remove Startmenu Shortcut
            try {
                string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
                string shortcutLocation = Path.Combine(commonStartMenuPath, "ImgurSniper" + ".lnk");
                System.IO.File.Delete(shortcutLocation);
            } catch(Exception) { }

            //Remove Desktop Shortcut
            try {
                object shDesktop = (object)"Desktop";
                WshShell shell = new WshShell();
                string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Imgur Sniper.lnk";
                System.IO.File.Delete(shortcutAddress);
            } catch(Exception) { }


            try {
                //Kill open instances if any
                foreach(Process p in Process.GetProcessesByName("ImgurSniper")) {
                    p.Kill();
                }

                await Task.Delay(2495);

                //Remove all files
                Array.ForEach(Directory.GetFiles(_docPath), System.IO.File.Delete);
                Array.ForEach(Directory.GetFiles(_path), System.IO.File.Delete);

                //Remove Directories
                Directory.Delete(_path);
                Directory.Delete(_docPath);


                _success.Show("Sad to see you go! Bye :(",
                    TimeSpan.FromSeconds(1.5));

                await Task.Delay(1500);

                invoker.ChangeButtonState(true);
                invoker.Close();
            } catch(Exception ex) {
                _error.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Rename Exe, Start Process and Finish Installer
        /// </summary>
        /// <param name="file">The exe path</param>
        private void Finalize(string file) {
            System.IO.File.Delete(Path.Combine(_path, "ImgurSniperArchive.zip"));
            CreateUninstaller();

            invoker.ChangeButtonState(true);
        }


        private void CreateDesktop(object sender, RoutedEventArgs e) {
            (sender as System.Windows.Controls.Button).Content = "Done!";
            (sender as System.Windows.Controls.Button).IsEnabled = false;
            (sender as System.Windows.Controls.Button).Tag = new object();

            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Imgur Sniper.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Instant Snipe an Image!";
            shortcut.WorkingDirectory = _path;
            shortcut.TargetPath = Path.Combine(_path, "ImgurSniper.exe");
            shortcut.Save();
        }

        private void CreateStartMenu(object sender, RoutedEventArgs e) {
            (sender as System.Windows.Controls.Button).Content = "Done!";
            (sender as System.Windows.Controls.Button).IsEnabled = false;
            (sender as System.Windows.Controls.Button).Tag = new object();

            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

            string shortcutLocation = Path.Combine(commonStartMenuPath, "ImgurSniper" + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "Instant Snipe an Image!";
            shortcut.WorkingDirectory = _path;
            shortcut.TargetPath = Path.Combine(_path, "ImgurSniper.exe");
            shortcut.Save();
        }

        private void CreateUninstaller() {
            using(RegistryKey parent = Registry.LocalMachine.OpenSubKey(
                         @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true)) {
                if(parent == null) {
                    return;
                }
                try {
                    RegistryKey key = null;

                    try {
                        string appName = "ImgurSniper";

                        key = parent.CreateSubKey(appName);

                        Assembly asm = GetType().Assembly;
                        Version v = asm.GetName().Version;
                        string exe = "\"" + asm.CodeBase.Substring(8).Replace("/", "\\\\") + "\"";

                        key.SetValue("DisplayName", "ImgurSniper");
                        key.SetValue("ApplicationVersion", v.ToString());
                        key.SetValue("Publisher", "mrousavy");
                        key.SetValue("DisplayIcon", exe);
                        key.SetValue("DisplayVersion", v.ToString(2));
                        key.SetValue("URLInfoAbout", "http://www.github.com/mrousavy/ImgurSniper");
                        key.SetValue("Contact", "");
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", exe + " /uninstallprompt");
                    } finally {
                        if(key != null) {
                            key.Close();
                        }
                    }
                } catch(Exception) {
                    _error.Show("Could not create Uninstaller for ImgurSniper! You will have to remove the Files manually (from " + _path + ")",
                        TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}
