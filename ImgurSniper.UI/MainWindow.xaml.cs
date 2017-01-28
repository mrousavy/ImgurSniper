using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ImgurSniper.UI {
    public partial class MainWindow : Window {
        public InstallerHelper helper;

        //Path to Program Files/ImgurSniper Folder
        private string _path {
            get {
                //string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ImgurSniper");
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        //Path to Documents/ImgurSniper Folder
        private string _docPath {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
                return value;
            }
        }

        //Animation Templates
        private DoubleAnimation _fadeOut {
            get {
                DoubleAnimation anim = new DoubleAnimation();
                anim.From = 1;
                anim.To = 0;
                anim.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                return anim;
            }
        }
        private DoubleAnimation _fadeIn {
            get {
                DoubleAnimation anim = new DoubleAnimation();
                anim.From = 0;
                anim.To = 1;
                anim.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                return anim;
            }
        }
        private ImgurLoginHelper _imgurhelper;


        public MainWindow() {
            InitializeComponent();
            this.Closing += WindowClosing;

            if(!Directory.Exists(_path)) {
                Directory.CreateDirectory(_path);
            }

            if(!Directory.Exists(_docPath))
                Directory.CreateDirectory(_docPath);

            helper = new InstallerHelper(_path, error_toast, success_toast, this);
            _imgurhelper = new ImgurLoginHelper(error_toast, success_toast);

            error_toast.Show("Loading...", TimeSpan.FromSeconds(2));
            Load();
        }

        //Load all Configs
        private async void Load() {
            PathBox.Text = _docPath;

            if(!FileIO.IsInContextMenu) {
                helper.AddToContextMenu();
                FileIO.SaveConfig(FileIO.ConfigType.IsInContextMenu, "True");
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
                PathBox.Text = string.IsNullOrWhiteSpace(SaveImagesPath) ? _docPath : SaveImagesPath;

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
                    helper.Autostart(true);
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
            } catch(Exception) { }
            #endregion

            //Run proecess if not running
            try {
                if(RunOnBoot.IsChecked == true) {
                    if(Process.GetProcessesByName("ImgurSniper").Length < 1) {
                        Process start = new Process {
                            StartInfo = {
                                FileName = _path + "\\ImgurSniper.exe",
                                Arguments = " -autostart"
                            }
                        };
                        start.Start();
                    }
                }
            } catch(Exception) {
                error_toast.Show("ImgurSniper Tray Service is not running!", TimeSpan.FromSeconds(2));
            }

            string refreshToken = FileIO.ReadRefreshToken();
            //name = null if refreshToken = null or any error occured in Login
            string name = await _imgurhelper.LoggedInUser(refreshToken);

            if(name != null) {
                Label_Account.Content = Label_Account.Content as string + " (Logged In as " + name + ")";

                Btn_SignIn.Visibility = Visibility.Collapsed;
                Btn_SignOut.Visibility = Visibility.Visible;
            }

            if(SaveBox.IsChecked.HasValue) {
                PathPanel.IsEnabled = (bool)SaveBox.IsChecked;
            }
        }

        #region Action Listeners
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Closing -= WindowClosing;

            DoubleAnimation fadingAnimation = new DoubleAnimation();
            fadingAnimation.From = 1;
            fadingAnimation.To = 0;
            fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            fadingAnimation.AutoReverse = false;
            fadingAnimation.Completed += delegate {
                this.Close();
            };

            grid.BeginAnimation(Grid.OpacityProperty, fadingAnimation);
        }
        private void AfterSnapClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if(button != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.AfterSnipeAction, button.Tag as string);
                } catch(Exception) { }
            }
        }
        private void MonitorsClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if(button != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.SnipeMonitor, button.Tag as string);
                } catch(Exception) { }
            }
        }
        private void ImgFormatClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if(button != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.ImageFormat, button.Tag as string);
                } catch(Exception) { }
            }
        }
        private void SaveImgs_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.SaveImages, box.IsChecked.ToString());

                    if(box.IsChecked.HasValue) {
                        PathPanel.IsEnabled = (bool)box.IsChecked;
                    }
                } catch(Exception) { }
            }
        }
        private void Magnifying_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.Magnifyer, box.IsChecked.ToString());
                } catch(Exception) { }
            }
        }
        private void OpenAfterUpload_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.OpenAfterUpload, box.IsChecked.ToString());
                } catch(Exception) { }
            }
        }
        private void RunOnBoot_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.RunOnBoot, box.IsChecked.ToString());


                    //Run proecess if not running
                    try {
                        if(RunOnBoot.IsChecked == true) {
                            if(Process.GetProcessesByName("ImgurSniper").Length < 1) {
                                Process start = new Process {
                                    StartInfo = {
                                FileName = _path + "\\ImgurSniper.exe",
                                Arguments = " -autostart"
                            }
                                };
                                start.Start();
                            }
                        }
                    } catch(Exception) {
                        error_toast.Show("ImgurSniper Tray Service is not running!", TimeSpan.FromSeconds(2));
                    }


                    helper.Autostart(box.IsChecked);
                } catch(Exception) { }
            }
        }
        private void PrintKeyBox_Click(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.UsePrint, box.IsChecked.ToString());
                } catch(Exception) { }
            }
        }
        private void Snipe(object sender, RoutedEventArgs e) {
            string exe = Path.Combine(_path, "ImgurSniper.exe");

            if(File.Exists(exe)) {
                Process.Start(exe);
            } else {
                error_toast.Show("Error, ImgurSniper could not be found on your System!",
                    TimeSpan.FromSeconds(3));
            }
        }
        private async void Repair(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            try {
                FileIO.WipeUserData();
                await success_toast.ShowAsync("Repaired ImgurSniper. Please restart ImgurSniper to complete!", TimeSpan.FromSeconds(3));
                this.Close();
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
        private void Uninstall(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            helper.Uninstall();
        }
        private void SignIn(object sender, RoutedEventArgs e) {
            try {
                _imgurhelper.Authorize();

                DoubleAnimation fadeBtnOut = _fadeOut;
                fadeBtnOut.Completed += delegate {

                    DoubleAnimation fadePanelIn = _fadeIn;
                    fadePanelIn.Completed += delegate {
                        Btn_SignIn.Visibility = Visibility.Collapsed;
                    };
                    Panel_PIN.Visibility = Visibility.Visible;
                    Panel_PIN.BeginAnimation(StackPanel.OpacityProperty, fadePanelIn);

                };
                Btn_SignIn.BeginAnimation(Button.OpacityProperty, fadeBtnOut);
            } catch(Exception) { }
        }
        private void SignOut(object sender, RoutedEventArgs e) {
            DoubleAnimation fadeBtnOut = _fadeOut;
            fadeBtnOut.Completed += delegate {
                FileIO.DeleteToken();

                DoubleAnimation fadeBtnIn = _fadeIn;
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

            if(result) {
                DoubleAnimation fadePanelOut = _fadeOut;
                fadePanelOut.Completed += delegate {
                    DoubleAnimation fadeBtnIn = _fadeIn;
                    fadeBtnIn.Completed += delegate {
                        Panel_PIN.Visibility = Visibility.Collapsed;
                    };
                    Btn_SignOut.Visibility = Visibility.Visible;
                    Btn_SignOut.BeginAnimation(StackPanel.OpacityProperty, fadeBtnIn);

                };
                Panel_PIN.BeginAnimation(Button.OpacityProperty, fadePanelOut);

                if(_imgurhelper.User != null) {
                    Label_Account.Content = Label_Account.Content as string + " (Logged In as " + _imgurhelper.User + ")";

                    Btn_SignIn.Visibility = Visibility.Collapsed;
                    Btn_SignOut.Visibility = Visibility.Visible;
                }
                Box_PIN.Clear();
            }
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

            fbd.Description = "Select the Path where ImgurSniper should save Images.";

            System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            if(!string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                PathBox.Text = fbd.SelectedPath;
                SavePath();
            }
        }
        private void SavePath() {
            if(Directory.Exists(PathBox.Text)) {
                FileIO.SaveConfig(FileIO.ConfigType.Path, PathBox.Text);
            } else {
                error_toast.Show("The selected Path does not exist!", TimeSpan.FromSeconds(4));
            }
        }
        #endregion


        //Enable or disable Buttons
        public void ChangeButtonState(bool enabled) {
            if(Btn_PinOk.Tag == null)
                Btn_PinOk.IsEnabled = enabled;

            if(Btn_Repair.Tag == null)
                Btn_Repair.IsEnabled = enabled;

            if(Btn_SignIn.Tag == null)
                Btn_SignIn.IsEnabled = enabled;

            if(Btn_SignOut.Tag == null)
                Btn_SignOut.IsEnabled = enabled;

            if(Btn_Snipe.Tag == null)
                Btn_Snipe.IsEnabled = enabled;

            if(Btn_Uninstall.Tag == null)
                Btn_Uninstall.IsEnabled = enabled;
        }
    }
}
