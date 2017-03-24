using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgurSniper {
    internal class Screenshot {

        public static Bitmap GetScreenshot(Rectangle coordinates) {
            System.Drawing.Point start = new System.Drawing.Point(coordinates.Left, coordinates.Top);

            int width = coordinates.Width;
            int height = coordinates.Height;

            //Use Pixel Format with 32 bits per pixel and no Alpha Channel (RGB)
            Bitmap screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            using(Graphics bmpGraphics = Graphics.FromImage(screenBmp)) {
                bmpGraphics.CopyFromScreen(start, System.Drawing.Point.Empty, new System.Drawing.Size(width, height));
            }

            return screenBmp;
        }




        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        public static Bitmap GetScreenshotWithMouse(Rectangle size) {
            Bitmap result = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using(Graphics g = Graphics.FromImage(result)) {
                g.CopyFromScreen(size.Left, size.Top, 0, 0, size.Size, CopyPixelOperation.SourceCopy);

                CURSORINFO pci;
                pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

                if(GetCursorInfo(out pci)) {
                    if(pci.flags == CURSOR_SHOWING) {
                        DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                        g.ReleaseHdc();
                    }
                }
            }

            return result;
        }



        public static Image MediaImageToDrawingImage(ImageSource image) {
            MemoryStream ms = new MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
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
