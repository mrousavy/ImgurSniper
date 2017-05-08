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
        internal static async Task CopyLink(string link, bool wasSaved) {
            Clipboard.SetText(link);
            Helpers.PlayBlop();

            Action action = delegate { Process.Start(link); };

            if (ConfigHelper.OpenBrowserAfterUpload) {
                Process.Start(link);
                action = null;
            }

            string content = wasSaved ? strings.linkclipboardAndSaved : strings.linkclipboard;

            await ShowNotificationAsync(content, NotificationType.Success, action);
        }

        //Parse stream to Image and write to Clipboard
        public static async Task CopyImage(Stream stream, bool wasSaved, bool gif) {
            //Parse byte[] to Images
            BitmapImage image = new BitmapImage();
            stream.Position = 0;
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();

            //Copy whole Image to Clipboard
            Clipboard.SetImage(image);

            string content;
            if (wasSaved) {
                content = gif ? strings.gifCopyClipboardSaved : strings.imgCopyClipboardSaved;
            } else {
                content = gif ? strings.gifCopyClipboard : strings.imgCopyClipboard;
            }

            await ShowNotificationAsync(content, NotificationType.Success);
        }
    }
}
