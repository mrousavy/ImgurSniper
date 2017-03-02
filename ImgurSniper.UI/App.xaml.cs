using ImgurSniper.UI.Properties;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace ImgurSniper.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        public App() {
            DispatcherUnhandledException += (s, e) => {
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

            IsInstaller();

            string language = FileIO.Language;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }




        private void IsInstaller() {
            //Restard if Argument "Installer" is passed (From CustomActions)
            //(Because Installer will wait for Process Exit)
            string[] args = Environment.GetCommandLineArgs();
            if(args.Length > 0 && args.Contains("Installer")) {
                string fileName = System.Reflection.Assembly.GetEntryAssembly().Location;
                if(fileName != null)
                    Process.Start(fileName);

                System.Windows.Application.Current.Shutdown(0);
            }
        }
    }
}
