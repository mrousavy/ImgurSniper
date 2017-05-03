using System;
using ImgurSniper.UI.Properties;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using RadioButton = System.Windows.Controls.RadioButton;

namespace ImgurSniper.UI.Pages.SettingsPages {
    /// <summary>
    /// Interaction logic for EventsPage.xaml
    /// </summary>
    public partial class EventsPage {
        private readonly MainWindow _window;

        public EventsPage() {
            InitializeComponent();

            _window = (MainWindow)Application.Current.MainWindow;

            Load();
        }

        //Load all Config Params
        private async void Load() {
            _window?.ShowProgressIndicator();

            //Update Loading Indicator
            _window?.SetProgressStatus(strings.loadConf);

            try {
                Settings settings = ConfigHelper.JsonConfig;

                bool openBrowserAfterUpload = settings.OpenBrowserAfterUpload;
                bool openFileAfterSnap = settings.OpenFileAfterSnap;
                bool saveImages = settings.SaveImages;
                AfterSnipe action = settings.AfterSnipeAction;
                string saveImagesPath = settings.SaveImagesPath;


                //Save Images on Snap
                SaveBox.IsChecked = saveImages;
                if (saveImages) {
                    PathPanel.IsEnabled = true;
                    OpenFileAfterSnapBox.IsEnabled = true;
                }

                //Path to Saved Images
                if (string.IsNullOrWhiteSpace(saveImagesPath)) {
                    PathBox.Text = ConfigHelper.ConfigPath;
                } else {
                    //Create Pictures\ImgurSniperImages Path
                    try {
                        if (!Directory.Exists(saveImagesPath)) {
                            Directory.CreateDirectory(saveImagesPath);
                        }
                        PathBox.Text = saveImagesPath;
                    } catch {
                        PathBox.Text = "";
                    }
                }

                //Open Image in Browser after upload
                OpenBrowserAfterUploadBox.IsChecked = openBrowserAfterUpload;

                //Open Image in Windows Explorer after snap
                OpenFileAfterSnapBox.IsChecked = openFileAfterSnap;

                //Upload to Imgur or Copy to Clipboard after Snipe
                switch (action) {
                    case AfterSnipe.CopyClipboard:
                        ClipboardRadio.IsChecked = true;
                        OpenBrowserAfterUploadBox.IsEnabled = false;
                        break;
                    case AfterSnipe.DoNothing:
                        DoNothingRadio.IsChecked = true;
                        OpenBrowserAfterUploadBox.IsEnabled = false;
                        break;
                    case AfterSnipe.UploadImgur:
                        ImgurRadio.IsChecked = true;
                        OpenBrowserAfterUploadBox.IsEnabled = true;
                        break;
                }
            } catch {
                await Dialog.ShowOkDialog(strings.couldNotLoad, string.Format(strings.errorConfig, ConfigHelper.ConfigPath));
            }

            //Remove Loading Indicator
            _window?.HideProgressIndicator();
        }

        #region UI
        private void AfterSnapClick(object sender, RoutedEventArgs e) {
            RadioButton button = sender as RadioButton;
            if (button == null) {
                return;
            }

            AfterSnipe action = (AfterSnipe)Enum.Parse(typeof(AfterSnipe), (string)button.Tag);

            switch (action) {
                case AfterSnipe.CopyClipboard:
                case AfterSnipe.DoNothing:
                    OpenBrowserAfterUploadBox.IsEnabled = false;
                    break;
                case AfterSnipe.UploadImgur:
                    OpenBrowserAfterUploadBox.IsEnabled = true;
                    break;
            }

            ConfigHelper.AfterSnipeAction = action;

            EnableSave();
        }
        private void OpenBrowserAfterUpload_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.OpenBrowserAfterUpload = box.IsChecked == true;
                EnableSave();
            } catch {
                // ignored
            }
        }
        private void OpenFileAfterSnap_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            try {
                ConfigHelper.OpenFileAfterSnap = box.IsChecked == true;
                EnableSave();
            } catch {
                // ignored
            }
        }

        private void SaveImgs_Checkbox(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            ConfigHelper.SaveImages = box.IsChecked == true;
            EnableSave();

            if (box.IsChecked.HasValue) {
                PathPanel.IsEnabled = (bool)box.IsChecked;
                OpenFileAfterSnapBox.IsEnabled = (bool)box.IsChecked;
            }
        }
        private void PathChooser(object sender, RoutedEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (Directory.Exists(PathBox.Text)) {
                fbd.SelectedPath = PathBox.Text;
            }

            fbd.Description = strings.selectPath;

            fbd.ShowDialog();

            if (string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                return;
            }
            PathBox.Text = fbd.SelectedPath;
            SavePath();
        }
        private void PathBox_Submit(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SavePath();
            }
        }
        private async void SavePath() {
            try {
                if (Directory.Exists(PathBox.Text)) {
                    ConfigHelper.SaveImagesPath = PathBox.Text;
                } else {
                    await Dialog.ShowOkDialog(strings.error, strings.pathNotExist);
                }
            } catch {
                // ignored
            }
        }
        #endregion

        private void EnableSave() {
            try {
                _window?.EnableSave();
            } catch {
                // no parent found
            }
        }
    }
}
