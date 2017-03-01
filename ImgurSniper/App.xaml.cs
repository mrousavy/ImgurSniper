using System.Diagnostics;
using System.Windows;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        public App() {
            DispatcherUnhandledException += (sender, e) => {
                if(MessageBox.Show($"An unknown Error occured in ImgurSniper.UI!\nImgurSniper has to shut down!\nWould you like to see a detailed Exception Info?\n\n({e.Exception.Message})",
                    "ImgurSniper Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error) == MessageBoxResult.Yes) {

                    MessageBox.Show(
                        "Message: " + e.Exception.Message + "\n\n" +
                        "Source: " + e.Exception.Source + "\n\n" +
                        "Stacktrace: " + e.Exception.StackTrace,
                        "ImgurSniper Exception Details");
                }

                Process.GetCurrentProcess().Kill();
            };
        }
    }
}
