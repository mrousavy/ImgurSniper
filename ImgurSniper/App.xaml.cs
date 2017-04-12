using ImgurSniper.Properties;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        public App() {
            LoadConfig();

            DispatcherUnhandledException += UnhandledException;

            LoadLanguage();
        }


        //Load the config.json
        private void LoadConfig() {
            FileIO.Exists();
            FileIO.JsonConfig = JsonConvert.DeserializeObject<FileIO.Settings>(File.ReadAllText(FileIO.ConfigFile));
        }

        //Set Language from Settings
        private void LoadLanguage() {
            string language = FileIO.Language;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }


        //Unhandled Exception User Message Boxes
        private void UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            if (MessageBox.Show(strings.unhandledError,
                    "Help fixing an ImgurSniper Bug?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes) {
                Process.Start("https://github.com/mrousavy/ImgurSniper/issues/new");
            }


            if (MessageBox.Show(String.Format(strings.unhandledErrorDescription, e.Exception.Message),
                "ImgurSniper Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error) == MessageBoxResult.Yes) {

                MessageBox.Show(
                    "||| Base Message: " + e.Exception.GetBaseException().Message + "\n\r\n\r" +
                    "||| Message: " + e.Exception.Message + "\n\r\n\r" +
                    "||| Source: " + e.Exception.Source + "\n\r\n\r" +
                    "||| Stacktrace: " + e.Exception.StackTrace,
                    "ImgurSniper Exception - More Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            try {
                string[] argumentsArray = Environment.GetCommandLineArgs();
                string arguments = argumentsArray.Aggregate("", (current, arg) => current + (arg + " "));
                string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;

                Process.Start(exePath, arguments);
            } catch { }

            Process.GetCurrentProcess().Kill();
        }
    }
}