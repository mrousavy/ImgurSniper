using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using str = ImgurSniper.UI.Properties.strings;


namespace ImgurSniper.UI {
    public partial class MainWindow {
        #region Action Listeners

        //Save Changes Button
        private async void Save(object s, RoutedEventArgs e) {
            try {
                Cursor = Cursors.Wait;

                ConfigHelper.Save();

                //Restart for applied changes
                InstallerHelper.KillImgurSniper(false);
                //If not Tray Service, do not start
                if (ConfigHelper.RunOnBoot) {
                    try {
                        InstallerHelper.StartImgurSniper();
                    } catch {
                        await ErrorToast.ShowAsync(str.trayServiceNotRunning, TimeSpan.FromSeconds(3));
                    }
                }

                SuccessToast.Show(str.applied, TimeSpan.FromSeconds(2));

                BtnSave.IsEnabled = false;
            } catch {
                ErrorToast.Show(str.couldNotApply, TimeSpan.FromSeconds(3));
            }
            Cursor = Cursors.Arrow;
        }

        public void Update(object sender, RoutedEventArgs e) {
            DoubleAnimation darken = Animations.GetDarkenAnimation(Opacity);
            DoubleAnimation brighten = Animations.GetBrightenAnimation(Opacity);

            VersionInfo info = new VersionInfo(InstallerHelper.Commits, ConfigHelper.CurrentCommits);

            try {
                info.Owner = this;
            } catch {
                // ignored
            }

            darken.Completed += delegate {
                bool? result = info.ShowDialog();

                if (result == true) {
                    ConfigHelper.CurrentCommits = InstallerHelper.Commits.Count;
                    ConfigHelper.UpdateAvailable = false;

                    StackPanel panel = ShowProgressDialog();
                    Helper.Update(panel);
                } else {

                    if (info.Skipped) {
                        if (sender is Button btn)
                            btn.IsEnabled = false;
                    }
                }

                BeginAnimation(OpacityProperty, brighten);
            };

            BeginAnimation(OpacityProperty, darken);
        }

        private void Image_OpenRepository(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.github.com/mrousavy/ImgurSniper");
        }

        private void Image_OpenGitHub(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.github.com/mrousavy");
        }

        //Ask for Save ?
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (BtnSave.IsEnabled) {
                e.Cancel = true;

                bool save = await ShowAskDialog(str.wantToApply);

                if (save) {
                    Save(null, null);
                    Close();
                } else {
                    BtnSave.IsEnabled = false;
                    Close();
                }
            } else {
                e.Cancel = false;
            }
        }
        #endregion
    }
}
