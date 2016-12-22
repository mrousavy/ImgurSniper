using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ImgurSniper.UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private string _path {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ImgurSniper");
                return value;
            }
        }
        private string _docPath {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
                return value;
            }
        }

        public InstallerHelper helper;


        public MainWindow() {
            InitializeComponent();
            this.Closing += WindowClosing;

            if(!Directory.Exists(_path)) {
                Directory.CreateDirectory(_path);
                NewToImgur();
            }

            if(!Directory.Exists(_docPath))
                Directory.CreateDirectory(_docPath);

            helper = new InstallerHelper(_path, error_toast, success_toast, this);

            Load();
        }


        private async void NewToImgur() {
            await Task.Delay(500);
            success_toast.Show("Hi! You're new to ImgurSniper! Start by clicking \"Install\" first!", TimeSpan.FromSeconds(2));
        }


        private void Load() {
            string[] lines = FileIO.ReadConfig();

            for(int i = 0; i < lines.Length; i++) {
                try {
                    string property = lines[i].Split(':')[0];
                    string value = lines[i].Split(':')[1];

                    switch(property) {
                        case "AfterSnipeAction":
                            if(value == "Clipboard") {
                                ClipboardRadio.IsChecked = true;
                            } else {
                                ImgurRadio.IsChecked = true;
                            }
                            break;
                        case "SaveImages":
                            SaveBox.IsChecked = bool.Parse(value);
                            break;
                    }
                } catch(Exception) { }
            }
        }

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

        private void SaveImgs_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box != null) {
                try {
                    FileIO.SaveConfig(FileIO.ConfigType.SaveImages, box.IsChecked.ToString());
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

        private void Install(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            try {
                helper.Install(sender);
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
        private void Uninstall(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            helper.Uninstall();
        }
        private void DesktopShortcut(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            try {
                helper.AddToDesktop(sender);
                success_toast.Show("Created Desktop Shortcut!", TimeSpan.FromSeconds(1));
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
        private void StartmenuShortcut(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            try {
                helper.AddToStartmenu(sender);
                success_toast.Show("Created Startmenu Shortcut!", TimeSpan.FromSeconds(1));
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Enable or disable Buttons
        /// </summary>
        public void ChangeButtonState(bool enabled) {
            if(Btn_Desktop.Tag == null)
                Btn_Desktop.IsEnabled = enabled;

            if(Btn_Install.Tag == null)
                Btn_Install.IsEnabled = enabled;

            if(Btn_Snipe.Tag == null)
                Btn_Snipe.IsEnabled = enabled;

            if(Btn_Startmenu.Tag == null)
                Btn_Startmenu.IsEnabled = enabled;

            if(Btn_Uninstall.Tag == null)
                Btn_Uninstall.IsEnabled = enabled;
        }
    }
}
