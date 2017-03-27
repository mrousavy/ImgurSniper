using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ImgurSniper {
    internal class WinAPI {
        public const uint SwpNosize = 0x0001;
        public const uint SwpNomove = 0x0002;
        public const uint SwpNoactivate = 0x0010;

        public static readonly IntPtr HwndBottom = new IntPtr(1);

        public static Rectangle GetWindowRectangle(IntPtr handle) {
            Rectangle rect = Rectangle.Empty;

            if(IsDwmEnabled()) {
                if(GetExtendedFrameBounds(handle, out Rectangle tempRect)) {
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
            if(GetBorderSize(handle, out Size size)) {
                windowRect = new Rectangle(windowRect.X + size.Width, windowRect.Y + size.Height,
                    windowRect.Width - size.Width * 2, windowRect.Height - size.Height * 2);
            }

            return windowRect;
        }

        public static bool IsDwmEnabled() {
            return Environment.OSVersion.Version.Major >= 6 && Dwmapi.DwmIsCompositionEnabled();
        }

        public static Rectangle GetWindowRect(IntPtr handle) {
            User32.GetWindowRect(handle, out RECT rect);
            return rect;
        }

        public static bool GetExtendedFrameBounds(IntPtr handle, out Rectangle rectangle) {
            int result = Dwmapi.DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.ExtendedFrameBounds, out RECT rect,
    Marshal.SizeOf(typeof(RECT)));
            rectangle = rect;
            return result == 0;
        }

        public static bool GetBorderSize(IntPtr handle, out Size size) {
            WindowInfo wi = new WindowInfo();

            bool result = User32.GetWindowInfo(handle, ref wi);

            size = result ? new Size((int)wi.cxWindowBorders, (int)wi.cyWindowBorders) : Size.Empty;

            return result;
        }

        /// <summary>
        ///     Helper class containing dwmapi API functions
        /// </summary>
        public static class Dwmapi {
            [DllImport("dwmapi.dll")]
            public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute,
                int cbAttribute);

            [DllImport("dwmapi.dll", PreserveSig = false)]
            public static extern bool DwmIsCompositionEnabled();
        }

        /// <summary>
        ///     Helper class containing User32 API functions
        /// </summary>
        public static class User32 {
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
                uint uFlags);

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
            public static extern bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        }

        #region Window styles

        [Flags]
        public enum ExtendedWindowStyles {
            // ...
            WsExToolwindow = 0x00000080
            // ...
        }

        public enum GetWindowLongFields {
            // ...
            GwlExstyle = -20
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
            int error = 0;
            IntPtr result;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if(IntPtr.Size == 4) {
                // use SetWindowLong
                int tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            } else {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if(result == IntPtr.Zero && error != 0) {
                throw new Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr) {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        #endregion

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

            public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int Width {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public int Height {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public static implicit operator Rectangle(RECT r) {
                return new Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(Rectangle r) {
                return new RECT(r);
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
        }

        #endregion
    }
}