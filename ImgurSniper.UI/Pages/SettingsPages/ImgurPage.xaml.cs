using ImgurSniper.UI.Properties;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ImgurSniper.UI.Pages.SettingsPages {
    /// <summary>
    /// Interaction logic for ImgurPage.xaml
    /// </summary>
    public partial class ImgurPage {
        private readonly MainWindow _window;
        private readonly ImgurLoginHelper _imgurhelper;

        public ImgurPage() {
            InitializeComponent();

            _window = (MainWindow)Application.Current.MainWindow;
            _imgurhelper = new ImgurLoginHelper(_window?.ErrorToast, _window?.SuccessToast);

            Load();
        }

        //Load all Config Params
        private async void Load() {
            _window?.ShowProgressIndicator();

            //Update Loading Indicator
            _window?.SetProgressStatus(strings.contactImgur);

            try {
                string refreshToken = ConfigHelper.ReadRefreshToken();
                if (refreshToken != null) {
                    //name = null if refreshToken = null or any error occured in Login
                    string name = await _imgurhelper.LoggedInUser(refreshToken);

                    if (name != null) {
                        LabelAccount.Content = string.Format(strings.imgurAccSignedIn, name);

                        PinInputPanel.Visibility = Visibility.Collapsed;
                        BtnSignIn.Visibility = Visibility.Collapsed;
                        MyAccountPanel.Visibility = Visibility.Visible;
                    }
                } else {
                    PinInputPanel.Visibility = Visibility.Collapsed;
                    BtnSignIn.Visibility = Visibility.Visible;
                    MyAccountPanel.Visibility = Visibility.Collapsed;
                }
            } catch {
                PinInputPanel.Visibility = Visibility.Collapsed;
                BtnSignIn.Visibility = Visibility.Visible;
                MyAccountPanel.Visibility = Visibility.Collapsed;
                await Dialog.ShowOkDialog(strings.couldNotLoad, string.Format(strings.errorConfig, ConfigHelper.ConfigPath));
            }

            //Remove Loading Indicator
            _window?.HideProgressIndicator();
        }


        #region UI
        private void Box_PIN_TextChanged(object sender, TextChangedEventArgs e) {
            BtnPinOk.IsEnabled = BoxPin.Text.Length > 0;
        }

        private async void PinOk(object sender, RoutedEventArgs e) {
            bool result = await _imgurhelper.Login(BoxPin.Text);

            if (!result) {
                return;
            }

            await PinInputPanel.AnimateAsync(OpacityProperty, Animations.FadeOut);
            PinInputPanel.Visibility = Visibility.Collapsed;
            MyAccountPanel.Visibility = Visibility.Visible;
            await MyAccountPanel.AnimateAsync(OpacityProperty, Animations.FadeIn);

            if (_imgurhelper.User != null) {
                LabelAccount.Content = string.Format(strings.imgurAccSignedIn, _imgurhelper.User);

                BtnSignIn.Visibility = Visibility.Collapsed;
            }
            BoxPin.Clear();
        }

        private async void SignIn(object sender, RoutedEventArgs e) {
            try {
                _imgurhelper.Authorize();

                await BtnSignIn.AnimateAsync(OpacityProperty, Animations.FadeOut);
                BtnSignIn.Visibility = Visibility.Collapsed;
                PinInputPanel.Visibility = Visibility.Visible;
                await PinInputPanel.AnimateAsync(OpacityProperty, Animations.FadeIn);
            } catch {
                // ignored
            }
        }

        private async void SignOut(object sender, RoutedEventArgs e) {
            ConfigHelper.DeleteToken();

            await MyAccountPanel.AnimateAsync(OpacityProperty, Animations.FadeOut);
            MyAccountPanel.Visibility = Visibility.Collapsed;
            BtnSignIn.Visibility = Visibility.Visible;
            await BtnSignIn.AnimateAsync(OpacityProperty, Animations.FadeIn);

            LabelAccount.Content = strings.imgurAcc;
        }

        private void ViewPics(object sender, RoutedEventArgs e) {
            Process.Start(_imgurhelper.UserUrl);
        }
        #endregion
    }
}
