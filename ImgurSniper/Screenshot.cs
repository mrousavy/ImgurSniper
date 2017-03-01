using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgurSniper {
    class Screenshot {

        public static ImageSource getScreenshot(Rectangle coordinates) {
            System.Drawing.Point start = new System.Drawing.Point(coordinates.Left, coordinates.Top);

            int width = coordinates.Width;
            int height = coordinates.Height;

            //Use Pixel Format with 32 bits per pixel and no Alpha Channel (RGB)
            using(var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb)) {
                using(var bmpGraphics = Graphics.FromImage(screenBmp)) {
                    bmpGraphics.CopyFromScreen(start, System.Drawing.Point.Empty, new System.Drawing.Size(width, height));

                    IntPtr hBitmap = screenBmp.GetHbitmap();

                    BitmapSource ret = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(hBitmap);

                    return ret;
                }
            }
        }

        public static Image MediaImageToDrawingImage(ImageSource image) {
            MemoryStream ms = new MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
            encoder.Save(ms);
            ms.Flush();
            return Image.FromStream(ms);
        }

        // P/Invoke declarations
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
    }
}
