using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.Start;
using System.Windows;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for EntryWindow.xaml
    /// </summary>
    public partial class EntryWindow : Window {
        public EntryWindow() {
            StartWithArguments();
        }

        private async void StartWithArguments() {
            CommandlineArgs args = CommandLineHelpers.GetCommandlineArguments();

            switch (args.Argument) {
                case CommandLineHelpers.Argument.Autostart:
                    //Tray with Hotkeys
                    await StartTray.Initialize();
                    break;
                case CommandLineHelpers.Argument.GIF:
                    //GIF Recording Capture
                    new GifWindow().ShowDialog();
                    break;
                case CommandLineHelpers.Argument.Snipe:
                    //Normal Image Capture
                    new ScreenshotWindow().ShowDialog();
                    break;
                case CommandLineHelpers.Argument.Upload:
                    //Context Menu Instant Upload
                    if (args.UploadFiles.Count > 1)
                        await StartUpload.UploadMultiple(args.UploadFiles);
                    else
                        await StartUpload.UploadSingle(args.UploadFiles[0]);
                    break;
            }
            Application.Current.Shutdown();
        }
    }
}
