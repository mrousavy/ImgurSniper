using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using ImgurSniper.Properties;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        public App() {
            DispatcherUnhandledException += (sender, e) => {
                if (MessageBox.Show($"{strings.unhandledError}({e.Exception.Message})",
                        "ImgurSniper Error",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error) == MessageBoxResult.Yes) {
                    if (MessageBox.Show("Do you want to help out and fix a bug in ImgurSniper?" + "\n" +
                                        "Please explain how the Problem you encountered can be replicated, and what the Error Message said!",
                            "Do you want to help ImgurSniper bugfixing?",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes) {
                        Process.Start("https://github.com/mrousavy/ImgurSniper/issues/new");
                    }

                    MessageBox.Show(
                        "|||Base Message: " + e.Exception.GetBaseException().Message + "\n\n" +
                        "|||Message: " + e.Exception.Message + "\n\n" +
                        "|||Source: " + e.Exception.Source + "\n\n" +
                        "|||Stacktrace: " + e.Exception.StackTrace,
                        "ImgurSniper Exception - More Details",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
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