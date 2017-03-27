using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImgurSniper {
    internal class Screenshot {
        private const int CURSOR_SHOWING = 0x00000001;

        public static Bitmap GetScreenshot(Rectangle coordinates) {
            //Use Pixel Format with 32 bits per pixel and no Alpha Channel (RGB)
            Bitmap screenBmp = new Bitmap(coordinates.Width, coordinates.Height, PixelFormat.Format32bppRgb);

            using (Graphics bmpGraphics = Graphics.FromImage(screenBmp)) {
                bmpGraphics.CopyFromScreen(coordinates.Left, coordinates.Top, 0, 0,
                    new Size(coordinates.Width, coordinates.Height));
            }

            return screenBmp;
        }


        public static Bitmap GetScreenshotWithMouse(Rectangle size) {
            Bitmap result = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(result)) {
                g.CopyFromScreen(size.Left, size.Top, 0, 0, size.Size, CopyPixelOperation.SourceCopy);

                CURSORINFO pci;
                pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                if (GetCursorInfo(out pci)) {
                    if (pci.flags == CURSOR_SHOWING) {
                        DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                        g.ReleaseHdc();
                    }
                }
            }


            return result;
        }

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
    }
}