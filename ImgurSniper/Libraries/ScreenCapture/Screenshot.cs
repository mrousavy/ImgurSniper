using ImgurSniper.Libraries.Native;
using System;
using System.Drawing;

namespace ImgurSniper.Libraries.ScreenCapture {
    public class Screenshot {
        public bool CaptureCursor { get; set; } = false;
        public bool CaptureClientArea { get; set; } = false;
        public bool RemoveOutsideScreenArea { get; set; } = true;
        public bool CaptureShadow { get; set; } = false;
        public int ShadowOffset { get; set; } = 20;
        public bool AutoHideTaskbar { get; set; } = false;

        public Image CaptureRectangle(Rectangle rect) {
            if (RemoveOutsideScreenArea) {
                Rectangle bounds = NativeMethods.GetScreenBounds();
                rect = Rectangle.Intersect(bounds, rect);
            }

            return ScreenCapture.GetScreenshotNative(NativeMethods.GetDesktopWindow(), rect);
        }

        public Image CaptureFullscreen() {
            Rectangle bounds = NativeMethods.GetScreenBounds();

            return CaptureRectangle(bounds);
        }

        public Image CaptureWindow(IntPtr handle) {
            if (handle.ToInt32() > 0) {
                Rectangle rect;

                if (CaptureClientArea) {
                    rect = NativeMethods.GetClientRect(handle);
                } else {
                    rect = NativeMethods.GetWindowRectangle(handle);
                }

                bool isTaskbarHide = false;

                try {
                    if (AutoHideTaskbar) {
                        isTaskbarHide = NativeMethods.SetTaskbarVisibilityIfIntersect(false, rect);
                    }

                    return CaptureRectangle(rect);
                } finally {
                    if (isTaskbarHide) {
                        NativeMethods.SetTaskbarVisibility(true);
                    }
                }
            }

            return null;
        }

        public Image CaptureActiveWindow() {
            IntPtr handle = NativeMethods.GetForegroundWindow();

            return CaptureWindow(handle);
        }

        public Image CaptureActiveMonitor() {
            Rectangle bounds = NativeMethods.GetActiveScreenBounds();

            return CaptureRectangle(bounds);
        }
    }
}
