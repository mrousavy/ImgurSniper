using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgurSniper {
    class Screenshot {

        public static ImageSource getScreenshot(bool AllMonitors) {
            if(!AllMonitors) {
                //Thanks http://stackoverflow.com/users/214375/marcel-gheorghita !
                Rectangle screen = ScreenshotWindow.screen;
                Rectangle rect = new Rectangle(screen.X, screen.Y, screen.Width, screen.Height);
                Bitmap bmp = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);


                byte[] byteImage = ImageToByte(bmp);

                bmp.Dispose();

                BitmapImage biImg = new BitmapImage();
                MemoryStream ms = new MemoryStream(byteImage);
                biImg.BeginInit();
                biImg.StreamSource = ms;
                biImg.EndInit();

                ImageSource imgSrc = biImg as ImageSource;
                return imgSrc;
            } else {
                //Thanks http://stackoverflow.com/users/183367/julien-lebosquain !
                var left = System.Windows.Forms.Screen.AllScreens.Min(screen => screen.Bounds.X);
                var top = System.Windows.Forms.Screen.AllScreens.Min(screen => screen.Bounds.Y);
                var right = System.Windows.Forms.Screen.AllScreens.Max(screen => screen.Bounds.X + screen.Bounds.Width);
                var bottom = System.Windows.Forms.Screen.AllScreens.Max(screen => screen.Bounds.Y + screen.Bounds.Height);
                var width = right - left;
                var height = bottom - top;

                using(var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                    using(var bmpGraphics = Graphics.FromImage(screenBmp)) {
                        bmpGraphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));

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
        }


        public static byte[] ImageToByte(Image img) {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
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
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);
    }
}
