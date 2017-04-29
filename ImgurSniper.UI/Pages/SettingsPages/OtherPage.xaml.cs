using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImgurSniper.UI.Properties;
using Application = System.Windows.Application;

namespace ImgurSniper.UI.Pages.SettingsPages {
    /// <summary>
    /// Interaction logic for OtherPage.xaml
    /// </summary>
    public partial class OtherPage {
        private readonly MainWindow _window;

        public OtherPage() {
            InitializeComponent();

            _window = Application.Current.MainWindow as MainWindow;
            Load();
        }

        private async void Load() {
            _window.ShowProgressIndicator();

            //Update Loading Indicator
            _window.SetProgressStatus(strings.loadConf);

            try {
                Settings settings = ConfigHelper.JsonConfig;

                bool usePrint = settings.UsePrint;
                bool runOnBoot = settings.RunOnBoot;
                bool autoUpdate = settings.AutoUpdate;
                string language = settings.Language;
                Key imgKey = settings.ShortcutImgKey;
                Key gifKey = settings.ShortcutGifKey;

                //Use Print Key instead of default Shortcut
                PrintKeyBox.IsChecked = usePrint;

                //Run ImgurSniper on boot
                if (runOnBoot) {
                    RunOnBoot.IsChecked = true;
                    InstallerHelper.Autostart(true);
                }

                //Auto search for Updates
                if (autoUpdate) {
                    AutoUpdateBox.IsChecked = true;
                }

                //Set correct Language for Current Language Box
                switch (language) {
                    case "en":
                        LanguageBox.SelectedItem = En;
                        break;
                    case "de":
                        LanguageBox.SelectedItem = De;
                        break;
                }
                LanguageBox.SelectionChanged += LanguageBox_SelectionChanged;

                //GIF Hotkey
                HotkeyGifBox.Text = gifKey.ToString();

                //Image Hotkey
                HotkeyImgBox.Text = imgKey.ToString();
            } catch {
                await Dialog.ShowOkDialog(strings.couldNotLoad,
                    string.Format(strings.errorConfig, ConfigHelper.ConfigPath));
            }

            //Search for Updates
            _window.SetProgressStatus(strings.checkingUpdate);
            BtnUpdate.IsEnabled = await InstallerHelper.CheckForUpdates(_window, false);

            //Remove Loading Indicator
            _window.HideProgressIndicator();
        }


        #region UI
        private void Update(object sender, RoutedEventArgs e) { _window.Update(sender, e); }

        private async void RunOnBoot_Checkbox(object sender, RoutedEventArgs e) {
            if (sender is CheckBox box) {
                ConfigHelper.RunOnBoot = box.IsChecked == true;
                EnableSave();

                //Run proecess if not running
                try {
                    bool choice = box.IsChecked == true;
                    //Show Dialog on disabling
                    if (box.IsChecked == false) {
                        choice = !await Dialog.ShowAskDialog(strings.disablingTrayWarning);
                        RunOnBoot.IsChecked = choice;
                    }

                    InstallerHelper.Autostart(choice);
                } catch {
                    await Dialog.ShowOkDialog(strings.error, strings.trayServiceStartFail);
                }
            }
        }
        private async void Btn_SearchUpdates(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn != null) {
                btn.IsEnabled = false;
            }

            //Show Progress Indicator
            _window.ShowProgressIndicator();

            BtnUpdate.IsEnabled = await InstallerHelper.CheckForUpdates(_window, true);

            //Hide Progress Indicator
            _window.HideProgressIndicator();

            if (btn != null) {
                btn.IsEnabled = true;
            }
        }

        private void Magnifying_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.MagnifyingGlassEnabled = box.IsChecked == true;
                EnableSave();
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
                EnableSave();
            } catch {
                // ignored
            }
        }

        private void PrintKeyBox_Click(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.UsePrint = box.IsChecked == true;
                EnableSave();
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
                        sel.Owner = _window;
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
                EnableSave();
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
                        sel.Owner = _window;
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
                EnableSave();
                HotkeyGifBox.Text = selectedKey.ToString();
            } catch {
                // ignored
            }
        }

        private async void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sender is ComboBox box) {
                try {
                    ConfigHelper.Language = ((ComboBoxItem)box.SelectedItem).Name.ToLower();

                    bool result = await Dialog.ShowAskDialog(strings.langChanged);

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
        #endregion

        private void EnableSave() {
            try {
                _window.EnableSave();
            } catch {
                // no parent found
            }
        }
    }
}
