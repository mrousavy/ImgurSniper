using System;
using System.Drawing;
using System.Runtime.InteropServices;
using static ImgurSniper.NativeStructs;

namespace ImgurSniper {
    public class CursorData : IDisposable {
        public bool IsVisible { get; private set; }
        public IntPtr IconHandle { get; private set; }
        public Point Position { get; private set; }

        public CursorData() {
            UpdateCursorData();
        }

        public void UpdateCursorData() {
            CursorInfo cursorInfo = new CursorInfo();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);

            if (NativeMethods.GetCursorInfo(out cursorInfo)) {
                IsVisible = cursorInfo.flags == 1;  //1 = CURSOR_SHOWING

                if (IsVisible) {
                    IconHandle = NativeMethods.CopyIcon(cursorInfo.hCursor);

                    if (NativeMethods.GetIconInfo(IconHandle, out IconInfo iconInfo)) {
                        Point cursorPosition = NativeMethods.ScreenToClient(NativeMethods.GetCursorPosition());
                        Position = new Point(cursorPosition.X - iconInfo.xHotspot, cursorPosition.Y - iconInfo.yHotspot);

                        if (iconInfo.hbmMask != IntPtr.Zero) {
                            NativeMethods.DeleteObject(iconInfo.hbmMask);
                        }

                        if (iconInfo.hbmColor != IntPtr.Zero) {
                            NativeMethods.DeleteObject(iconInfo.hbmColor);
                        }
                    }
                }
            }
        }

        public void DrawCursorToImage(Image img) {
            DrawCursorToImage(img, Point.Empty);
        }

        public void DrawCursorToImage(Image img, Point cursorOffset) {
            if (IconHandle != IntPtr.Zero) {
                Point drawPosition = new Point(Position.X - cursorOffset.X, Position.Y - cursorOffset.Y);

                using (Graphics g = Graphics.FromImage(img))
                using (Icon icon = Icon.FromHandle(IconHandle)) {
                    g.DrawIcon(icon, drawPosition.X, drawPosition.Y);
                }
            }
        }

        public void DrawCursorToHandle(IntPtr hdcDest) {
            DrawCursorToHandle(hdcDest, Point.Empty);
        }

        public void DrawCursorToHandle(IntPtr hdcDest, Point cursorOffset) {
            if (IconHandle != IntPtr.Zero) {
                Point drawPosition = new Point(Position.X - cursorOffset.X, Position.Y - cursorOffset.Y);
                NativeMethods.DrawIconEx(hdcDest, drawPosition.X, drawPosition.Y, IconHandle, 0, 0, 0, IntPtr.Zero, 0x0003);    // 0x0003 = DI_NORMAL
            }
        }

        public void Dispose() {
            if (IconHandle != IntPtr.Zero) {
                NativeMethods.DestroyIcon(IconHandle);
                IconHandle = IntPtr.Zero;
            }
        }
    }
}