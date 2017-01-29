using System.Windows.Interop;
using System.Windows.Media;

namespace ImgurSniper {
    public static class UnitsHelper {

        public static double DpiX {
            get {
                Matrix transformToDevice;
                using(var source = new HwndSource(new HwndSourceParameters()))
                    transformToDevice = source.CompositionTarget.TransformToDevice;
                double M11 = transformToDevice.M11;

                double DpiX = M11 * 96;
                return DpiX;
            }
        }

        public static double DpiY {
            get {
                Matrix transformToDevice;
                using(var source = new HwndSource(new HwndSourceParameters()))
                    transformToDevice = source.CompositionTarget.TransformToDevice;
                double M22 = transformToDevice.M22;

                double DpiY = M22 * 96;
                return DpiY;
            }
        }

        public static double DpiXScale {
            get {
                Matrix transformToDevice;
                using(var source = new HwndSource(new HwndSourceParameters()))
                    transformToDevice = source.CompositionTarget.TransformToDevice;
                double M11 = transformToDevice.M11;

                return M11;
            }
        }

        public static double DpiYScale {
            get {
                Matrix transformToDevice;
                using(var source = new HwndSource(new HwndSourceParameters()))
                    transformToDevice = source.CompositionTarget.TransformToDevice;
                double M22 = transformToDevice.M22;

                return M22;
            }
        }

        /// <summary>
        /// Convert Pixels to WPF Points
        /// </summary>
        /// <param name="PixelSize">The Size in Pixels</param>
        /// <returns>The Size in WPF Points</returns>
        public static RECT PixelToPoints(RECT PixelSize) {
            Matrix transformToDevice;
            using(var source = new HwndSource(new HwndSourceParameters()))
                transformToDevice = source.CompositionTarget.TransformToDevice;
            double M11 = transformToDevice.M11;
            double M22 = transformToDevice.M22;

            double DpiX = M11 * 96;
            double DpiY = M22 * 96;

            RECT PointsSize;

            //For 72 points per [dumb american unit] with default of 96 DPI
            PointsSize.Left = PixelSize.Left * 72 / DpiX;
            PointsSize.Top = PixelSize.Top * 72 / DpiY;
            PointsSize.Width = PixelSize.Width * 72 / DpiX;
            PointsSize.Height = PixelSize.Height * 72 / DpiY;

            return PointsSize;
        }

        /// <summary>
        /// Convert WPF Points to Pixels
        /// </summary>
        /// <param name="PixelSize">The Size in WPF Points</param>
        /// <returns>The Size in Pixels</returns>
        public static RECT PointsToPixel(RECT PointsSize) {
            Matrix transformToDevice;
            using(var source = new HwndSource(new HwndSourceParameters()))
                transformToDevice = source.CompositionTarget.TransformToDevice;
            double M11 = transformToDevice.M11;
            double M22 = transformToDevice.M22;

            double DpiX = M11 * 96;
            double DpiY = M22 * 96;

            RECT PixelSize;

            //For 72 points per [dumb american unit] with default of 96 DPI
            PixelSize.Left = PointsSize.Left * DpiX / 72;
            PixelSize.Top = PointsSize.Top * DpiY / 72;
            PixelSize.Width = PointsSize.Width * DpiX / 72;
            PixelSize.Height = PointsSize.Height * DpiY / 72;

            return PixelSize;
        }

        /// <summary>
        /// Scale a given Point with the User's DPIX
        /// </summary>
        /// <param name="UnscaledPointX">The unscaled Point (X)</param>
        /// <returns>The scaled Point (X)</returns>
        public static double ScaleX(double UnscaledPointX) {
            double ScaledPointX = UnscaledPointX;

            ScaledPointX = UnscaledPointX * DpiX / 72;

            return ScaledPointX;
        }

        /// <summary>
        /// Scale a given Point with the User's DPIY
        /// </summary>
        /// <param name="UnscaledPointX">The unscaled Point (Y)</param>
        /// <returns>The scaled Point (Y)</returns>
        public static double ScaleY(double UnscaledPointY) {
            double ScaledPointY = UnscaledPointY;

            ScaledPointY = UnscaledPointY * DpiY / 72;

            return ScaledPointY;
        }

        public struct RECT {
            public RECT(double left, double top, double width, double height) {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
            }

            public double Left;
            public double Top;
            public double Width;
            public double Height;
        }
    }
}
