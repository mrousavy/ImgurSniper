using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImgurSniper.Libraries.Helper {
    public static class ImageHelper {
        public static readonly string[] ImageFileExtensions = { "jpg", "jpeg", "png", "apng", "gif", "bmp", "ico", "tif", "tiff" };

        public static MemoryStream CompressImage(Image image, ImageFormat format, long quality) {
            if (image == null) {
                throw new ArgumentNullException();
            }

            if (format.Equals(ImageFormat.Jpeg)) {
                //Jpeg Quality in %
                return CompressJpeg(image, quality);
            } else if (format.Equals(ImageFormat.Tiff)) {
                //Tiff Compression in %
                return CompressTiff(image, quality);
            } else {
                //No compression (Png, Gif, ..)
                MemoryStream stream = new MemoryStream();
                image.Save(stream, format);
                return stream;
            }
        }

        private static MemoryStream CompressJpeg(Image image, long quality) {
            ImageCodecInfo codec = GetEncoder(ImageFormat.Jpeg);

            EncoderParameters parameters =
                new EncoderParameters(1) {
                    Param = {
                        [0] = new EncoderParameter(Encoder.Quality, quality)
                    }
                };

            MemoryStream stream = new MemoryStream();
            image.Save(stream, codec, parameters);
            return stream;
        }

        private static MemoryStream CompressTiff(Image image, long quality) {
            ImageCodecInfo codec = GetEncoder(ImageFormat.Tiff);

            EncoderParameters parameters =
                new EncoderParameters(1) {
                    Param = {
                        [0] = new EncoderParameter(Encoder.Quality, 100 - quality)
                    }
                };

            MemoryStream stream = new MemoryStream();
            image.Save(stream, codec, parameters);
            return stream;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public static Image LoadImage(string filePath) {
            try {
                if (!string.IsNullOrEmpty(filePath) && IsImage(filePath) && File.Exists(filePath)) {
                    return Image.FromStream(new MemoryStream(File.ReadAllBytes(filePath)));
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static bool IsImage(string filePath) {
            return CheckExtension(filePath, ImageFileExtensions);
        }
        public static bool CheckExtension(string filePath, IEnumerable<string> extensions) {
            string ext = GetFilenameExtension(filePath);

            if (!string.IsNullOrEmpty(ext)) {
                return extensions.Any(x => ext.Equals(x, StringComparison.InvariantCultureIgnoreCase));
            }

            return false;
        }
        public static string GetFilenameExtension(string filePath) {
            if (!string.IsNullOrEmpty(filePath)) {
                int pos = filePath.LastIndexOf('.');

                if (pos >= 0) {
                    return filePath.Substring(pos + 1);
                }
            }

            return null;
        }
    }
}
