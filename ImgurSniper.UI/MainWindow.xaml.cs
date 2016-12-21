using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ImgurSniper.UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            this.Closing += WindowClosing;
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
    }
}
