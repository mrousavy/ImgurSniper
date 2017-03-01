using ImgurSniper.UI.Properties;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace ImgurSniper.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public App() : base() {
            DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs e) => {
                if(MessageBox.Show($"{strings.unhandledError}({e.Exception.Message})",
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

            string language = FileIO.Language;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }
}
