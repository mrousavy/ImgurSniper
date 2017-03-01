using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        public App() {
            DispatcherUnhandledException += (sender, e) => {
                if(MessageBox.Show($"{ImgurSniper.Properties.strings.unhandledError}({e.Exception.Message})",
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
