using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Size = System.Drawing.Size;

namespace ImgurSniper {
    internal class Screenshot {
        private const int CursorShowing = 0x00000001;

        //~10 ms Faster on 4480 x 1440 Size
        public static Image GetScreenshotNative(IntPtr handle, Rectangle rect, bool captureCursor = true) {
            if (rect.Width == 0 || rect.Height == 0) {
                return null;
            }

            IntPtr hdcSrc = NativeMethods.GetWindowDC(handle);
            IntPtr hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
            IntPtr hOld = NativeMethods.SelectObject(hdcDest, hBitmap);
            NativeMethods.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

            if (captureCursor) {
                Point cursorOffset = CaptureHelpers.ScreenToClient(rect.Location);

                try {
                    using (CursorData cursorData = new CursorData()) {
                        cursorData.DrawCursorToHandle(hdcDest, cursorOffset);
                    }
                } catch {
                    //Could not capture Cursor
                }
            }

            NativeMethods.SelectObject(hdcDest, hOld);
            NativeMethods.DeleteDC(hdcDest);
            NativeMethods.ReleaseDC(handle, hdcSrc);
            Image img = Image.FromHbitmap(hBitmap);
            NativeMethods.DeleteObject(hBitmap);

            return img;
        }

        //~6 ms Slower on 4480 x 1440 Size
        //Get a normal Screenshot
        public static Bitmap GetScreenshot(Rectangle coordinates) {
            //Use Pixel Format with 32 bits per pixel and no Alpha Channel (RGB)
            Bitmap screenBmp = new Bitmap(coordinates.Width, coordinates.Height, PixelFormat.Format32bppRgb);

            using (Graphics bmpGraphics = Graphics.FromImage(screenBmp)) {
                bmpGraphics.CopyFromScreen(coordinates.Left, coordinates.Top, 0, 0,
                    new Size(coordinates.Width, coordinates.Height));
            }

            return screenBmp;
        }

        //~10 ms Slower on 4480 x 1440 Size
        //Get a Screenshot with mouse cursor
        public static Bitmap GetScreenshotWithMouse(Rectangle size) {
            Bitmap result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppRgb);

            using (Graphics g = Graphics.FromImage(result)) {
                g.CopyFromScreen(size.Left, size.Top, 0, 0, size.Size, CopyPixelOperation.SourceCopy);

                CURSORINFO pci;
                pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                if (GetCursorInfo(out pci)) {
                    if (pci.flags == CursorShowing) {
                        DrawIcon(g.GetHdc(), pci.ptScreenPos.x - size.X, pci.ptScreenPos.y - size.Y, pci.hCursor);
                        g.ReleaseHdc();
                    }
                }
            }

            return result;
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