using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ImgurSniper.UI.Properties;
using Path = System.IO.Path;

namespace ImgurSniper.UI.Pages.SettingsPages {
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage {
        private readonly MainWindow _window;

        public HomePage() {
            InitializeComponent();

            _window = Application.Current.MainWindow as MainWindow;

            int commits = InstallerHelper.TotalCommits;
            if (commits == 999) {
                CommitsDisplay.Opacity = 0;
            } else {
                CommitsDisplay.Content = string.Format(strings.commitsDisplay, InstallerHelper.TotalCommits);
            }
        }

        private void Help(object sender, RoutedEventArgs e) { _window.Help(sender, e); }

        private async void Snipe(object sender, RoutedEventArgs e) {
            string exe = Path.Combine(ConfigHelper.InstallDir, "ImgurSniper.exe");

            if (File.Exists(exe)) {
                Process snipeProc = new Process { StartInfo = new ProcessStartInfo(exe) };
                snipeProc.Start();

                Visibility = Visibility.Hidden;

                await Task.Delay(500);
                snipeProc.WaitForExit();

                Visibility = Visibility.Visible;
            } else {
                _window.ErrorToast.Show(strings.imgurSniperNotFound,
                    TimeSpan.FromSeconds(3));
            }
        }

        public async void Gif(object sender, RoutedEventArgs e) {
            string exe = Path.Combine(ConfigHelper.InstallDir, "ImgurSniper.exe");

            if (File.Exists(exe)) {
                Process snipeProc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = exe,
                        Arguments = "-gif"
                    }
                };
                snipeProc.Start();

                Visibility = Visibility.Hidden;

                await Task.Delay(500);
                snipeProc.WaitForExit();

                Visibility = Visibility.Visible;
            } else {
                _window.ErrorToast.Show(strings.imgurSniperNotFound,
                    TimeSpan.FromSeconds(3));
            }
        }

    }
}
