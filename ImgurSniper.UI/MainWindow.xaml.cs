using System;
using System.Diagnostics;
using System.IO;
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
                if(!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                return value;
            }
        }
        private string _docPath {
            get {
                string value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
                if(!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                return value;
            }
        }

        public InstallerHelper helper;


        public MainWindow() {
            InitializeComponent();
            this.Closing += WindowClosing;

            helper = new InstallerHelper(_path, error_toast, success_toast);

            Load();
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

        }
        private void Install(object sender, RoutedEventArgs e) {
            try {
                helper.Install(sender);
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
        private void Uninstall(object sender, RoutedEventArgs e) {
            try {
                //Kill open instances if any
                foreach(Process p in Process.GetProcessesByName("ImgurSniper")) {
                    p.Kill();
                }

                //Remove all files
                Array.ForEach(Directory.GetFiles(_docPath), File.Delete);
                Array.ForEach(Directory.GetFiles(_path), File.Delete);

                this.Close();
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
        private void DesktopShortcut(object sender, RoutedEventArgs e) {
            try {
                helper.AddToDesktop(sender);
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
        private void StartmenuShortcut(object sender, RoutedEventArgs e) {
            try {
                helper.AddToStartmenu(sender);
            } catch(Exception ex) {
                error_toast.Show("An unknown Error occured!\nShow this to the smart Computer apes: " + ex.Message,
                    TimeSpan.FromSeconds(5));
            }
        }
    }
}
