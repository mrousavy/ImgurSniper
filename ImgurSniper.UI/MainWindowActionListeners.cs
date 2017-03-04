using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using str = ImgurSniper.UI.Properties.strings;


namespace ImgurSniper.UI {
    public partial class MainWindow {
        #region Action Listeners
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Closing -= WindowClosing;

            DoubleAnimation fadingAnimation = new DoubleAnimation {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.24)),
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

        private void HotkeyBoxMDown(object sender, RoutedEventArgs e) {
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
                    FileIO.ShortcutKey = sel.key;
                    HotkeyBox.Text = sel.key.ToString();
                }
            } catch { }
        }

        private async void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox box = sender as ComboBox;
            if(box != null) {
                try {
                    FileIO.Language = (box.SelectedItem as ComboBoxItem).Name;

                    bool result = await ShowAskDialog(str.langChanged);

                    if(result) {
                        InstallerHelper.KillImgurSniper(false);
                        Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                        System.Windows.Application.Current.Shutdown();
                    }
                } catch {
                    box.SelectedIndex = 0;
                }
            }
        }

        private void Help(object sender, RoutedEventArgs e) {
            //Process.Start("http://github.com/mrousavy/ImgurSniper#features");
            Help help = new Help();

            try {
                help.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                help.Owner = this;
            } catch { }

            help.Show();
            help.Activate();
            help.Focus();
            help.BringIntoView();
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

        private void Update(object sender, RoutedEventArgs e) {
            ChangeButtonState(false);
            VersionInfo info = new VersionInfo(_commits, FileIO.CurrentCommits);

            try {
                info.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                info.Owner = this;
            } catch { }

            bool? result = info.ShowDialog();

            if(result == true) {
                FileIO.CurrentCommits = _commits.Count;
                FileIO.UpdateAvailable = false;

                StackPanel panel = ShowProgressDialog();
                Helper.Update(panel);
            } else {
                ChangeButtonState(true);
            }
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
    }
}
