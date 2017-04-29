using ImgurSniper.UI.Properties;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace ImgurSniper.UI {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        public App() {
            DispatcherUnhandledException += UnhandledException;

            LoadConfig();

            LoadLanguage();
        }

        private static void LoadLanguage() {
            string language = ConfigHelper.Language;

            //If language is not yet set manually, select system default
            if (!string.IsNullOrWhiteSpace(language)) {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            } else {
                ConfigHelper.Language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                ConfigHelper.Save();
            }
        }

        //Load from config.json
        private static void LoadConfig() {
            ConfigHelper.Exists();
            ConfigHelper.JsonConfig = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ConfigHelper.ConfigFile));
        }

        //Unhandled Exception User Message Boxes
        private static void UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            MessageBoxResult helpFixing = MessageBox.Show(strings.unhandledError,
                "Help fixing an ImgurSniper Bug?",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (helpFixing == MessageBoxResult.Yes) {
                Process.Start("https://github.com/mrousavy/ImgurSniper/issues/new");
            } else if (helpFixing == MessageBoxResult.Cancel) {
                Process.GetCurrentProcess().Kill();
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

                Process.Start(Assembly.GetCallingAssembly().Location, arguments);
            } catch {
                // ignored
            }
        }
    }
}