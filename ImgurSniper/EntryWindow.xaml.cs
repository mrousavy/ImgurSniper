using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.Start;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for EntryWindow.xaml
    /// </summary>
    public partial class EntryWindow {
        public EntryWindow() {
            Hide();

            KillImgurSniper();

            StartWithArguments();
        }

        private async void StartWithArguments() {
            CommandlineArgs args = CommandLineHelper.GetCommandlineArguments();

            switch (args.Argument) {
                case CommandLineHelper.Argument.Autostart:
                    //Tray with Hotkeys
                    await StartTray.Initialize(this);
                    break;
                case CommandLineHelper.Argument.Gif:
                    //GIF Recording Capture
                    using (GifWindow window = new GifWindow())
                        window.ShowDialog();
                    break;
                case CommandLineHelper.Argument.Snipe:
                    //Normal Image Capture
                    if (ConfigHelper.FreezeScreen) {
                        using (ScreenshotWindowFreeze window = new ScreenshotWindowFreeze())
                            window.ShowDialog();
                    } else {
                        using (ScreenshotWindow window = new ScreenshotWindow())
                            window.ShowDialog();
                    }
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
            if (NotificationWindow.IsShown)
                await Task.Delay(NotificationWindow.ShowDuration);

            Application.Current.Shutdown();
        }



        public void KillImgurSniper() {
            Process.GetProcesses().Where(p => (p.ProcessName == "ImgurSniper" && p.Id != Process.GetCurrentProcess().Id)).ForEach(p => p.Kill());
        }
    }
}
