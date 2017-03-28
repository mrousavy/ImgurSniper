using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Size = System.Drawing.Size;

namespace ImgurSniper {
    internal class Screenshot {
        private const int CURSOR_SHOWING = 0x00000001;

        //Get a normal Screenshot
        public static Bitmap GetScreenshot(Rectangle coordinates) {
            //Use Pixel Format with 32 bits per pixel and no Alpha Channel (RGB)
            Bitmap screenBmp = new Bitmap(coordinates.Width, coordinates.Height, PixelFormat.Format32bppRgb);

            using(Graphics bmpGraphics = Graphics.FromImage(screenBmp)) {
                bmpGraphics.CopyFromScreen(coordinates.Left, coordinates.Top, 0, 0,
                    new Size(coordinates.Width, coordinates.Height));
            }

            return screenBmp;
        }

        //Get a Screenshot with mouse cursor
        public static Bitmap GetScreenshotWithMouse(Rectangle size) {
            Bitmap result = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

            using(Graphics g = Graphics.FromImage(result)) {
                g.CopyFromScreen(size.Left, size.Top, 0, 0, size.Size, CopyPixelOperation.SourceCopy);

                CURSORINFO pci;
                pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                if(GetCursorInfo(out pci)) {
                    if(pci.flags == CURSOR_SHOWING) {
                        DrawIcon(g.GetHdc(), pci.ptScreenPos.x - size.X, pci.ptScreenPos.y - size.Y, pci.hCursor);
                        g.ReleaseHdc();
                    }
                }
            }

            return result;
        }

        //Get a Screenshot for GIF (GIF has max Width 400 and Height 300)
        public static Bitmap GetGifScreenshot(Rectangle size) {
            Bitmap image = GetScreenshotWithMouse(size);


            if(size.Width > 400 || size.Height > 300) {
                int width = size.Width;
                int height = size.Height;

                KeyValuePair<int, int> ratio = GetAspectRatio(width, height);
                int ratioY = ratio.Key;
                int ratioX = ratio.Value;

                while(width > 400 || height > 300) {
                    if((height - ratioY) > 0 && (width - ratioX > 0)) {
                        height -= ratioY;
                        width -= ratioX;
                    } else {
                        break;
                    }
                }

                image = ResizeImage(image, width, height);
            }

            return image;
        }


        //Get the Aspect Ratio of two Integers
        private static KeyValuePair<int, int> GetAspectRatio(int width, int height) {
            int hcf = FindHcf(width, height);
            int factorW = width / hcf;
            int factorH = height / hcf;

            return new KeyValuePair<int, int>(factorH, factorW);
        }


        private static int FindHcf(int m, int n) {
            if(m < n) {
                int temp = m;
                m = n;
                n = temp;
            }
            while(true) {
                int reminder = m % n;
                if(reminder == 0)
                    return n;
                else
                    m = n;
                n = reminder;
            }
        }


        //Resize a Bitmap
        public static Bitmap ResizeImage(Image image, int width, int height) {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using(Graphics graphics = Graphics.FromImage(destImage)) {
                //Disabled for more Performance:
                //graphics.CompositingMode = CompositingMode.SourceCopy;
                //graphics.CompositingQuality = CompositingQuality.HighQuality;
                //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //graphics.SmoothingMode = SmoothingMode.HighQuality;
                //graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using(ImageAttributes wrapMode = new ImageAttributes()) {
                    //Disabled for more Performance:
                    //wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        #region Cursor
        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        private static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);


        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO {
            public int cbSize;
            public readonly int flags;
            public readonly IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTAPI {
            public readonly int x;
            public readonly int y;
        }
        #endregion
    }
}