using FFmpegManager.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace FFmpegManager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        private void ProgressChanged(object o, DownloadProgressChangedEventArgs e) {
            Progress.Value = e.ProgressPercentage;
        }

        private async Task Install() {
            StatusLabel.Content = strings.downloading;

            string archive = await FFmpegHelper.DownloadFFmpeg(ProgressChanged);

            StatusLabel.Content = strings.installing;
            FFmpegHelper.InstallFFmpeg(archive);
        }

        private void Remove() {
            StatusLabel.Content = strings.removing;

            FFmpegHelper.RemoveFFmpeg();

            Progress.Value = 100;
        }

        private async void WindowLoaded(object sender, System.Windows.RoutedEventArgs e) {
            FadeIn();

            //remove dash or slash infront of param
            Regex regexParam = new Regex("^(-/)");
            List<string> args = new List<string>(Environment.GetCommandLineArgs().Select(arg => regexParam.Replace(arg, "")));

            if (args.Contains("install")) {
                await Install();
                StatusLabel.Content = strings.done;
                Cursor = Cursors.Arrow;
                await Task.Delay(1500);
                FadeOut();
            } else if (args.Contains("remove")) {
                Remove();
                StatusLabel.Content = strings.done;
                Cursor = Cursors.Arrow;
                await Task.Delay(1500);
                FadeOut();
            } else {
                Cursor = Cursors.Arrow;
                Progress.Visibility = System.Windows.Visibility.Collapsed;
                StatusLabel.Content = strings.invalidParam;
                await Task.Delay(1500);
                FadeOut();
            }
        }


        private void FadeIn() {
            DoubleAnimation anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            BeginAnimation(OpacityProperty, anim);
        }

        private void FadeOut() {
            DoubleAnimation anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
            anim.Completed += delegate { Close(); };
            BeginAnimation(OpacityProperty, anim);
        }
    }
}
