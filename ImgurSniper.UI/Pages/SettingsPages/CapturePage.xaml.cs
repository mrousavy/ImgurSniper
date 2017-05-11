using ImgurSniper.UI.Properties;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;

namespace ImgurSniper.UI.Pages.SettingsPages {
    /// <summary>
    /// Interaction logic for CapturePage.xaml
    /// </summary>
    public partial class CapturePage {
        private readonly MainWindow _window;


        public CapturePage() {
            InitializeComponent();

            _window = Application.Current.MainWindow as MainWindow;

            Load();
        }

        //Load all Config Params
        private async void Load() {
            _window?.ShowProgressIndicator();

            //Update Loading Indicator
            _window?.SetProgressStatus(strings.loadConf);

            try {
                Settings settings = ConfigHelper.JsonConfig;

                bool showMouse = settings.ShowMouse;
                bool freezeScreen = settings.FreezeScreen;
                int gifFps = settings.GifFps;
                int gifLength = settings.GifLength / 1000;
                long compression = settings.Quality;
                ImageFormat format = settings.ImageFormat;

                //Image Format
                switch (format.ToString()) {
                    case "Jpeg":
                        ImageFormatBox.SelectedIndex = 0;
                        break;
                    case "Png":
                        ImageFormatBox.SelectedIndex = 1;
                        break;
                    case "Gif":
                        ImageFormatBox.SelectedIndex = 2;
                        break;
                    case "Tiff":
                        ImageFormatBox.SelectedIndex = 3;
                        break;
                }
                ImageFormatBox.SelectionChanged += ImageFormatBoxChanged;

                //Show Mouse Cursor on Image Capture
                ShowMouseBox.IsChecked = showMouse;

                //Show Mouse Cursor on Image Capture
                FreezeScreenBox.IsChecked = freezeScreen;

                //Set GIF FPS
                GifFpsSlider.Value = gifFps;
                GifFpsSlider.ValueChanged += SliderGifFps_Changed;
                GifFpsLabel.Content = string.Format(strings.gifFpsVal, gifFps);

                //Set GIF Length in Seconds
                GifLengthSlider.Value = gifLength;
                GifLengthSlider.ValueChanged += SliderGifLength_Changed;
                GifLengthLabel.Content = string.Format(strings.gifLengthVal, gifLength);

                //Set Quality in %
                QualitySlider.Value = compression;
                QualitySlider.ValueChanged += QualitySlider_ValueChanged;
                QualityLabel.Content = compression + "%";
            } catch {
                await Dialog.ShowOkDialog(strings.couldNotLoad, string.Format(strings.errorConfig, ConfigHelper.ConfigPath));
            }

            //Remove Loading Indicator
            _window?.HideProgressIndicator();
        }


        #region UI
        private void ShowMouseBox_Check(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            ConfigHelper.ShowMouse = box.IsChecked == true;
            EnableSave();
        }
        private void FreezeScreenBox_Check(object sender, RoutedEventArgs e) {
            CheckBox box = sender as CheckBox;
            if (box == null) {
                return;
            }
            ConfigHelper.FreezeScreen = box.IsChecked == true;
            EnableSave();
        }
        private void ImageFormatBoxChanged(object sender, SelectionChangedEventArgs e) {
            if (ImageFormatBox.SelectedItem is ComboBoxItem item) {
                switch ((string)item.Content) {
                    case "Png":
                        ConfigHelper.ImageFormat = ImageFormat.Png;
                        break;
                    case "Jpeg":
                        ConfigHelper.ImageFormat = ImageFormat.Png;
                        break;
                    case "Bmp":
                        ConfigHelper.ImageFormat = ImageFormat.Png;
                        break;
                    case "Tiff":
                        ConfigHelper.ImageFormat = ImageFormat.Png;
                        break;
                    case "Gif":
                        ConfigHelper.ImageFormat = ImageFormat.Png;
                        break;
                }

                EnableSave();
            }
        }

        private void SliderGifLength_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (sender is Slider slider) {
                int value = (int)slider.Value;
                ConfigHelper.GifLength = value * 1000;
                EnableSave();

                GifLengthLabel.Content = string.Format(strings.gifLengthVal, value);
            }
        }

        private void SliderGifFps_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (sender is Slider slider) {
                int value = (int)slider.Value;
                ConfigHelper.GifFps = value;
                EnableSave();

                GifFpsLabel.Content = string.Format(strings.gifFpsVal, value);
            }
        }

        private void QualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (sender is Slider slider) {
                byte value = (byte)slider.Value;
                ConfigHelper.Quality = value;
                EnableSave();

                QualityLabel.Content = value + "%";
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
