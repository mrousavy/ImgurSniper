using ImgurSniper.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static ImgurSniper.NotificationWindow;
using static ImgurSniper.Statics;

namespace ImgurSniper.Libraries.Helper {
    public static class ClipboardHelper {
        //Copy Link to Clipboard
        internal static async Task CopyLink(string link) {
            Clipboard.SetText(link);
            Helpers.PlayBlop();

            Action action = delegate { Process.Start(link); };

            if (ConfigHelper.OpenAfterUpload) {
                Process.Start(link);
                action = null;
            }

            await ShowNotificationAsync(strings.linkclipboard, NotificationType.Success, action);
        }

        //Parse byte[] to Image and write to Clipboard
        public static async Task CopyImage(byte[] cimg) {
            //Parse byte[] to Images
            BitmapImage image = new BitmapImage();
            using (MemoryStream mem = new MemoryStream(cimg)) {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();

            //Copy whole Image to Clipboard
            Clipboard.SetImage(image);

            await ShowNotificationAsync(strings.imgclipboard, NotificationType.Success);
        }
    }
}
