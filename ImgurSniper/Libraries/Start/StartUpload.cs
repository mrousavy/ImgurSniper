using ImgurSniper.Libraries.Helper;
using ImgurSniper.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using static ImgurSniper.NotificationWindow;
using static ImgurSniper.Statics;

namespace ImgurSniper.Libraries.Start {
    public static class StartUpload {
        /// <summary>
        /// Upload more than one Image to Imgur Album
        /// </summary>
        /// <param name="files">Path to all upload-queued Images</param>
        public static async Task UploadMultiple(IEnumerable<string> files) {
            try {
                //Logging in
                ImgurUploader imgur = new ImgurUploader();
                await imgur.Login();

                //Binary Image
                List<byte[]> images = new List<byte[]>();

                //Load every Image
                double size = 0;
                foreach (string file in files) {
                    byte[] image = File.ReadAllBytes(file);
                    images.Add(image);
                    size += image.Length;
                }

                //Image Size
                string kb = $"{size / 1024d:0.#}";

                //Key = Album ID | Value = Album Delete Hash (Key = Value if User is logged in)
                Tuple<string, string> albumInfo = await imgur.CreateAlbum();

                ShowNotification("", NotificationType.Progress, false);

                int index = 1;
                //Upload each image
                foreach (byte[] image in images) {
                    try {
                        Notification.contentLabel.Text = string.Format(strings.uploadingFiles, kb, index, images.Count);
                        await imgur.UploadToAlbum(image, "", albumInfo.Item2);
                    } catch (Exception e) {
                        Console.WriteLine(e.Message);
                        //this image was not uploaded
                    }
                    index++;
                }

                Notification?.Close();
                await OpenAlbum(albumInfo.Item1);
            } catch {
                //Unsupported File Type? Internet connection error?
                await ShowNotificationAsync(strings.errorInstantUpload, NotificationType.Error, ActionTroubleshoot);
            }

            await Task.Delay(500);
            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// Upload a single File to Imgur
        /// </summary>
        /// <param name="file">Path to Image</param>
        public static async Task UploadSingle(string file) {
            ImgurUploader imgur = new ImgurUploader();
            await imgur.Login();

            try {
                //Binary Image
                byte[] byteImg = File.ReadAllBytes(file);

                //Image Size
                string kb = $"{byteImg.Length / 1024d:0.#}";

                //e.g. "Uploading Image (123KB)"
                ShowNotification(string.Format(strings.uploading, kb), NotificationType.Progress, false);

                string link = await imgur.Upload(byteImg);
                await ClipboardHelper.CopyLink(link);

                Notification?.Close();
            } catch {
                //Unsupported File Type? Internet connection error?
                await ShowNotificationAsync(strings.errorInstantUpload, NotificationType.Error, ActionTroubleshoot);
            }

            await Task.Delay(500);
            Application.Current.Shutdown(0);
        }

        //Open an Album with the ID
        private static async Task OpenAlbum(string albumId) {
            //Default Imgur Album URL
            string link = "http://imgur.com/a/" + albumId;

            await ClipboardHelper.CopyLink(link);
        }
    }
}
