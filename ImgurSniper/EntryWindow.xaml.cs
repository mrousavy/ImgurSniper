using System.Threading.Tasks;
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
                    await StartTray.Initialize(this);
                    break;
                case CommandLineHelper.Argument.GIF:
                    //GIF Recording Capture
                    using (GifWindow window = new GifWindow())
                        window.ShowDialog();
                        break;
                case CommandLineHelper.Argument.Snipe:
                    //Normal Image Capture
                    using (ScreenshotWindow window = new ScreenshotWindow())
                        window.ShowDialog();
                    break;
                case CommandLineHelper.Argument.Upload:
                    //Context Menu Instant Upload
                    if (args.UploadFiles.Count > 1)
                        //1 or more files
                        //TODO: Implement "%1" more than 1
                        await StartUpload.UploadMultiple(args.UploadFiles);
                    else if (args.UploadFiles.Count == 1)
                        //1 File
                        await StartUpload.UploadSingle(args.UploadFiles[0]);
                    else
                        //No Image File detected
                        await Statics.ShowNotificationAsync(Properties.strings.notAnImage, NotificationWindow.NotificationType.Error);
                    break;
            }

            //Wait for every Notification to close
            if(NotificationWindow.IsShown)
                await Task.Delay(NotificationWindow.ShowDuration);

            Application.Current.Shutdown();
        }
    }
}
