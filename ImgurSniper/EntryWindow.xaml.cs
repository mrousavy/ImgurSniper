using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.Start;
using System.Windows;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for EntryWindow.xaml
    /// </summary>
    public partial class EntryWindow {
        public EntryWindow() {
            Hide();

            StartWithArguments();
        }

        private async void StartWithArguments() {
            CommandlineArgs args = CommandLineHelper.GetCommandlineArguments();

            switch (args.Argument) {
                case CommandLineHelper.Argument.Autostart:
                    //Tray with Hotkeys
                    await StartTray.Initialize();
                    break;
                case CommandLineHelper.Argument.GIF:
                    //GIF Recording Capture
                    new GifWindow().ShowDialog();
                    break;
                case CommandLineHelper.Argument.Snipe:
                    //Normal Image Capture
                    new ScreenshotWindow().ShowDialog();
                    break;
                case CommandLineHelper.Argument.Upload:
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
