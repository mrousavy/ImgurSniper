using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() : base() {
            this.DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs e) => {
                if(MessageBox.Show("An unknown Error occured in ImgurSniper.UI!\nImgurSniper has to shut down!\nWould you like to see a detailed Error Information?",
                    "ImgurSniper Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error) == MessageBoxResult.Yes) {

                    MessageBox.Show(
                        "Message: " + e.Exception.Message + "\n\n" +
                        "Source: " + e.Exception.Source + "\n\n" +
                        "InnerException Message:" + e.Exception.InnerException.Message + "\n\n" +
                        "Stacktrace: " + e.Exception.StackTrace,
                        "ImgurSniper Exception Details");
                }

                Process.GetCurrentProcess().Kill();
            };
        }
    }
}
