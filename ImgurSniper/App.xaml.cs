using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.Start;
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
        }

        //select startup by command line args
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            CommandlineArgs args = CommandLineHelpers.GetCommandlineArguments();

            switch (args.Argument) {
                case CommandLineHelpers.Argument.Autostart:
                    //Tray with Hotkeys
                    StartTray.Initialize();
                    break;
                case CommandLineHelpers.Argument.GIF:
                    //GIF Recording Capture
                    StartGif.CaptureGif();
                    break;
                case CommandLineHelpers.Argument.Snipe:
                    //Normal Image Capture
                    StartImage.CaptureImage();
                    break;
                case CommandLineHelpers.Argument.Upload:
                    //Context Menu Instant Upload
                    if (args.UploadFiles.Count > 1)
                        StartUpload.UploadMultiple(args.UploadFiles);
                    else
                        StartUpload.UploadSingle(args.UploadFiles[0]);
                    break;
            }
        }


        //Load the config.json
        private static void LoadConfig() {
            ConfigHelper.Exists();
            ConfigHelper.JsonConfig = JsonConvert.DeserializeObject<ConfigHelper.Settings>(File.ReadAllText(ConfigHelper.ConfigFile));
        }

        //Set Language from Settings
        private static void LoadLanguage() {
            string language = ConfigHelper.Language;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }


        //Unhandled Exception User Message Boxes
        private static void UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            if (MessageBox.Show(strings.unhandledError,
                    "Help fixing an ImgurSniper Bug?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes) {
                Process.Start("https://github.com/mrousavy/ImgurSniper/issues/new");
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
                string[] argumentsArray = Environment.GetCommandLineArgs();
                string arguments = argumentsArray.Aggregate("", (current, arg) => current + (arg + " "));
                string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;

                Process.Start(exePath, arguments);
            } catch {
                // ignored
            }

            Process.GetCurrentProcess().Kill();
        }
    }
}