#region License Information (GPL v3)

/*
    Source code provocatively stolen from ShareX: https://github.com/ShareX/ShareX.
    (Seriously, awesome work over there, I took some parts of the Code to make
    ImgurSniper.)
    Their License:

    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2017 ShareX Team
    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.Native;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Size = System.Drawing.Size;

namespace ImgurSniper.Libraries.ScreenCapture {
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
            NativeMethods.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y,
                CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

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
            Bitmap screenBmp = new Bitmap(coordinates.Width, coordinates.Height, PixelFormat.Format24bppRgb);

            using (Graphics bmpGraphics = Graphics.FromImage(screenBmp)) {
                bmpGraphics.CopyFromScreen(coordinates.Left, coordinates.Top, 0, 0,
                    new Size(coordinates.Width, coordinates.Height));
            }

            return screenBmp;
        }

        //~10 ms Slower on 4480 x 1440 Size
        //Get a Screenshot with mouse cursor
        public static Bitmap GetScreenshotWithMouse(Rectangle size) {
            Bitmap result = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

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
        private static extern bool DrawIcon(IntPtr hDc, int x, int y, IntPtr hIcon);


        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        private struct CURSORINFO {
            public int cbSize;
            public readonly int flags;
            public readonly IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        private struct POINTAPI {
            public readonly int x;
            public readonly int y;
        }
        #endregion
    }
}
