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
        public InstallerHelper Helper;

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
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                return anim;
            }
        }
        private static DoubleAnimation FadeIn {
            get {
                DoubleAnimation anim = new DoubleAnimation {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                return anim;
            }
        }
        private readonly ImgurLoginHelper _imgurhelper;


        public MainWindow() {
            InitializeComponent();
            this.Closing += WindowClosing;

            if(!Directory.Exists(DocPath))
                Directory.CreateDirectory(DocPath);

            Helper = new InstallerHelper(Path, DocPath, error_toast, success_toast, this);
            _imgurhelper = new ImgurLoginHelper(error_toast, success_toast);

            error_toast.Show(str.loading, TimeSpan.FromSeconds(2));
            Load();
        }

        //Load all Configs
        private async void Load() {
            PathBox.Text = DocPath;

            if(!FileIO.IsInContextMenu) {
                Helper.AddToContextMenu();
                FileIO.IsInContextMenu = true;
            }

            #region Read Config
            try {
                string SaveImagesPath = FileIO.SaveImagesPath;
                bool UsePNG = FileIO.UsePNG;
                bool AllMonitors = FileIO.AllMonitors;
                bool OpenAfterUpload = FileIO.OpenAfterUpload;
                bool UsePrint = FileIO.UsePrint;
                bool RunOnBoot = FileIO.RunOnBoot;
                //bool Magnifyer = FileIO.MagnifyingGlassEnabled;
                bool SaveImages = FileIO.SaveImages;
                bool ImgurAfterSnipe = FileIO.ImgurAfterSnipe;

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
                if(DateTime.Now - FileIO.LastChecked > TimeSpan.FromDays(1)) {
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
                        Btn_Update.Visibility = Visibility.Visible;
                        success_toast.Show(string.Format(str.updateAvailable, currentCommits, _commits.Count),
                            TimeSpan.FromSeconds(4));
                    }
                }
            } catch {
                error_toast.Show(str.failedUpdate, TimeSpan.FromSeconds(3));
            }
        }

        #region Action Listeners
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Closing -= WindowClosing;

            DoubleAnimation fadingAnimation = new DoubleAnimation {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                AutoReverse = false
            };
            fadingAnimation.Completed += delegate {
                this.Close();
            };

            grid.BeginAnimation(Grid.OpacityProperty, fadingAnimation);
        }
        private void AfterSnapClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if(button == null) {
                return;
            }
            try {
                FileIO.ImgurAfterSnipe = button.Tag as string == "Imgur";
            } catch { }
        }
        private void MonitorsClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if(button == null) {
                return;
            }
            try {
                FileIO.AllMonitors = button.Tag as string == "All";
            } catch { }
        }
        private void ImgFormatClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if(button != null) {
                try {
                    FileIO.UsePNG = button.Tag as string == "PNG";
                } catch { }
            }
        }
        private void SaveImgs_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box == null) {
                return;
            }
            try {
                FileIO.SaveImages = box.IsChecked == true;

                if(box.IsChecked.HasValue) {
                    PathPanel.IsEnabled = (bool)box.IsChecked;
                }
            } catch { }
        }
        private void Magnifying_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box == null) {
                return;
            }
            try {
                FileIO.MagnifyingGlassEnabled = box.IsChecked == true;
            } catch { }
        }
        private void OpenAfterUpload_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box == null) {
                return;
            }
            try {
                FileIO.OpenAfterUpload = box.IsChecked == true;
            } catch { }
        }
        private async void RunOnBoot_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                FileIO.RunOnBoot = box.IsChecked == true;

                //Run proecess if not running
                try {
                    bool choice = box.IsChecked == true;
                    //Show Dialog on disabling
                    if(box.IsChecked == false) {
                        choice = !await AreYouSure(str.disablingTrayWarning);
                        RunOnBoot.IsChecked = choice;
                    }

                    Helper.Autostart(choice);

                    //Choice: Are you sure you want to disable?
                    if(choice) {
                        //Start ImgurSniper if not yet running
                        if(Process.GetProcessesByName("ImgurSniper").Length < 1) {
                            Process start = new Process {
                                StartInfo = {
                                        FileName = Path + "\\ImgurSniper.exe",
                                        Arguments = " -autostart"
                                    }
                            };
                            start.Start();
                        }
                    } else {
                        //Kill all ImgurSniper Instances
                        foreach(Process proc in Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper"))) {
                            if(proc.Id != Process.GetCurrentProcess().Id)
                                proc.Kill();
                        }
                    }
                } catch {
                    error_toast.Show(str.trayServiceNotRunning, TimeSpan.FromSeconds(2));
                }
            }
        }
        private void PrintKeyBox_Click(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box == null) {
                return;
            }
            try {
                FileIO.UsePrint = box.IsChecked == true;


                //Restart ImgurSniper

                //Kill all ImgurSniper Instances
                foreach(Process proc in Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper"))) {
                    if(proc.Id != Process.GetCurrentProcess().Id)
                        proc.Kill();
                }
                //Start ImgurSniper if not yet running
                Process start = new Process {
                    StartInfo = {
                    FileName = Path + "\\ImgurSniper.exe",
                    Arguments = " -autostart"
                    }
                };
                start.Start();
            } catch { }
        }
        private async void Snipe(object sender, RoutedEventArgs e) {
            string exe = System.IO.Path.Combine(Path, "ImgurSniper.exe");

            if(File.Exists(exe)) {
                Process snipeProc = new Process { StartInfo = new ProcessStartInfo(exe) };
                snipeProc.Start();

                this.Visibility = Visibility.Hidden;

                await Task.Delay(500);
                snipeProc.WaitForExit();

                this.Visibility = Visibility.Visible;
            } else {
                error_toast.Show(str.imgurSniperNotFound,
                    TimeSpan.FromSeconds(3));
            }
        }
        private async void Repair(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            try {
                FileIO.WipeUserData();
                await success_toast.ShowAsync(str.repairedImgurSniper, TimeSpan.FromSeconds(3));
                this.Close();
            } catch(Exception ex) {
                error_toast.Show(string.Format(str.unknownError, ex.Message),
                    TimeSpan.FromSeconds(5));
            }
        }
        private void Update(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            FileIO.CurrentCommits = _commits.Count;

            Helper.Update(sender as Button);
        }
        private void SignIn(object sender, RoutedEventArgs e) {
            try {
                _imgurhelper.Authorize();

                DoubleAnimation fadeBtnOut = FadeOut;
                fadeBtnOut.Completed += delegate {

                    DoubleAnimation fadePanelIn = FadeIn;
                    fadePanelIn.Completed += delegate {
                        Btn_SignIn.Visibility = Visibility.Collapsed;
                    };
                    Panel_PIN.Visibility = Visibility.Visible;
                    Panel_PIN.BeginAnimation(StackPanel.OpacityProperty, fadePanelIn);

                };
                Btn_SignIn.BeginAnimation(Button.OpacityProperty, fadeBtnOut);
            } catch { }
        }
        private void SignOut(object sender, RoutedEventArgs e) {
            DoubleAnimation fadeBtnOut = FadeOut;
            fadeBtnOut.Completed += delegate {
                FileIO.DeleteToken();

                DoubleAnimation fadeBtnIn = FadeIn;
                fadeBtnIn.Completed += delegate {
                    Btn_SignOut.Visibility = Visibility.Collapsed;

                    Label_Account.Content = "Imgur Account";
                };
                Btn_SignIn.Visibility = Visibility.Visible;
                Btn_SignIn.BeginAnimation(StackPanel.OpacityProperty, fadeBtnIn);

            };
            Btn_SignOut.BeginAnimation(Button.OpacityProperty, fadeBtnOut);
        }
        private async void PINOk(object sender, RoutedEventArgs e) {
            bool result = await _imgurhelper.Login(Box_PIN.Text);

            if(!result) {
                return;
            }
            DoubleAnimation fadePanelOut = FadeOut;
            fadePanelOut.Completed += delegate {
                DoubleAnimation fadeBtnIn = FadeIn;
                fadeBtnIn.Completed += delegate {
                    Panel_PIN.Visibility = Visibility.Collapsed;
                };
                Btn_SignOut.Visibility = Visibility.Visible;
                Btn_SignOut.BeginAnimation(StackPanel.OpacityProperty, fadeBtnIn);

            };
            Panel_PIN.BeginAnimation(Button.OpacityProperty, fadePanelOut);

            if(_imgurhelper.User != null) {
                Label_Account.Content = string.Format(str.imgurAccSignedIn, _imgurhelper.User);

                Btn_SignIn.Visibility = Visibility.Collapsed;
                Btn_SignOut.Visibility = Visibility.Visible;
            }
            Box_PIN.Clear();
        }
        private void Box_PIN_TextChanged(object sender, TextChangedEventArgs e) {
            Btn_PinOk.IsEnabled = Box_PIN.Text.Length > 0;
        }
        private void PathBox_Submit(object sender, System.Windows.Input.KeyEventArgs e) {
            if(e.Key == System.Windows.Input.Key.Enter) {
                SavePath();
            }
        }
        private void PathChooser(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            if(Directory.Exists(PathBox.Text))
                fbd.SelectedPath = PathBox.Text;

            fbd.Description = str.selectPath;

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            if(string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                return;
            }
            PathBox.Text = fbd.SelectedPath;
            SavePath();
        }
        private void SavePath() {
            if(Directory.Exists(PathBox.Text)) {
                FileIO.SaveImagesPath = PathBox.Text;
            } else {
                error_toast.Show(str.pathNotExist, TimeSpan.FromSeconds(4));
            }
        }
        #endregion


        //Enable or disable Buttons
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




        //Show a Material Design Dialog
        private async Task<bool> AreYouSure(string message) {
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

        private void CloseDia() {
            DialogHost.CloseDialogCommand.Execute(null, DialogHost);
        }

        private void Help(object sender, RoutedEventArgs e) {
            Process.Start("http://github.com/mrousavy/ImgurSniper");
        }
    }
}
