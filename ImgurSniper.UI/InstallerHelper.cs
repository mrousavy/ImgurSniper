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
        private string _tempPath;
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
            _tempPath = Path.Combine(_path, "TempDownload");

            if(!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            try {
                if(Directory.Exists(_tempPath)) {
                    Directory.Delete(_tempPath, true);
                }
            } catch(Exception) { }
            Directory.CreateDirectory(_tempPath);

            if(System.IO.File.Exists(Path.Combine(_path, "ImgurSniperArchive.zip"))) {
                try {
                    System.IO.File.Delete(Path.Combine(_path, "ImgurSniperArchive.zip"));
                } catch(Exception) { }
            }

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

        public void AddToContextMenu(object sender) {
            CreateContextMenu(sender, null);
            invoker.ChangeButtonState(true);
        }


        /// <summary>
        /// Download the ImgurSniper Archive from github
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

                client.DownloadFileAsync(new Uri(@"https://github.com/mrousavy/ImgurSniper/blob/master/Downloads/ImgurSniper.zip?raw=true"),
                    file);
            }
        }

        private void DownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
            string file = Path.Combine(_path, "ImgurSniperArchive.zip");

            if(!System.IO.File.Exists(file)) {
                _error.Show("Could not download ZIP Archive from github.com!",
                    TimeSpan.FromSeconds(5));
            } else {
                try {
                    foreach(string tempFile in Directory.GetFiles(_tempPath)) {
                        System.IO.File.Delete(tempFile);
                    }
                } catch(Exception) { }
                try {
                    Directory.Delete(_tempPath, true);
                } catch(Exception) { }
                Directory.CreateDirectory(_tempPath);

                Extract(file, _tempPath);
                Move(_tempPath, _path);
                Finalize(Path.Combine(_path, "ImgurSniper.exe"));
            }
        }


        /// <summary>
        /// Remove all old ImgurSniper Files
        /// </summary>
        private void RemoveOldFiles() {
            foreach(string file in Directory.GetFiles(_path)) {
                try {
                    System.IO.File.Delete(file);
                } catch(Exception) { }
            }

            foreach(string directory in Directory.GetDirectories(_path)) {
                try {
                    if(!directory.Contains("Temp"))
                        Directory.Delete(directory, true);
                } catch(Exception) { }
            }
        }

        /// <summary>
        /// Extract the downloaded ImgurSniper Messenger Archive
        /// </summary>
        /// <param name="file">The path of the Archive</param>
        /// <param name="path">The path of the Folder</param>
        private void Extract(string file, string path) {
            using(ZipArchive archive = new ZipArchive(new FileStream(file, FileMode.Open))) {
                archive.ExtractToDirectory(path);
            }
        }

        private void Move(string from, string to) {
            try {
                DirectoryInfo info = new DirectoryInfo(from);
                FileInfo[] infos = info.GetFiles();

                foreach(FileInfo file in infos) {
                    string filePath = Path.Combine(to, file.Name);

                    if(!System.IO.File.Exists(filePath) || file.Extension == ".exe") {
                        try {
                            System.IO.File.Move(file.FullName, filePath);
                        } catch(Exception) { }
                    }
                }

                DirectoryInfo[] dirInfos = info.GetDirectories();
                foreach(DirectoryInfo dirInfo in dirInfos) {
                    string dirPath = Path.Combine(to, dirInfo.Name);

                    if(!Directory.Exists(dirPath)) {
                        try {
                            Directory.Move(dirInfo.FullName, Path.Combine(to, dirPath));
                        } catch(Exception) { }
                    }
                }
            } catch(Exception) {
                _error.Show("Error moving Files, please close all ImgurSniper instances and try again!", TimeSpan.FromSeconds(2));
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
                using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"jpegfile\shell")) {
                    baseKey.DeleteSubKeyTree("Upload Image to Imgur");
                }
                using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"pngfile\shell")) {
                    baseKey.DeleteSubKeyTree("Upload Image to Imgur");
                }
                using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"giffile\shell")) {
                    baseKey.DeleteSubKeyTree("Upload Image to Imgur");
                }
            } catch(Exception) { }


            try {
                //Kill open instances if any
                foreach(Process p in Process.GetProcessesByName("ImgurSniper")) {
                    p.Kill();
                }

                await Task.Delay(2500);

                //Remove all files
                bool notRemoved = false;

                foreach(string filesPrograms in Directory.GetFiles(_path)) {
                    try {
                        System.IO.File.Delete(filesPrograms);
                    } catch(Exception) {
                        notRemoved = true;
                    }
                }
                foreach(string filesDocuments in Directory.GetFiles(_docPath)) {
                    try {
                        System.IO.File.Delete(filesDocuments);
                    } catch(Exception) {
                        notRemoved = true;
                    }
                }

                //Remove Directories
                try {
                    Directory.Delete(_path, true);
                } catch(Exception) { }
                try {
                    Directory.Delete(_docPath, true);
                } catch(Exception) { }


                if(notRemoved)
                    _error.Show("Some Files were not successfully removed!",
                        TimeSpan.FromSeconds(1));

                _success.Show("Sad to see you go! Bye :(",
                        TimeSpan.FromSeconds(1.5));

                string path = Path.Combine(Path.GetTempPath(), "Cleanup.exe");
                System.IO.File.WriteAllBytes(path, Properties.Resources.Cleanup);
                Process.Start(path);

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
            try {
                System.IO.File.Delete(Path.Combine(_path, "ImgurSniperArchive.zip"));

                Directory.Delete(_tempPath, true);
                CreateUninstaller();
            } catch(Exception) { }

            invoker.ChangeButtonState(true);
        }


        private void CreateDesktop(object sender, RoutedEventArgs e) {
            if(!System.IO.File.Exists(Path.Combine(_path, "ImgurSniper.exe"))) {
                _error.Show("Error, ImgurSniper could not be found on this Machine!", TimeSpan.FromSeconds(2));
                return;
            }
            if(!System.IO.File.Exists(Path.Combine(_path, "ImgurSniper.UI.exe"))) {
                _error.Show("Error, ImgurSniper.UI could not be found on this Machine!", TimeSpan.FromSeconds(2));
                return;
            }

            object shDesktop = "Desktop";
            WshShell shell = new WshShell();

            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Imgur Sniper.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Instant Snipe an Image!";
            shortcut.WorkingDirectory = _path;
            shortcut.TargetPath = Path.Combine(_path, "ImgurSniper.exe");
            shortcut.Save();


            string shortcutAddressUI = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Imgur Sniper UI.lnk";
            IWshShortcut shortcutUI = (IWshShortcut)shell.CreateShortcut(shortcutAddressUI);
            shortcutUI.Description = "ImgurSniper UI Control Panel";
            shortcutUI.WorkingDirectory = _path;
            shortcutUI.TargetPath = Path.Combine(_path, "ImgurSniper.UI.exe");
            shortcutUI.Save();


            (sender as System.Windows.Controls.Button).Content = "Done!";
            (sender as System.Windows.Controls.Button).IsEnabled = false;
            (sender as System.Windows.Controls.Button).Tag = new object();
        }

        private void CreateStartMenu(object sender, RoutedEventArgs e) {
            if(!System.IO.File.Exists(Path.Combine(_path, "ImgurSniper.exe"))) {
                _error.Show("Error, ImgurSniper could not be found on this Machine!", TimeSpan.FromSeconds(2));
                return;
            }
            if(!System.IO.File.Exists(Path.Combine(_path, "ImgurSniper.UI.exe"))) {
                _error.Show("Error, ImgurSniper.UI could not be found on this Machine!", TimeSpan.FromSeconds(2));
                return;
            }

            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

            string shortcutLocation = Path.Combine(commonStartMenuPath, "ImgurSniper" + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "Instant Snipe an Image!";
            shortcut.WorkingDirectory = _path;
            shortcut.TargetPath = Path.Combine(_path, "ImgurSniper.exe");
            shortcut.Save();


            string shortcutLocationUI = Path.Combine(commonStartMenuPath, "ImgurSniper.UI" + ".lnk");
            WshShell shellUI = new WshShell();
            IWshShortcut shortcutUI = (IWshShortcut)shell.CreateShortcut(shortcutLocationUI);

            shortcutUI.Description = "ImgurSniper UI Control Panel";
            shortcutUI.WorkingDirectory = _path;
            shortcutUI.TargetPath = Path.Combine(_path, "ImgurSniper.UI.exe");
            shortcutUI.Save();

            (sender as System.Windows.Controls.Button).Content = "Done!";
            (sender as System.Windows.Controls.Button).IsEnabled = false;
            (sender as System.Windows.Controls.Button).Tag = new object();
        }
        private void CreateContextMenu(object sender, RoutedEventArgs e) {
            string path = Path.Combine(_path, "ImgurSniper.exe");
            if(!System.IO.File.Exists(path)) {
                _error.Show("Error, ImgurSniper could not be found on this Machine!", TimeSpan.FromSeconds(2));
                return;
            }

            using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"jpegfile\shell\Upload Image to Imgur")) {
                baseKey.SetValue("Icon", path);
                using(RegistryKey key = baseKey.CreateSubKey("command")) {
                    key.SetValue(string.Empty, "\"" + path + "\" upload \"%1\"");
                }
            }
            using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"pngfile\shell\Upload Image to Imgur")) {
                baseKey.SetValue("Icon", path);
                using(RegistryKey key = baseKey.CreateSubKey("command")) {
                    key.SetValue(string.Empty, "\"" + path + "\" upload \"%1\"");
                }
            }
            using(RegistryKey baseKey = Registry.ClassesRoot.CreateSubKey(@"giffile\shell\Upload Image to Imgur")) {
                baseKey.SetValue("Icon", path);
                using(RegistryKey key = baseKey.CreateSubKey("command")) {
                    key.SetValue(string.Empty, "\"" + path + "\" upload \"%1\"");
                }
            }

            (sender as System.Windows.Controls.Button).Content = "Done!";
            (sender as System.Windows.Controls.Button).IsEnabled = false;
            (sender as System.Windows.Controls.Button).Tag = new object();
        }

        private void CreateUninstaller() {
            try {
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
                            key.SetValue("UninstallString", exe + " /uninstall");
                        } finally {
                            key?.Close();
                        }
                    } catch(Exception) {
                        _error.Show("Could not create Uninstaller for ImgurSniper! You will have to remove the Files manually (from " + _path + ")",
                            TimeSpan.FromSeconds(5));
                    }
                }
            } catch(Exception) { }
        }
    }
}
