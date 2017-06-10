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
        private static void UpdateCheck() {
#if !DEBUG
            try {
                //If automatically update, and last checked was more than 2 days ago
                if (ConfigHelper.AutoUpdate && (DateTime.Now - ConfigHelper.LastChecked) > TimeSpan.FromDays(2))
                    Process.Start(Path.Combine(ConfigHelper.InstallDir, "ImgurSniper.UI.exe"), "Update");
            } catch {
                // ignored
            }
#endif
        }

        //Load the config.json
        private static void LoadConfig() {
            ConfigHelper.Exists();
            ConfigHelper.JsonConfig = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ConfigHelper.ConfigFile));
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
            Helpers.WriteError(e.Exception);
            if (MessageBox.Show(string.Format(strings.unhandledErrorDescription, e.Exception.Message),
                "ImgurSniper Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error) == MessageBoxResult.Yes) {
                try {
                    //Restart with same args
                    string[] argumentsArray = Environment.GetCommandLineArgs();
                    string arguments = argumentsArray.Aggregate("", (current, arg) => current + (arg.Contains(" ") ? "\"" + arg + "\" " : arg + " "));
                    string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;

                    Process.Start(exePath, arguments);
                } catch {
                    // ignored
                }
            } else {
                Process.GetCurrentProcess().Kill();
                return;
            }
        }
    }
}
