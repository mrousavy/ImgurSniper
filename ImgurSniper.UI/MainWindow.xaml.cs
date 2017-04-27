using Octokit;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using str = ImgurSniper.UI.Properties.strings;

namespace ImgurSniper.UI {
    public partial class MainWindow {
        //Constructor
        public MainWindow() {
            InitializeComponent();

            //Update Loading Indicator
            LoadingDesc.Content = str.initializing;

            //Check for Commandline Arguments
            Arguments();

            //Create Documents\ImgurSniper Path
            if (!Directory.Exists(DocPath)) {
                Directory.CreateDirectory(DocPath);
            }

            //Initialize Helpers
            Helper = new InstallerHelper(Path, DocPath, ErrorToast, SuccessToast, this);
            _imgurhelper = new ImgurLoginHelper(ErrorToast, SuccessToast);
        }

        //Command Line Args
        private async void Arguments() {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("Help")) {
                Hide();
                Help(null, null);
                Close();
            }
            if (args.Contains("Update")) {
                Hide();
                bool updateAvailable = await CheckForUpdates(true);
                if (updateAvailable) {
                    Update(null, null);
                } else {
                    Close();
                }
            }
            if (args.Contains("Troubleshooting")) {
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
            if (!ConfigHelper.IsInContextMenu) {
                Helper.AddToContextMenu();
                ConfigHelper.IsInContextMenu = true;
                ConfigHelper.Save();

                //First time using ImgurSniper? Show Help Window
                Help(null, null);
            }

            //Update Loading Indicator
            LoadingDesc.Content = str.loadConf;

            #region Read Config

            try {
                //Only 1x ConfigHelper File read, optimized performance
                ConfigHelper.Settings settings = ConfigHelper.JsonConfig;

                bool allMonitors = settings.AllMonitors;
                bool openAfterUpload = settings.OpenAfterUpload;
                bool usePrint = settings.UsePrint;
                bool runOnBoot = settings.RunOnBoot;
                //bool Magnifyer = settings.MagnifyingGlassEnabled;
                bool saveImages = settings.SaveImages;
                bool imgurAfterSnipe = settings.ImgurAfterSnipe;
                bool autoUpdate = settings.AutoUpdate;
                bool showMouse = settings.ShowMouse;
                int gifFps = settings.GifFps;
                int gifLength = settings.GifLength / 1000;
                long compression = settings.Compression;
                string language = settings.Language;
                string saveImagesPath = settings.SaveImagesPath;
                Key imgKey = settings.ShortcutImgKey;
                Key gifKey = settings.ShortcutGifKey;
                ImageFormat format = settings.ImageFormat;

                //Path to Saved Images
                if (string.IsNullOrWhiteSpace(saveImagesPath)) {
                    PathBox.Text = DocPath;
                } else {
                    //Create Pictures\ImgurSniperImages Path
                    try {
                        if (!Directory.Exists(saveImagesPath)) {
                            Directory.CreateDirectory(saveImagesPath);
                        }
                        PathBox.Text = saveImagesPath;
                    } catch {
                        PathBox.Text = "";
                    }
                }


                //Image Format
                switch (format.ToString()) {
                    case "Jpeg":
                        ImageFormatBox.SelectedIndex = 0;
                        break;
                    case "Png":
                        ImageFormatBox.SelectedIndex = 1;
                        break;
                    case "Gif":
                        ImageFormatBox.SelectedIndex = 2;
                        break;
                    case "Tiff":
                        ImageFormatBox.SelectedIndex = 3;
                        break;
                }

                //Current or All Monitors
                if (allMonitors) {
                    MultiMonitorRadio.IsChecked = true;
                } else {
                    CurrentMonitorRadio.IsChecked = true;
                }

                //Open Image in Browser after upload
                OpenAfterUploadBox.IsChecked = openAfterUpload;

                //Use Print Key instead of default Shortcut
                PrintKeyBox.IsChecked = usePrint;

                //Run ImgurSniper on boot
                if (runOnBoot) {
                    RunOnBoot.IsChecked = true;
                    Helper.Autostart(true);
                }

                //Auto search for Updates
                if (autoUpdate) {
                    AutoUpdateBox.IsChecked = true;
                }

                //Show Mouse Cursor on Image Capture
                if (showMouse) {
                    ShowMouseBox.IsChecked = true;
                }

                //Enable or Disable Magnifying Glass (WIP)
                //MagnifyingGlassBox.IsChecked = Magnifyer;

                //Save Images on Snap
                SaveBox.IsChecked = saveImages;

                //Upload to Imgur or Copy to Clipboard after Snipe
                if (imgurAfterSnipe) {
                    ImgurRadio.IsChecked = true;
                } else {
                    ClipboardRadio.IsChecked = true;
                }

                //Set correct Language for Current Language Box
                switch (language) {
                    case "en":
                        LanguageBox.SelectedItem = en;
                        break;
                    case "de":
                        LanguageBox.SelectedItem = de;
                        break;
                }
                LanguageBox.SelectionChanged += LanguageBox_SelectionChanged;

                HotkeyGifBox.Text = gifKey.ToString();

                HotkeyImgBox.Text = imgKey.ToString();

                GifFpsSlider.Value = gifFps;
                GifFpsSlider.ValueChanged += SliderGifFps_Changed;
                GifFpsLabel.Content = string.Format(str.gifFpsVal, gifFps);

                GifLengthSlider.Value = gifLength;
                GifLengthSlider.ValueChanged += SliderGifLength_Changed;
                GifLengthLabel.Content = string.Format(str.gifLengthVal, gifLength);

                //Set Quality in %
                QualitySlider.Value = compression;
                QualitySlider.ValueChanged += QualitySlider_ValueChanged;
                QualityLabel.Content = compression + "%";
            } catch {
                await ShowOkDialog(str.couldNotLoad, string.Format(str.errorConfig, ConfigHelper.ConfigPath));
            }

            #endregion

            //Run proecess if not running
            try {
                if (RunOnBoot.IsChecked == true) {
                    InstallerHelper.StartImgurSniper();
                }
            } catch {
                ErrorToast.Show(str.trayServiceNotRunning, TimeSpan.FromSeconds(2));
            }

            //Update Loading Indicator
            LoadingDesc.Content = str.contactImgur;

            string refreshToken = ConfigHelper.ReadRefreshToken();
            //name = null if refreshToken = null or any error occured in Login
            string name = await _imgurhelper.LoggedInUser(refreshToken);

            if (name != null) {
                LabelAccount.Content = string.Format(str.imgurAccSignedIn, name);

                BtnSignIn.Visibility = Visibility.Collapsed;
                BtnViewPics.Visibility = Visibility.Visible;
                BtnSignOut.Visibility = Visibility.Visible;
            }

            if (SaveBox.IsChecked.HasValue) {
                PathPanel.IsEnabled = (bool)SaveBox.IsChecked;
            }

#if DEBUG
            BtnUpdate.IsEnabled = true;
#else
            await CheckForUpdates(false);
#endif

            //Remove Loading Indicator
            ProgressIndicator.BeginAnimation(OpacityProperty, Animations.FadeOut);

            BtnSave.IsEnabled = false;
        }

        //Enable or disable all Buttons
        public void ChangeButtonState(bool enabled) {
            if (BtnPinOk.Tag == null) {
                BtnPinOk.IsEnabled = enabled;
            }

            if (BtnSignIn.Tag == null) {
                BtnSignIn.IsEnabled = enabled;
            }

            if (BtnSignOut.Tag == null) {
                BtnSignOut.IsEnabled = enabled;
            }

            if (BtnViewPics.Tag == null) {
                BtnViewPics.IsEnabled = enabled;
            }

            if (BtnSnipe.Tag == null) {
                BtnSnipe.IsEnabled = enabled;
            }

            if (BtnUpdate.Tag == null) {
                BtnUpdate.IsEnabled = enabled;
            }
        }

        //forceSearch = true if should search for updates even if Last Checked is not longer than 1 Day ago
        private async Task<bool> CheckForUpdates(bool forceSearch) {
            try {
                //Last update Check
                DateTime lastChecked = ConfigHelper.LastChecked;

                //Update Available?
                bool updateAvailable = ConfigHelper.UpdateAvailable;

                //Last Update Content for Label
                LabelLastUpdate.Content = string.Format(str.updateLast, $"{lastChecked:dd.MM.yyyy HH:mm}");

                //If AutoUpdate is disabled and the User does not manually search, exit Method
                if (!ConfigHelper.AutoUpdate && !forceSearch) {
                    return false;
                }

                //Update Loading Indicator
                LoadingDesc.Content = str.checkingUpdate;

                //Check for Update, if last update is longer than 1 Day ago
                if (forceSearch || DateTime.Now - lastChecked > TimeSpan.FromDays(1) || updateAvailable) {
                    //Retrieve info from github
                    GitHubClient github = new GitHubClient(new ProductHeaderValue("ImgurSniper"));
                    IReadOnlyList<GitHubCommit> commitsRaw = await github.Repository.Commit.GetAll("mrousavy",
                        "ImgurSniper");
                    //All Commits where a new ImgurSniper Version is available start with "R:"
                    _commits = new List<GitHubCommit>(commitsRaw.Where(c => c.Commit.Message.StartsWith("R:")));

                    ConfigHelper.LastChecked = DateTime.Now;
                    ConfigHelper.Save();

                    //Last Update Content for Label
                    LabelLastUpdate.Content = string.Format(str.updateLast, $"{DateTime.Now:dd.MM.yyyy HH:mm}");

                    int currentCommits = ConfigHelper.CurrentCommits;
                    //999 = value is unset
                    if (currentCommits == 999) {
                        ConfigHelper.CurrentCommits = _commits.Count;
                        ConfigHelper.Save();
                    } else if (updateAvailable || _commits.Count > currentCommits) {
                        //Newer Version is available
                        ConfigHelper.UpdateAvailable = true;
                        ConfigHelper.Save();
                        BtnUpdate.IsEnabled = true;
                        SuccessToast.Show(string.Format(str.updateAvailable, currentCommits, _commits.Count),
                            TimeSpan.FromSeconds(4));

                        return true;
                    } else {
                        //No Update available
                        ConfigHelper.UpdateAvailable = false;
                        ConfigHelper.Save();
                    }
                }
            } catch {
                ErrorToast.Show(str.failedUpdate, TimeSpan.FromSeconds(3));
            }
            //Any other way than return true = no update
            return false;
        }

        #region Fields

        public InstallerHelper Helper;
        private readonly ImgurLoginHelper _imgurhelper;

        //Path to Program Files/ImgurSniper Folder
        private static string Path => AppDomain.CurrentDomain.BaseDirectory;
        private List<GitHubCommit> _commits;

        //Path to Documents/ImgurSniper Folder
        private static string DocPath {
            get {
                string value = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "ImgurSniper");
                return value;
            }
        }

        #endregion
    }
}
