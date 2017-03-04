using MaterialDesignThemes.Wpf;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

        //Animation Templates
        private static DoubleAnimation FadeOut {
            get {
                DoubleAnimation anim = new DoubleAnimation {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.18))
                };
                return anim;
            }
        }
        private static DoubleAnimation FadeIn {
            get {
                DoubleAnimation anim = new DoubleAnimation {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.18))
                };
                return anim;
            }
        }
        #endregion

        //Constructor
        public MainWindow() {
            InitializeComponent();

            //Check for Commandline Arguments
            Arguments();

            //Window Closing Event
            Closing += WindowClosing;

            //Create Documents\ImgurSniper Path
            if(!Directory.Exists(DocPath))
                Directory.CreateDirectory(DocPath);

            //Initialize Helpers
            Helper = new InstallerHelper(Path, DocPath, error_toast, success_toast, this);
            _imgurhelper = new ImgurLoginHelper(error_toast, success_toast);

            //Load Config
            error_toast.Show(str.loading, TimeSpan.FromSeconds(2));
            Load();
        }

        //Command Line Args
        private void Arguments() {
            string[] args = Environment.GetCommandLineArgs();
            if(args.Contains("Help")) {
                Help(null, null);
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
                string language = settings.Language;

                //Path to Saved Images
                PathBox.Text = string.IsNullOrWhiteSpace(SaveImagesPath) ? DocPath : SaveImagesPath;

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

            } catch { }
            #endregion

            //Run proecess if not running
            try {
                if(RunOnBoot.IsChecked == true) {
                    if(Process.GetProcessesByName("ImgurSniper").Length < 1) {
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


            string refreshToken = FileIO.ReadRefreshToken();
            //name = null if refreshToken = null or any error occured in Login
            string name = await _imgurhelper.LoggedInUser(refreshToken);

            if(name != null) {
                Label_Account.Content = string.Format(str.imgurAccSignedIn, name);

                Btn_SignIn.Visibility = Visibility.Collapsed;
                Btn_SignOut.Visibility = Visibility.Visible;
            }

            if(SaveBox.IsChecked.HasValue) {
                PathPanel.IsEnabled = (bool)SaveBox.IsChecked;
            }

            try {
                //Check for Update, if last update is longer than 1 Day ago
                if(DateTime.Now - FileIO.LastChecked > TimeSpan.FromDays(1) || FileIO.UpdateAvailable) {
                    FileIO.LastChecked = DateTime.Now;

                    //Retrieve info from github
                    GitHubClient github = new GitHubClient(new ProductHeaderValue("ImgurSniper"));
                    _commits = await github.Repository.Commit.GetAll("mrousavy", "ImgurSniper");

                    int currentCommits = FileIO.CurrentCommits;
                    //999 = value is unset
                    if(currentCommits == 999) {
                        FileIO.CurrentCommits = _commits.Count;
                    } else if(_commits.Count > currentCommits) {
                        //Newer Version is available
                        FileIO.UpdateAvailable = true;
                        Btn_Update.IsEnabled = true;
                        success_toast.Show(string.Format(str.updateAvailable, currentCommits, _commits.Count),
                            TimeSpan.FromSeconds(4));
                    }
                }
            } catch {
                error_toast.Show(str.failedUpdate, TimeSpan.FromSeconds(3));
            }
        }

        //Enable or disable all Buttons
        public void ChangeButtonState(bool enabled) {
            if(Btn_PinOk.Tag == null)
                Btn_PinOk.IsEnabled = enabled;

            //if(Btn_Repair.Tag == null)
            //Btn_Repair.IsEnabled = enabled;

            if(Btn_SignIn.Tag == null)
                Btn_SignIn.IsEnabled = enabled;

            if(Btn_SignOut.Tag == null)
                Btn_SignOut.IsEnabled = enabled;

            if(Btn_Snipe.Tag == null)
                Btn_Snipe.IsEnabled = enabled;

            if(Btn_Update.Tag == null)
                Btn_Update.IsEnabled = enabled;
        }

        //Show a Material Design Yes/No Dialog
        private async Task<bool> ShowAskDialog(string message) {
            bool choice = false;

            CloseDia();

            StackPanel vPanel = new StackPanel {
                Margin = new Thickness(5)
            };

            StackPanel hPanel = new StackPanel {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            System.Windows.Controls.Label label = new System.Windows.Controls.Label {
                Content = message,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button yes = new Button {
                Content = str.yes,
                Foreground = Brushes.Gray,
                Width = 60,
                Margin = new Thickness(3),
                Style = (Style)FindResource("MaterialDesignFlatButton")
            };
            yes.Click += delegate {
                choice = true;
                CloseDia();
            };
            Button no = new Button {
                Content = str.no,
                Foreground = Brushes.Gray,
                Width = 60,
                Margin = new Thickness(3),
                Style = (Style)FindResource("MaterialDesignFlatButton")
            };
            no.Click += delegate {
                choice = false;
                CloseDia();
            };

            hPanel.Children.Add(yes);
            hPanel.Children.Add(no);

            vPanel.Children.Add(label);
            vPanel.Children.Add(hPanel);

            await DialogHost.ShowDialog(vPanel);

            return choice;
        }

        //Show a Material Design Progressbar Dialog
        private StackPanel ShowProgressDialog() {
            CloseDia();

            StackPanel vpanel = new StackPanel {
                Margin = new Thickness(10)
            };

            System.Windows.Controls.Label label = new System.Windows.Controls.Label {
                Content = str.downloadingUpdate,
                FontSize = 13,
                Foreground = Brushes.Gray
            };

            ProgressBar bar = new ProgressBar {
                Margin = new Thickness(3),
                IsIndeterminate = false,
                Minimum = 0,
                Maximum = 100
            };

            vpanel.Children.Add(label);
            vpanel.Children.Add(bar);

            DialogHost.ShowDialog(vpanel);

            return vpanel;
        }

        //Close the Material Design Dialog
        private void CloseDia() {
            DialogHost.CloseDialogCommand.Execute(null, DialogHost);
        }
    }
}
