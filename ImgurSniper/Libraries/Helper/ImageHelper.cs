using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;

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
                return CompressTiff(image);
            } else if (format.Equals(ImageFormat.Png)) {
                //Png Compression by reducing Color Palette
                return CompressPng(image);
            } else {
                //No compression (Png, Gif, ..)
                MemoryStream stream = new MemoryStream();
                image.Save(stream, format);
                return stream;
            }
        }

        //Compress JPEG by settings Image quality
        private static MemoryStream CompressJpeg(Image image, long quality) {
            ImageCodecInfo codec = GetEncoder(ImageFormat.Jpeg);

            //Set JPEG Quality Encoder Parameter
            EncoderParameters parameters =
                new EncoderParameters(1) {
                    Param = {
                        [0] = new EncoderParameter(Encoder.Quality, quality)
                    }
                };

            //Save to stream and return
            MemoryStream stream = new MemoryStream();
            image.Save(stream, codec, parameters);
            return stream;
        }

        //Compress TIFF using LZW Compression
        private static MemoryStream CompressTiff(Image image) {
            ImageCodecInfo codec = GetEncoder(ImageFormat.Tiff);

            //Set TIFF Compression Encoder Parameter
            EncoderParameters parameters =
                new EncoderParameters(1) {
                    Param = {
                        [0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW)
                    }
                };

            //Save to stream and return
            MemoryStream stream = new MemoryStream();
            image.Save(stream, codec, parameters);
            return stream;
        }

        //Compress PNG by reducing Color amout to 256
        private static MemoryStream CompressPng(Image image) {
            //TODO: Use WuQuantizer C Project here

            //256 color palette (lesser colors = compression)
            //BitmapPalette palette = BitmapPalettes.Halftone256;

            //// Creates a new empty image with the pre-defined palette
            //BitmapSource bitmapSource = BitmapSource.Create(
            //    image.Width,
            //    image.Height,
            //    96,
            //    96,
            //    PixelFormats.Indexed8,
            //    palette,
            //    pixels,
            //    image.Width);

            ////Create and init Encoder
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Interlace = PngInterlaceOption.On;
            //encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            //Save to stream and return
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
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


        public static ImageSource ImageToImageSource(Image image) {
            System.Windows.Media.Imaging.BitmapImage bitmapImage;
            using (var ms = new MemoryStream()) {
                image.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static Image Crop(Image screenshot, System.Windows.Point from, System.Windows.Point to) {
            Rectangle cropRect = new Rectangle((int)from.X, (int)from.Y, (int)to.X - (int)from.X, (int)to.Y - (int)from.Y);
            Image cropped = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(cropped)) {
                g.DrawImage(screenshot, new Rectangle(0, 0, cropped.Width, cropped.Height), cropRect, GraphicsUnit.Pixel);
            }

            return cropped;
        }
    }
}
