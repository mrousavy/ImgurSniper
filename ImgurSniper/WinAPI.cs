using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ImgurSniper {
    class WinAPI {

        public static Rectangle GetWindowRectangle(IntPtr handle) {
            Rectangle rect = Rectangle.Empty;

            if(IsDWMEnabled()) {
                Rectangle tempRect;

                if(GetExtendedFrameBounds(handle, out tempRect)) {
                    rect = tempRect;
                }
            }

            if(rect.IsEmpty) {
                rect = GetWindowRect(handle);
            }

            if(Environment.OSVersion.Version.Major < 10 && User32.IsZoomed(handle)) {
                rect = MaximizedWindowFix(handle, rect);
            }

            return rect;
        }

        public static Rectangle MaximizedWindowFix(IntPtr handle, Rectangle windowRect) {
            Size size;

            if(GetBorderSize(handle, out size)) {
                windowRect = new Rectangle(windowRect.X + size.Width, windowRect.Y + size.Height, windowRect.Width - (size.Width * 2), windowRect.Height - (size.Height * 2));
            }

            return windowRect;
        }

        public static bool IsDWMEnabled() {
            return Environment.OSVersion.Version.Major >= 6 && dwmapi.DwmIsCompositionEnabled();
        }

        public static Rectangle GetWindowRect(IntPtr handle) {
            RECT rect;
            User32.GetWindowRect(handle, out rect);
            return rect;
        }

        public static bool GetExtendedFrameBounds(IntPtr handle, out Rectangle rectangle) {
            RECT rect;
            int result = dwmapi.DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.ExtendedFrameBounds, out rect, Marshal.SizeOf(typeof(RECT)));
            rectangle = rect;
            return result == 0;
        }


        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 SWP_NOACTIVATE = 0x0010;


        public static bool GetBorderSize(IntPtr handle, out Size size) {
            WINDOWINFO wi = new WINDOWINFO();

            bool result = User32.GetWindowInfo(handle, ref wi);

            if(result) {
                size = new Size((int)wi.cxWindowBorders, (int)wi.cyWindowBorders);
            } else {
                size = Size.Empty;
            }

            return result;
        }

        /// <summary>
        /// Helper class containing dwmapi API functions
        /// </summary>
        public static class dwmapi {
            [DllImport("dwmapi.dll")]
            public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

            [DllImport("dwmapi.dll")]
            public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);

            [DllImport("dwmapi.dll")]
            public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);


            [DllImport("dwmapi.dll", PreserveSig = false)]
            public static extern bool DwmIsCompositionEnabled();
        }

        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        public static class GDI32 {
            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        public static class User32 {

            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowConstants wCmd);

            [DllImport("user32.dll")]
            public static extern IntPtr SetActiveWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern bool BringWindowToTop(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr WindowFromPoint(POINT point);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsZoomed(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern int GetSystemMetrics(SystemMetric smIndex);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        }


        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if(IntPtr.Size == 4) {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            } else {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if((result == IntPtr.Zero) && (error != 0)) {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr) {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion

        #region Custom Definitions
        public enum GetWindowConstants : uint {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6,
            GW_MAX = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO {
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

            public WINDOWINFO(bool? filler) : this() // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (uint)Marshal.SizeOf(typeof(WINDOWINFO));
            }
        }

        [Flags]
        public enum DwmWindowAttribute {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
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
                get { return Left; }
                set { Right -= Left - value; Left = value; }
            }

            public int Y {
                get { return Top; }
                set { Bottom -= Top - value; Top = value; }
            }

            public int Width {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public int Height {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public Point Location {
                get { return new Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public Size Size {
                get { return new Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator Rectangle(RECT r) {
                return new Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(Rectangle r) {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2) {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2) {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r) {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj) {
                if(obj is RECT) {
                    return Equals((RECT)obj);
                }

                if(obj is Rectangle) {
                    return Equals(new RECT((Rectangle)obj));
                }

                return false;
            }

            public override int GetHashCode() {
                return ((Rectangle)this).GetHashCode();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                X = x;
                Y = y;
            }

            public static explicit operator Point(POINT p) {
                return new Point(p.X, p.Y);
            }

            public static explicit operator POINT(Point p) {
                return new POINT(p.X, p.Y);
            }
        }
        #endregion
    }
}
