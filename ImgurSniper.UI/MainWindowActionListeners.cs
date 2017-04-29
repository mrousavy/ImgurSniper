using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using RadioButton = System.Windows.Controls.RadioButton;
using str = ImgurSniper.UI.Properties.strings;
using TextBox = System.Windows.Controls.TextBox;


namespace ImgurSniper.UI {
    public partial class MainWindow {
        #region Action Listeners

        private void AfterSnapClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if (button == null) {
                return;
            }
            try {
                ConfigHelper.ImgurAfterSnipe = button.Tag as string == "Imgur";
                BtnSave.IsEnabled = true;
            } catch {
                // ignored
            }
        }

        private void MonitorsClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if (button == null) {
                return;
            }
            try {
                ConfigHelper.AllMonitors = button.Tag as string == "All";
                BtnSave.IsEnabled = true;
            } catch {
                // ignored
            }
        }

        private void ImageFormatBoxChanged(object sender, SelectionChangedEventArgs e) {

            if (ImageFormatBox.SelectedItem is ComboBoxItem item) {
                try {
                    ConfigHelper.ImageFormat = (ImageFormat)Enum.Parse(typeof(ImageFormat), (string)item.Content);
                } catch {
                    // ignored
                }

                BtnSave.IsEnabled = true;
            }
        }

        private void SaveImgs_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.SaveImages = box.IsChecked == true;
                BtnSave.IsEnabled = true;

                if (box.IsChecked.HasValue) {
                    PathPanel.IsEnabled = (bool)box.IsChecked;
                }
            } catch {
                // ignored
            }
        }


        private void ShowMouseBox_Check(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.ShowMouse = box.IsChecked == true;
                BtnSave.IsEnabled = true;
            } catch {
                // ignored
            }
        }

        private void Magnifying_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.MagnifyingGlassEnabled = box.IsChecked == true;
                BtnSave.IsEnabled = true;
            } catch {
                // ignored
            }
        }

        private void OpenAfterUpload_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.OpenAfterUpload = box.IsChecked == true;
                BtnSave.IsEnabled = true;
            } catch {
                // ignored
            }
        }

        private void AutoUpdate_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.AutoUpdate = box.IsChecked == true;
                BtnSave.IsEnabled = true;
            } catch {
                // ignored
            }
        }

        private async void RunOnBoot_Checkbox(object sender, RoutedEventArgs e) {
            if (sender is CheckBox box) {
                ConfigHelper.RunOnBoot = box.IsChecked == true;
                BtnSave.IsEnabled = true;

                //Run proecess if not running
                try {
                    bool choice = box.IsChecked == true;
                    //Show Dialog on disabling
                    if (box.IsChecked == false) {
                        choice = !await ShowAskDialog(str.disablingTrayWarning);
                        RunOnBoot.IsChecked = choice;
                    }

                    Helper.Autostart(choice);
                } catch {
                    ErrorToast.Show(str.trayServiceNotRunning, TimeSpan.FromSeconds(2));
                }
            }
        }

        private void PrintKeyBox_Click(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.UsePrint = box.IsChecked == true;
                BtnSave.IsEnabled = true;
            } catch {
                // ignored
            }
        }

        private void HotkeyImgBoxMDown(object sender, RoutedEventArgs e) {
            TextBox box = sender as TextBox;
            if (box == null) {
                return;
            }

            try {
                bool keyAvailable;
                Key selectedKey;
                do {
                    HotKeySelector sel = new HotKeySelector();

                    try {
                        sel.Owner = this;
                    } catch {
                        // ignored
                    }

                    bool? result = sel.ShowDialog();

                    if (result == true) {
                        selectedKey = sel.Key;
                        keyAvailable = ConfigHelper.ShortcutGifKey != selectedKey;
                    } else {
                        return;
                    }
                } while (!keyAvailable);

                ConfigHelper.ShortcutImgKey = selectedKey;
                BtnSave.IsEnabled = true;
                HotkeyImgBox.Text = selectedKey.ToString();
            } catch {
                // ignored
            }
        }

        private void HotkeyGifBoxMDown(object sender, RoutedEventArgs e) {
            TextBox box = sender as TextBox;
            if (box == null) {
                return;
            }

            try {
                bool keyAvailable;
                Key selectedKey;
                do {
                    HotKeySelector sel = new HotKeySelector();

                    try {
                        sel.Owner = this;
                    } catch {
                        // ignored
                    }

                    bool? result = sel.ShowDialog();

                    if (result == true) {
                        selectedKey = sel.Key;
                        keyAvailable = ConfigHelper.ShortcutImgKey != selectedKey;
                    } else {
                        return;
                    }
                } while (!keyAvailable);

                ConfigHelper.ShortcutGifKey = selectedKey;
                BtnSave.IsEnabled = true;
                HotkeyGifBox.Text = selectedKey.ToString();
            } catch {
                // ignored
            }
        }

        private async void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sender is ComboBox box) {
                try {
                    ConfigHelper.Language = ((ComboBoxItem)box.SelectedItem).Name.ToLower();

                    bool result = await ShowAskDialog(str.langChanged);

                    if (result) {
                        InstallerHelper.KillImgurSniper(false);
                        Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                        Application.Current.Shutdown();
                    }
                } catch {
                    box.SelectedIndex = 0;
                }
            }
        }

        private void SliderGifLength_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (sender is Slider slider) {
                int value = (int)slider.Value;
                ConfigHelper.GifLength = value * 1000;
                BtnSave.IsEnabled = true;

                GifLengthLabel.Content = string.Format(str.gifLengthVal, value);
            }
        }

        private void SliderGifFps_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (sender is Slider slider) {
                int value = (int)slider.Value;
                ConfigHelper.GifFps = value;
                BtnSave.IsEnabled = true;

                GifFpsLabel.Content = string.Format(str.gifFpsVal, value);
            }
        }

        private void QualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (sender is Slider slider) {
                byte value = (byte)slider.Value;
                ConfigHelper.Compression = value;
                BtnSave.IsEnabled = true;

                QualityLabel.Content = value + "%";
            }
        }

        //Save Changes Button
        private async void Save(object s, RoutedEventArgs e) {
            try {
                Cursor = System.Windows.Input.Cursors.Wait;

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
            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void Help(object sender, RoutedEventArgs e) {
            //Process.Start("http://github.com/mrousavy/ImgurSniper#features");
            Help help = new Help();

            try {
                help.Owner = this;
            } catch {
                // ignored
            }

            help.Show();
        }

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
                ErrorToast.Show(str.imgurSniperNotFound,
                    TimeSpan.FromSeconds(3));
            }
        }

        private void Update(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            DoubleAnimation darken = Animations.GetDarkenAnimation(Opacity);
            DoubleAnimation brighten = Animations.GetBrightenAnimation(Opacity);

            VersionInfo info = new VersionInfo(_commits, ConfigHelper.CurrentCommits);

            try {
                info.Owner = this;
            } catch {
                // ignored
            }

            darken.Completed += delegate {
                bool? result = info.ShowDialog();

                if (result == true) {
                    ConfigHelper.CurrentCommits = _commits.Count;
                    ConfigHelper.UpdateAvailable = false;

                    StackPanel panel = ShowProgressDialog();
                    Helper.Update(panel);
                } else {
                    ChangeButtonState(true);

                    if (info.Skipped) {
                        BtnUpdate.IsEnabled = false;
                    }
                }

                BeginAnimation(OpacityProperty, brighten);
            };

            BeginAnimation(OpacityProperty, darken);
        }


        private async void Btn_SearchUpdates(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn != null) {
                btn.IsEnabled = false;
            }

            //Show Progress Indicator
            ProgressIndicator.BeginAnimation(OpacityProperty, Animations.FadeIn);

            await CheckForUpdates(true);

            //Hide Progress Indicator
            ProgressIndicator.BeginAnimation(OpacityProperty, Animations.FadeOut);

            if (btn != null) {
                btn.IsEnabled = true;
            }
        }

        private void SignIn(object sender, RoutedEventArgs e) {
            try {
                _imgurhelper.Authorize();

                DoubleAnimation fadeBtnOut = Animations.FadeOut;
                fadeBtnOut.Completed += delegate {
                    DoubleAnimation fadePanelIn = Animations.FadeIn;
                    fadePanelIn.Completed += delegate { BtnSignIn.Visibility = Visibility.Collapsed; };
                    PanelPin.Visibility = Visibility.Visible;
                    PanelPin.BeginAnimation(OpacityProperty, fadePanelIn);
                };
                BtnSignIn.BeginAnimation(OpacityProperty, fadeBtnOut);
            } catch {
                // ignored
            }
        }

        private void SignOut(object sender, RoutedEventArgs e) {
            DoubleAnimation fadeBtnOut = Animations.FadeOut;
            BtnViewPics.BeginAnimation(OpacityProperty, fadeBtnOut);

            fadeBtnOut.Completed += delegate {
                ConfigHelper.DeleteToken();

                DoubleAnimation fadeBtnIn = Animations.FadeIn;
                BtnViewPics.BeginAnimation(OpacityProperty, fadeBtnIn);

                fadeBtnIn.Completed += delegate {
                    BtnSignOut.Visibility = Visibility.Collapsed;
                    BtnViewPics.Visibility = Visibility.Collapsed;

                    LabelAccount.Content = "Imgur Account";
                };
                BtnSignIn.Visibility = Visibility.Visible;
                BtnSignIn.BeginAnimation(OpacityProperty, fadeBtnIn);
            };
            BtnSignOut.BeginAnimation(OpacityProperty, fadeBtnOut);
        }

        private void ViewPics(object sender, RoutedEventArgs e) {
            Process.Start(_imgurhelper.UserUrl);
        }

        private async void PinOk(object sender, RoutedEventArgs e) {
            bool result = await _imgurhelper.Login(BoxPin.Text);

            if (!result) {
                return;
            }
            DoubleAnimation fadePanelOut = Animations.FadeOut;
            fadePanelOut.Completed += delegate {
                DoubleAnimation fadeBtnIn = Animations.FadeIn;
                fadeBtnIn.Completed += delegate { PanelPin.Visibility = Visibility.Collapsed; };
                BtnSignOut.Visibility = Visibility.Visible;
                BtnSignOut.BeginAnimation(OpacityProperty, fadeBtnIn);
                BtnViewPics.Visibility = Visibility.Visible;
                BtnViewPics.BeginAnimation(OpacityProperty, fadeBtnIn);
            };
            PanelPin.BeginAnimation(OpacityProperty, fadePanelOut);

            if (_imgurhelper.User != null) {
                LabelAccount.Content = string.Format(str.imgurAccSignedIn, _imgurhelper.User);

                BtnSignIn.Visibility = Visibility.Collapsed;
                BtnSignOut.Visibility = Visibility.Visible;
                BtnViewPics.Visibility = Visibility.Visible;
            }
            BoxPin.Clear();
        }

        private void Box_PIN_TextChanged(object sender, TextChangedEventArgs e) {
            BtnPinOk.IsEnabled = BoxPin.Text.Length > 0;
        }

        private void PathBox_Submit(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SavePath();
            }
        }

        private void PathChooser(object sender, RoutedEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (Directory.Exists(PathBox.Text)) {
                fbd.SelectedPath = PathBox.Text;
            }

            fbd.Description = str.selectPath;

            fbd.ShowDialog();

            if (string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                return;
            }
            PathBox.Text = fbd.SelectedPath;
            SavePath();
        }

        private void SavePath() {
            try {
                if (Directory.Exists(PathBox.Text)) {
                    ConfigHelper.SaveImagesPath = PathBox.Text;
                } else {
                    ErrorToast.Show(str.pathNotExist, TimeSpan.FromSeconds(4));
                }
            } catch {
                // ignored
            }
        }

        private void Image_OpenRepository(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.github.com/mrousavy/ImgurSniper");
        }

        private void Image_OpenGitHub(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.github.com/mrousavy");
        }

        //Load config
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Load();
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
