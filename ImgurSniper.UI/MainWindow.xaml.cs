using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using str = ImgurSniper.UI.Properties.strings;

namespace ImgurSniper.UI {
    public partial class MainWindow : Window {
        #region Fields
        public InstallerHelper Helper;
        private readonly ImgurLoginHelper _imgurhelper;

        //Path to Program Files/ImgurSniper Folder
        private static string Path => AppDomain.CurrentDomain.BaseDirectory;
        private IReadOnlyList<GitHubCommit> _commits;

        //Path to Documents/ImgurSniper Folder
        private static string DocPath {
            get {

                string value = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
                return value;
            }
        }
        #endregion

        //Constructor
        public MainWindow() {
            InitializeComponent();

            //Update Loading Indicator
            loadingDesc.Content = str.initializing;

            //Check for Commandline Arguments
            Arguments();

            //Create Documents\ImgurSniper Path
            if(!Directory.Exists(DocPath))
                Directory.CreateDirectory(DocPath);

            //Initialize Helpers
            Helper = new InstallerHelper(Path, DocPath, error_toast, success_toast, this);
            _imgurhelper = new ImgurLoginHelper(error_toast, success_toast);

            //Load Config
            Loaded += delegate { Load(); };
        }

        //Command Line Args
        private async void Arguments() {
            string[] args = Environment.GetCommandLineArgs();
            if(args.Contains("Help")) {
                Help(null, null);
                Close();
            }
            if(args.Contains("Update")) {
                bool updateAvailable = await CheckForUpdates(true);
                if(updateAvailable) {
                    Update(null, null);
                } else {
                    Close();
                }
            }
            if(args.Contains("Troubleshooting")) {
                //Task.Delay for Open/Close Animations
                await Task.Delay(400);
                await ShowOkDialog(str.troubleshooting, str.troubleshootingTips);
                await Task.Delay(400);
                Close();
            }
        }

        //Load all Config Params
        private async void Load() {
            PathBox.Text = DocPath;

            //Is "Upload to Imgur" entry in Windows Explorer Context Menu?
            if(!FileIO.IsInContextMenu) {
                Helper.AddToContextMenu();
                FileIO.IsInContextMenu = true;

                //First time using ImgurSniper? Show Help Window
                Help(null, null);
            }

            //Update Loading Indicator
            loadingDesc.Content = str.loadConf;

            #region Read Config
            try {
                //Only 1x FileIO File read, optimized performance
                FileIO.Settings settings = FileIO.JsonConfig;

                string SaveImagesPath = settings.SaveImagesPath;
                bool UsePNG = settings.UsePNG;
                bool AllMonitors = settings.AllMonitors;
                bool OpenAfterUpload = settings.OpenAfterUpload;
                bool UsePrint = settings.UsePrint;
                bool RunOnBoot = settings.RunOnBoot;
                //bool Magnifyer = settings.MagnifyingGlassEnabled;
                bool SaveImages = settings.SaveImages;
                bool ImgurAfterSnipe = settings.ImgurAfterSnipe;
                bool AutoUpdate = settings.AutoUpdate;
                string language = settings.Language;
                System.Windows.Input.Key key = settings.ShortcutKey;

                //Path to Saved Images
                if(string.IsNullOrWhiteSpace(SaveImagesPath)) {
                    PathBox.Text = DocPath;
                } else {
                    //Create Pictures\ImgurSniperImages Path
                    try {
                        if(!Directory.Exists(SaveImagesPath)) {
                            Directory.CreateDirectory(SaveImagesPath);
                        }
                        PathBox.Text = SaveImagesPath;
                    } catch {
                        PathBox.Text = "";
                    }
                }

                //PNG or JPEG
                if(UsePNG)
                    PngRadio.IsChecked = true;
                else
                    JpegRadio.IsChecked = true;

                //Current or All Monitors
                if(AllMonitors)
                    MultiMonitorRadio.IsChecked = true;
                else
                    CurrentMonitorRadio.IsChecked = true;

                //Open Image in Browser after upload
                OpenAfterUploadBox.IsChecked = OpenAfterUpload;

                //Use Print Key instead of default Shortcut
                PrintKeyBox.IsChecked = UsePrint;

                //Run ImgurSniper on boot
                if(RunOnBoot) {
                    this.RunOnBoot.IsChecked = true;
                    Helper.Autostart(true);
                }

                //Auto search for Updates
                if(AutoUpdate) {
                    AutoUpdateBox.IsChecked = true;
                }

                //Enable or Disable Magnifying Glass (WIP)
                //MagnifyingGlassBox.IsChecked = Magnifyer;

                //Save Images on Snap
                SaveBox.IsChecked = SaveImages;

                //Upload to Imgur or Copy to Clipboard after Snipe
                if(ImgurAfterSnipe)
                    ImgurRadio.IsChecked = true;
                else
                    ClipboardRadio.IsChecked = true;

                //Set correct Language for Current Language Box
                switch(language) {
                    case "en":
                        LanguageBox.SelectedItem = en;
                        break;
                    case "de":
                        LanguageBox.SelectedItem = de;
                        break;
                }
                LanguageBox.SelectionChanged += LanguageBox_SelectionChanged;

                HotkeyBox.Text = key.ToString();

            } catch { }
            #endregion

            //Run proecess if not running
            try {
                if(RunOnBoot.IsChecked == true) {
                    if(!Process.GetProcessesByName("ImgurSniper").Any()) {
                        Process start = new Process {
                            StartInfo = {
                                FileName = Path + "\\ImgurSniper.exe",
                                Arguments = " -autostart"
                            }
                        };
                        start.Start();
                    }
                }
            } catch {
                error_toast.Show(str.trayServiceNotRunning, TimeSpan.FromSeconds(2));
            }


            //Update Loading Indicator
            loadingDesc.Content = str.contactImgur;

            string refreshToken = FileIO.ReadRefreshToken();
            //name = null if refreshToken = null or any error occured in Login
            string name = await _imgurhelper.LoggedInUser(refreshToken);

            if(name != null) {
                Label_Account.Content = string.Format(str.imgurAccSignedIn, name);

                Btn_SignIn.Visibility = Visibility.Collapsed;
                Btn_ViewPics.Visibility = Visibility.Visible;
                Btn_SignOut.Visibility = Visibility.Visible;
            }

            if(SaveBox.IsChecked.HasValue) {
                PathPanel.IsEnabled = (bool)SaveBox.IsChecked;
            }

#if DEBUG
            Btn_Update.IsEnabled = true;
#else
            await CheckForUpdates(false);
#endif

            //Remove Loading Indicator
            progressIndicator.BeginAnimation(OpacityProperty, Animations.FadeOut);
        }

        //Enable or disable all Buttons
        public void ChangeButtonState(bool enabled) {
            if(Btn_PinOk.Tag == null)
                Btn_PinOk.IsEnabled = enabled;

            if(Btn_SignIn.Tag == null)
                Btn_SignIn.IsEnabled = enabled;

            if(Btn_SignOut.Tag == null)
                Btn_SignOut.IsEnabled = enabled;

            if(Btn_ViewPics.Tag == null)
                Btn_ViewPics.IsEnabled = enabled;

            if(Btn_Snipe.Tag == null)
                Btn_Snipe.IsEnabled = enabled;

            if(Btn_Update.Tag == null)
                Btn_Update.IsEnabled = enabled;
        }

        //forceSearch = true if should search for updates even if Last Checked is not longer than 1 Day ago
        private async Task<bool> CheckForUpdates(bool forceSearch) {
            try {
                //Last update Check
                DateTime lastChecked = FileIO.LastChecked;

                //Update Available?
                bool updateAvailable = FileIO.UpdateAvailable;

                //Last Update Content for Label
                Label_LastUpdate.Content = string.Format(str.updateLast, $"{lastChecked:dd.MM.yyyy HH:mm}");

                //If AutoUpdate is disabled and the User does not manually search, exit Method
                if(!FileIO.AutoUpdate && !forceSearch)
                    return false;

                //Update Loading Indicator
                loadingDesc.Content = str.checkingUpdate;

                //Check for Update, if last update is longer than 1 Day ago
                if(forceSearch || DateTime.Now - lastChecked > TimeSpan.FromDays(1) || updateAvailable) {
                    //Retrieve info from github
                    GitHubClient github = new GitHubClient(new ProductHeaderValue("ImgurSniper"));
                    _commits = await github.Repository.Commit.GetAll("mrousavy", "ImgurSniper");

                    FileIO.LastChecked = DateTime.Now;

                    //Last Update Content for Label
                    Label_LastUpdate.Content = string.Format(str.updateLast, $"{DateTime.Now:dd.MM.yyyy HH:mm}");

                    int currentCommits = FileIO.CurrentCommits;
                    //999 = value is unset
                    if(currentCommits == 999) {
                        FileIO.CurrentCommits = _commits.Count;
                    } else if(updateAvailable || _commits.Count > currentCommits) {
                        //Newer Version is available
                        FileIO.UpdateAvailable = true;
                        Btn_Update.IsEnabled = true;
                        success_toast.Show(string.Format(str.updateAvailable, currentCommits, _commits.Count),
                            TimeSpan.FromSeconds(4));

                        return true;
                    } else {
                        //No Update available
                        FileIO.UpdateAvailable = false;
                    }
                }
            } catch {
                error_toast.Show(str.failedUpdate, TimeSpan.FromSeconds(3));
            }
            //Any other way than return true = no update
            return false;
        }
    }
}
