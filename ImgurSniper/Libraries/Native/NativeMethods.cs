using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using static ImgurSniper.Libraries.Helper.CaptureHelpers;
using static ImgurSniper.Libraries.Native.NativeStructs;

namespace ImgurSniper.Libraries.Native {
    internal class NativeMethods {
        public const uint SwpNosize = 0x0001;
        public const uint SwpNomove = 0x0002;
        public const uint SwpNoactivate = 0x0010;

        public static readonly IntPtr HwndBottom = new IntPtr(1);

        private static readonly Version OsVersion = Environment.OSVersion.Version;

        #region DllImports
        #region GDI32
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        #endregion

        #region Dwmapi
        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute,
                int cbAttribute);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();
        #endregion

        #region User32
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr parentHwnd, IntPtr childAfterHwnd, IntPtr className, string windowText);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
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
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT point);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WindowInfo pwi);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsZoomed(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        #endregion
        #endregion

        #region Windows
        public static bool IsWindowsVista() => OsVersion.Major == 6;

        public static bool IsWindows7() => OsVersion.Major == 6 && OsVersion.Minor == 1;

        public enum GetWindowLongFields {
            GwlExstyle = -20
        }

        [Flags]
        public enum ExtendedWindowStyles {
            WsExToolwindow = 0x00000080
        }
        #endregion


        public static string GetWindowName(POINT position) {
            try {
                const int nChars = 256;
                StringBuilder buff = new StringBuilder(nChars);
                return GetWindowText(WindowFromPoint(position), buff, nChars) > 0 ? buff.ToString() : "";
            } catch {
                return "";
            }
        }

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
            int error;
            IntPtr result;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4) {
                // use SetWindowLong
                int tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            } else {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if (result == IntPtr.Zero && error != 0) {
                throw new Win32Exception(error);
            }

            return result;
        }

        private static int IntPtrToInt32(IntPtr intPtr) {
            return unchecked((int)intPtr.ToInt64());
        }

        public static Point ScreenToClient(Point p) {
            int screenX = GetSystemMetrics(SystemMetric.SmXvirtualscreen);
            int screenY = GetSystemMetrics(SystemMetric.SmYvirtualscreen);
            return new Point(p.X - screenX, p.Y - screenY);
        }

        public static Point GetCursorPosition() {
            if (GetCursorPos(out POINT point)) {
                return (Point)point;
            }

            return Point.Empty;
        }

        public static bool IsDwmEnabled() {
            return Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled();
        }

        public static Rectangle GetWindowRect(IntPtr handle) {
            GetWindowRect(handle, out RECT rect);
            return rect;
        }

        public static bool GetExtendedFrameBounds(IntPtr handle, out Rectangle rectangle) {
            int result = DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.ExtendedFrameBounds, out RECT rect, Marshal.SizeOf(typeof(RECT)));
            rectangle = rect;
            return result == 0;
        }

        public static Rectangle GetWindowRectangle(IntPtr handle) {
            Rectangle rect = Rectangle.Empty;

            if (IsDwmEnabled()) {
                if (GetExtendedFrameBounds(handle, out Rectangle tempRect)) {
                    rect = tempRect;
                }
            }

            if (rect.IsEmpty) {
                rect = GetWindowRect(handle);
            }

            if (Environment.OSVersion.Version.Major < 10 && IsZoomed(handle)) {
                rect = MaximizedWindowFix(handle, rect);
            }

            return rect;
        }

        public static Rectangle MaximizedWindowFix(IntPtr handle, Rectangle windowRect) {
            if (GetBorderSize(handle, out Size size)) {
                windowRect = new Rectangle(windowRect.X + size.Width, windowRect.Y + size.Height,
                    windowRect.Width - size.Width * 2, windowRect.Height - size.Height * 2);
            }

            return windowRect;
        }

        public static bool GetBorderSize(IntPtr handle, out Size size) {
            WindowInfo wi = new WindowInfo();

            bool result = GetWindowInfo(handle, ref wi);

            size = result ? new Size((int)wi.cxWindowBorders, (int)wi.cyWindowBorders) : Size.Empty;

            return result;
        }

        #region Custom Definitions
        [StructLayout(LayoutKind.Sequential)]
        public struct WindowInfo {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;
        }

        [Flags]
        public enum DwmWindowAttribute {
            NcRenderingEnabled = 1,
            NcRenderingPolicy,
            TransitionsForceDisabled,
            AllowNcPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation,
            Last
        }

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        public struct RECT {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom) {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) {
            }

            public int X {
                get => Left;
                set { Right -= Left - value; Left = value; }
            }

            public int Y {
                get => Top;
                set { Bottom -= Top - value; Top = value; }
            }

            public int Width {
                get => Right - Left;
                set => Right = value + Left;
            }

            public int Height {
                get => Bottom - Top;
                set => Bottom = value + Top;
            }

            public Point Location {
                get => new Point(Left, Top);
                set { X = value.X; Y = value.Y; }
            }

            public Size Size {
                get => new Size(Width, Height);
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator Rectangle(RECT r) {
                return new Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(Rectangle r) {
                return new RECT(r);
            }
        }

        #endregion
    }
}
