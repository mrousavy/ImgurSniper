using ImgurSniper.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using static ImgurSniper.NotificationWindow;
using static ImgurSniper.Statics;

namespace ImgurSniper.Libraries.Helper {
    public static class ScreenshotHelper {
        public static async Task FinishScreenshot(byte[] image, string hwndName) {
            try {
                //Config: Save Image locally?
                if (ConfigHelper.SaveImages) {
                    try {
                        //Save File with unique name
                        long time = DateTime.Now.ToFileTimeUtc();
                        string extension = "." + ConfigHelper.ImageFormat.ToString().ToLower();
                        string filename = Path.Combine(ConfigHelper.SaveImagesPath, $"Snipe_{time}{extension}");
                        File.WriteAllBytes(filename, image);

                        if (ConfigHelper.OpenAfterUpload) {
                            //Open Explorer and Highlight Image
                            Process.Start("explorer.exe", $"/select, \"{filename}\"");
                        }
                    } catch {
                        // could not start process
                    }
                }

                //Config: Upload Image to Imgur or Copy to Clipboard?
                if (ConfigHelper.ImgurAfterSnipe) {
                    await UploadImgur(image, hwndName, false);
                } else {
                    //Copy Binary Image to Clipboard
                    await ClipboardHelper.CopyImage(image);
                }
            } catch (Exception ex) {
                await ShowNotificationAsync(strings.errorMsg, NotificationType.Error, true, ActionTroubleshoot);
                MessageBox.Show(string.Format(strings.otherErrorMsg, ex.Message), strings.errorMsg);
            }
        }

        public static async Task FinishGif(byte[] image, string hwndName) {
            try {
                //Config: Save GIF locally?
                if (ConfigHelper.SaveImages) {
                    try {
                        //Save File with unique name
                        long time = DateTime.Now.ToFileTimeUtc();
                        string filename = Path.Combine(ConfigHelper.SaveImagesPath, $"Snipe_{time}.gif");
                        File.WriteAllBytes(filename, image);

                        if (ConfigHelper.OpenAfterUpload) {
                            //Open Explorer and Highlight Image
                            Process.Start("explorer.exe", $"/select, \"{filename}\"");
                        }
                    } catch {
                        // could not start process
                    }
                }

                //Config: Upload GIF to Imgur or Copy to Clipboard?
                if (ConfigHelper.ImgurAfterSnipe) {
                    await UploadImgur(image, hwndName, true);
                } else {
                    //Copy Binary GIF to Clipboard
                    await ClipboardHelper.CopyImage(image);
                }
            } catch (Exception ex) {
                await ShowNotificationAsync(strings.errorMsg, NotificationType.Error, true, ActionTroubleshoot);
                MessageBox.Show(string.Format(strings.otherErrorMsg, ex.Message), strings.errorMsg);
            }
        }



        private static async Task UploadImgur(byte[] image, string hwndName, bool gif) {
            ImgurUploader imgur = new ImgurUploader();
            await imgur.Login();

#if DEBUG
            bool tooBig = false;
#else
                            //10 MB = 10.485.760 Bytes      => Imgur's max. File Size
                            bool tooBig = image.Length >= 10485760;
#endif
            if (tooBig) {
                //Could not upload to imgur
                await ShowNotificationAsync(gif ? strings.imgTooBigGif : strings.imgTooBig, NotificationType.Error, true, ActionTroubleshoot);
                return;
            }

            //Progress Indicator
            string kb = $"{image.Length / 1024d:0.#}";
            ShowNotification(string.Format(gif ? strings.uploadingGif : strings.uploading, kb), NotificationType.Progress, false);

            //Upload Binary
            string link = await imgur.Upload(image, hwndName);
            await ClipboardHelper.CopyLink(link);

            Notification?.Close();

        }
    }
}
