using ImgurSniper.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using static ImgurSniper.NotificationWindow;
using static ImgurSniper.Statics;
// ReSharper disable HeuristicUnreachableCode

namespace ImgurSniper.Libraries.Helper {
    public static class ScreenshotHelper {
        public static async Task FinishScreenshot(Stream stream, string hwndName) {
            try {
                //Set Stream Position to beginning
                stream.Position = 0;

                //Config: Save Image locally?
                if (ConfigHelper.SaveImages) {
                    try {
                        //Save File with unique name
                        long time = DateTime.Now.ToFileTimeUtc();
                        string extension = "." + ConfigHelper.ImageFormat.ToString().ToLower();
                        string filename = Path.Combine(ConfigHelper.SaveImagesPath, $"Snipe_{time}{extension}");
                        using (FileStream fstream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                            await stream.CopyToAsync(fstream);
                        }

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
                    await UploadImgur(stream, hwndName, false);
                } else {
                    //Copy Binary Image to Clipboard
                    await ClipboardHelper.CopyImage(stream);
                }
            } catch (Exception ex) {
                await ShowNotificationAsync(strings.errorMsg, NotificationType.Error, ActionTroubleshoot);
                MessageBox.Show(string.Format(strings.otherErrorMsg, ex.Message), strings.errorMsg);
            }
        }

        public static async Task FinishGif(Stream stream, string hwndName) {
            try {
                //Set Stream Position to beginning
                stream.Position = 0;

                //Config: Save GIF locally?
                if (ConfigHelper.SaveImages) {
                    try {
                        //Save File with unique name
                        long time = DateTime.Now.ToFileTimeUtc();
                        string filename = Path.Combine(ConfigHelper.SaveImagesPath, $"Snipe_{time}.gif");
                        using (FileStream fstream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                            await stream.CopyToAsync(fstream);
                        }

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
                    await UploadImgur(stream, hwndName, true);
                } else {
                    //Copy Binary GIF to Clipboard
                    await ClipboardHelper.CopyImage(stream);
                }
            } catch (Exception ex) {
                await ShowNotificationAsync(strings.errorMsg, NotificationType.Error, ActionTroubleshoot);
                MessageBox.Show(string.Format(strings.otherErrorMsg, ex.Message), strings.errorMsg);
            }
        }



        private static async Task UploadImgur(Stream stream, string hwndName, bool gif) {
            ImgurUploader imgur = new ImgurUploader();
            await imgur.Login();

#if DEBUG
            // ReSharper disable once ConvertToConstant.Local
            bool tooBig = false;
#else
            //10 MB = 10.485.760 Bytes => Imgur's max. File Size
            bool tooBig = image.Length >= 10485760;
#endif
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (tooBig) {
                //Could not upload to imgur
                await ShowNotificationAsync(gif ? strings.imgTooBigGif : strings.imgTooBig, NotificationType.Error, ActionTroubleshoot);
                return;
            }

            //Progress Indicator
            string kb = $"{stream.Length / 1024d:0.#}";
            ShowNotification(string.Format(gif ? strings.uploadingGif : strings.uploading, kb), NotificationType.Progress, false);

            //Upload Binary
            string link = await imgur.Upload(stream, hwndName);
            await ClipboardHelper.CopyLink(link);
        }
    }
}
