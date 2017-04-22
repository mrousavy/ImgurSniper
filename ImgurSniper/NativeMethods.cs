using System;
using System.Drawing;
using System.Runtime.InteropServices;
using static ImgurSniper.CaptureHelpers;
using static ImgurSniper.NativeStructs;

namespace ImgurSniper {
    internal class NativeMethods {

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric smIndex);
        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(out CursorInfo pci);
        [DllImport("user32.dll")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);
        [DllImport("user32.dll")]
        public static extern bool GetIconInfo(IntPtr hIcon, out IconInfo piconinfo);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        public static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon, int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();


        public static Point GetZeroBasedMousePosition() {
            return ScreenToClient(GetCursorPosition());
        }
        public static Point ScreenToClient(Point p) {
            int screenX = GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN);
            int screenY = GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN);
            return new Point(p.X - screenX, p.Y - screenY);
        }
        public static Point GetCursorPosition() {
            POINT point;

            if (GetCursorPos(out point)) {
                return (Point)point;
            }

            return Point.Empty;
        }
    }
}
