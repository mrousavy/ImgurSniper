using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImgurSniper.Libraries.Native {
    internal class NativeStructs {
        [StructLayout(LayoutKind.Sequential)]
        public struct CursorInfo {
            public int cbSize; // Specifies the size, in bytes, of the structure.
            public int flags; // Specifies the cursor state. This parameter can be one of the following values:
            public IntPtr hCursor; // Handle to the cursor.
            public Point ptScreenPos; // A POINT structure that receives the screen coordinates of the cursor.
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IconInfo {
            public bool fIcon; // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies
            public int xHotspot; // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot
            public int yHotspot; // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot
            public IntPtr hbmMask; // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon,
            public IntPtr hbmColor; // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this
        }

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
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

    }
}
