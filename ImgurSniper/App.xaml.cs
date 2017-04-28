using ImgurSniper.Libraries.Helper;
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
            DispatcherUnhandledException += UnhandledException;

            LoadConfig();

            LoadLanguage();

            UpdateCheck();
        }

        //Check for Updates
        private void UpdateCheck() {
#if !DEBUG
            try {
                //If automatically update, and last checked was more than 2 days ago
                if (ConfigHelper.AutoUpdate && (DateTime.Now - ConfigHelper.LastChecked) > TimeSpan.FromDays(2))
                    Process.Start(Path.Combine(ConfigHelper.InstallDir, "ImgurSniper.UI.exe"), "Update");
            } catch { }
#endif
        }

        //Load the config.json
        private static void LoadConfig() {
            ConfigHelper.Exists();
            ConfigHelper.JsonConfig = JsonConvert.DeserializeObject<ConfigHelper.Settings>(File.ReadAllText(ConfigHelper.ConfigFile));
        }

        //Set Language from Settings
        private static void LoadLanguage() {
            string language = ConfigHelper.Language;
            if (string.IsNullOrWhiteSpace(language))
                return;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
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


            if (MessageBox.Show(string.Format(strings.unhandledErrorDescription, e.Exception.Message),
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
                //Restart with same args
                string[] argumentsArray = Environment.GetCommandLineArgs();
                string arguments = argumentsArray.Aggregate("", (current, arg) => current + (arg.Contains(" ") ? "\"" + arg + "\" " : arg + " "));
                string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;

                Process.Start(exePath, arguments);
            } catch {
                // ignored
            }

            Process.GetCurrentProcess().Kill();
        }
    }
}