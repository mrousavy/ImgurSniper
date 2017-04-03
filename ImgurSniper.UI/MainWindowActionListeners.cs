using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        private void ImageFormatBoxChanged(object sender, SelectionChangedEventArgs e) {
            ComboBoxItem item = ImageFormatBox.SelectedItem as ComboBoxItem;

            if(item != null)
                switch(item.Content as string) {
                    case "Jpeg":
                        FileIO.ImageFormat = ImageFormat.Jpeg;
                        break;
                    case "Png":
                        FileIO.ImageFormat = ImageFormat.Png;
                        break;
                    case "Gif":
                        FileIO.ImageFormat = ImageFormat.Gif;
                        break;
                    case "Tiff":
                        FileIO.ImageFormat = ImageFormat.Tiff;
                        break;
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


        private void ShowMouseBox_Check(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box == null) {
                return;
            }
            try {
                FileIO.ShowMouse = box.IsChecked == true;
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

        private void AutoUpdate_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if(box == null) {
                return;
            }
            try {
                FileIO.AutoUpdate = box.IsChecked == true;
            } catch { }
        }

        private async void RunOnBoot_Checkbox(object sender, RoutedEventArgs e) {
            if(sender is CheckBox box) {
                FileIO.RunOnBoot = box.IsChecked == true;

                //Run proecess if not running
                try {
                    bool choice = box.IsChecked == true;
                    //Show Dialog on disabling
                    if(box.IsChecked == false) {
                        choice = !await ShowAskDialog(str.disablingTrayWarning);
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
                        foreach(
                            Process proc in Process.GetProcesses().Where(p => p.ProcessName.Contains("ImgurSniper"))) {
                            if(proc.Id != Process.GetCurrentProcess().Id) {
                                proc.Kill();
                            }
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
                    if(proc.Id != Process.GetCurrentProcess().Id) {
                        proc.Kill();
                    }
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

        private void HotkeyImgBoxMDown(object sender, RoutedEventArgs e) {
            TextBox box = sender as TextBox;
            if(box == null) {
                return;
            }

            try {
                HotKeySelector sel = new HotKeySelector();

                try {
                    sel.Owner = this;
                } catch { }

                bool? result = sel.ShowDialog();

                if(result == true) {
                    FileIO.ShortcutImgKey = sel.key;
                    HotkeyImgBox.Text = sel.key.ToString();

                    InstallerHelper.KillImgurSniper(false);
                    InstallerHelper.StartImgurSniper();
                }
            } catch { }
        }

        private void HotkeyGifBoxMDown(object sender, RoutedEventArgs e) {
            TextBox box = sender as TextBox;
            if(box == null) {
                return;
            }

            try {
                HotKeySelector sel = new HotKeySelector();

                try {
                    sel.Owner = this;
                } catch { }

                bool? result = sel.ShowDialog();

                if(result == true) {
                    FileIO.ShortcutGifKey = sel.key;
                    HotkeyGifBox.Text = sel.key.ToString();

                    InstallerHelper.KillImgurSniper(false);
                    InstallerHelper.StartImgurSniper();
                }
            } catch { }
        }

        private async void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(sender is ComboBox box) {
                try {
                    FileIO.Language = (box.SelectedItem as ComboBoxItem).Name;

                    bool result = await ShowAskDialog(str.langChanged);

                    if(result) {
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
            if(sender is Slider slider) {
                int value = (int)slider.Value;
                FileIO.GifLength = value * 1000;

                GifLengthLabel.Content = string.Format(str.gifLengthVal, value);
            }
        }

        private void SliderGifFps_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if(sender is Slider slider) {
                int value = (int)slider.Value;
                FileIO.GifFps = value;

                GifFpsLabel.Content = string.Format(str.gifFpsVal, value);
            }
        }

        private void Help(object sender, RoutedEventArgs e) {
            //Process.Start("http://github.com/mrousavy/ImgurSniper#features");
            Help help = new Help();

            try {
                help.Owner = this;
            } catch { }

            help.Show();
        }

        private async void Snipe(object sender, RoutedEventArgs e) {
            string exe = System.IO.Path.Combine(Path, "ImgurSniper.exe");

            if(File.Exists(exe)) {
                Process snipeProc = new Process { StartInfo = new ProcessStartInfo(exe) };
                snipeProc.Start();

                Visibility = Visibility.Hidden;

                await Task.Delay(500);
                snipeProc.WaitForExit();

                Visibility = Visibility.Visible;
            } else {
                error_toast.Show(str.imgurSniperNotFound,
                    TimeSpan.FromSeconds(3));
            }
        }

        private void Update(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);

            DoubleAnimation darken = Animations.GetDarkenAnimation(Opacity);
            DoubleAnimation brighten = Animations.GetBrightenAnimation(Opacity);

            VersionInfo info = new VersionInfo(_commits, FileIO.CurrentCommits);

            try {
                info.Owner = this;
            } catch {
                // ignored
            }

            darken.Completed += delegate {
                bool? result = info.ShowDialog();

                if(result == true) {
                    FileIO.CurrentCommits = _commits.Count;
                    FileIO.UpdateAvailable = false;

                    StackPanel panel = ShowProgressDialog();
                    Helper.Update(panel);
                } else {
                    ChangeButtonState(true);

                    if(info.skipped) {
                        Btn_Update.IsEnabled = false;
                    }
                }

                BeginAnimation(OpacityProperty, brighten);
            };

            BeginAnimation(OpacityProperty, darken);
        }


        private async void Btn_SearchUpdates(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if(btn != null) {
                btn.IsEnabled = false;
            }

            //Show Progress Indicator
            progressIndicator.BeginAnimation(OpacityProperty, Animations.FadeIn);

            await CheckForUpdates(true);

            //Hide Progress Indicator
            progressIndicator.BeginAnimation(OpacityProperty, Animations.FadeOut);

            if(btn != null) {
                btn.IsEnabled = true;
            }
        }

        private void SignIn(object sender, RoutedEventArgs e) {
            try {
                _imgurhelper.Authorize();

                DoubleAnimation fadeBtnOut = Animations.FadeOut;
                fadeBtnOut.Completed += delegate {
                    DoubleAnimation fadePanelIn = Animations.FadeIn;
                    fadePanelIn.Completed += delegate { Btn_SignIn.Visibility = Visibility.Collapsed; };
                    Panel_PIN.Visibility = Visibility.Visible;
                    Panel_PIN.BeginAnimation(OpacityProperty, fadePanelIn);
                };
                Btn_SignIn.BeginAnimation(OpacityProperty, fadeBtnOut);
            } catch { }
        }

        private void SignOut(object sender, RoutedEventArgs e) {
            DoubleAnimation fadeBtnOut = Animations.FadeOut;
            Btn_ViewPics.BeginAnimation(OpacityProperty, fadeBtnOut);

            fadeBtnOut.Completed += delegate {
                FileIO.DeleteToken();

                DoubleAnimation fadeBtnIn = Animations.FadeIn;
                Btn_ViewPics.BeginAnimation(OpacityProperty, fadeBtnIn);

                fadeBtnIn.Completed += delegate {
                    Btn_SignOut.Visibility = Visibility.Collapsed;
                    Btn_ViewPics.Visibility = Visibility.Collapsed;

                    Label_Account.Content = "Imgur Account";
                };
                Btn_SignIn.Visibility = Visibility.Visible;
                Btn_SignIn.BeginAnimation(OpacityProperty, fadeBtnIn);
            };
            Btn_SignOut.BeginAnimation(OpacityProperty, fadeBtnOut);
        }

        private void ViewPics(object sender, RoutedEventArgs e) {
            Process.Start(_imgurhelper.UserUrl);
        }

        private async void PINOk(object sender, RoutedEventArgs e) {
            bool result = await _imgurhelper.Login(Box_PIN.Text);

            if(!result) {
                return;
            }
            DoubleAnimation fadePanelOut = Animations.FadeOut;
            fadePanelOut.Completed += delegate {
                DoubleAnimation fadeBtnIn = Animations.FadeIn;
                fadeBtnIn.Completed += delegate { Panel_PIN.Visibility = Visibility.Collapsed; };
                Btn_SignOut.Visibility = Visibility.Visible;
                Btn_SignOut.BeginAnimation(OpacityProperty, fadeBtnIn);
                Btn_ViewPics.Visibility = Visibility.Visible;
                Btn_ViewPics.BeginAnimation(OpacityProperty, fadeBtnIn);
            };
            Panel_PIN.BeginAnimation(OpacityProperty, fadePanelOut);

            if(_imgurhelper.User != null) {
                Label_Account.Content = string.Format(str.imgurAccSignedIn, _imgurhelper.User);

                Btn_SignIn.Visibility = Visibility.Collapsed;
                Btn_SignOut.Visibility = Visibility.Visible;
                Btn_ViewPics.Visibility = Visibility.Visible;
            }
            Box_PIN.Clear();
        }

        private void Box_PIN_TextChanged(object sender, TextChangedEventArgs e) {
            Btn_PinOk.IsEnabled = Box_PIN.Text.Length > 0;
        }

        private void PathBox_Submit(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                SavePath();
            }
        }

        private void PathChooser(object sender, RoutedEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if(Directory.Exists(PathBox.Text)) {
                fbd.SelectedPath = PathBox.Text;
            }

            fbd.Description = str.selectPath;

            DialogResult result = fbd.ShowDialog();

            if(string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                return;
            }
            PathBox.Text = fbd.SelectedPath;
            SavePath();
        }

        private void SavePath() {
            try {
                if(Directory.Exists(PathBox.Text)) {
                    FileIO.SaveImagesPath = PathBox.Text;
                } else {
                    error_toast.Show(str.pathNotExist, TimeSpan.FromSeconds(4));
                }
            } catch { }
        }

        private void Image_OpenRepository(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.github.com/mrousavy/ImgurSniper");
        }

        private void Image_OpenGitHub(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.github.com/mrousavy");
        }


        //TODO: Custom Title Bar (Mac OSX Title bar?)
        //private async void Window_Maximize(object sender, MouseButtonEventArgs e) {
        //    if(WindowState == WindowState.Normal) {
        //        WindowState = WindowState.Maximized;
        //    } else {
        //        WindowState = WindowState.Normal;
        //    }

        //    WindowStyle = WindowStyle.SingleBorderWindow;
        //}
        //private void Window_Minimize(object sender, MouseButtonEventArgs e) {
        //    WindowStyle = WindowStyle.SingleBorderWindow;
        //    WindowState = WindowState.Minimized;
        //}
        //private void Window_Close(object sender, MouseButtonEventArgs e) {
        //    Close();
        //}
        //private void Window_Move(object sender, MouseButtonEventArgs e) {
        //    DragMove();
        //}

        #endregion
    }
}