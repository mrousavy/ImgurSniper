using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.Native;
using ImgurSniper.Properties;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for GifWindow.xaml
    /// </summary>
    public partial class GifWindow : IDisposable {
        private bool _drag;

        public Point From, To;
        public string HwndName;
        public bool Error = true;
        public string ErrorMsg;

        public MemoryStream SelectionStream { get; private set; }


        public GifWindow() {
#if DEBUG
            Topmost = false;
#else
            Topmost = true;
#endif

            InitializeComponent();

            Position();
            //LoadConfig();
        }

        ~GifWindow() {
            Dispose();
        }

        //Size of current Mouse Location screen
        public static Rectangle Screen
            => System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;

        //Size of whole Screen Array
        public static Rectangle AllScreens => SystemInformation.VirtualScreen;

        //Position Window correctly
        private void Position() {
            Rectangle size = ConfigHelper.AllMonitors ? AllScreens : Screen;

            Left = size.Left;
            Top = size.Top;
            Width = size.Width;
            Height = size.Height;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            SelectionRectangle.CaptureMouse();

            Activate();
            Focus();

            //Hide in Alt + Tab Switcher View
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)NativeMethods.GetWindowLong(wndHelper.Handle, (int)NativeMethods.GetWindowLongFields.GwlExstyle);

            exStyle |= (int)NativeMethods.ExtendedWindowStyles.WsExToolwindow;
            NativeMethods.SetWindowLong(wndHelper.Handle, (int)NativeMethods.GetWindowLongFields.GwlExstyle, (IntPtr)exStyle);
        }

        //All Keys
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    //Close
                    Error = false;
                    CloseSnap(false);
                    break;
                case Key.A:
                    //Select All
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                        SelectAllCmd();
                    break;
            }
        }

        //Make image of whole Window with Ctrl + A
        private void SelectAllCmd() {
            SelectionRectangle.Margin = new Thickness(0);

            From = new Point(0, 0);
            To = new Point(Width, Height);
            FinishRectangle();
        }

        #region Rectangle Mouse Events

        //MouseDown Event
        private void StartDrawing(object sender, MouseButtonEventArgs e) {
            switch (e.ChangedButton) {
                case MouseButton.Right:
                    RightClick();
                    break;
                case MouseButton.Left:
                    //Lock the from Point to the Mouse Position when started holding Mouse Button
                    From = e.GetPosition(null);
                    break;
            }
        }

        //Perform Right click -> Screenshot Window on cursor pos
        private async void RightClick() {
            Cursor = Cursors.Hand;

            NativeMethods.GetCursorPos(out NativeStructs.POINT point);

            //Fade out
            await Grid.AnimateAsync(OpacityProperty, Grid.Opacity, 0, 250);

            Topmost = false;
            Opacity = 0;

            //For render complete
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
            await Task.Delay(50);

            //Send Window to back, so WinAPI.User32.WindowFromPoint does not detect ImgurSniper as Window
            NativeMethods.SetWindowPos(new WindowInteropHelper(this).Handle, NativeMethods.HwndBottom, 0, 0, 0, 0,
                NativeMethods.SwpNomove | NativeMethods.SwpNosize | NativeMethods.SwpNoactivate);

            IntPtr whandle = NativeMethods.WindowFromPoint(point);

            NativeMethods.SetForegroundWindow(whandle);
            NativeMethods.SetActiveWindow(whandle);

            Rectangle hwnd = NativeMethods.GetWindowRectangle(whandle);

            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            if (NativeMethods.GetWindowText(whandle, buff, nChars) > 0) {
                HwndName = buff.ToString();
            }

            Point to = new Point(hwnd.Left + hwnd.Width, hwnd.Top + hwnd.Height);
            Point from = new Point(hwnd.Left, hwnd.Top);

            Crop(from, to);
        }

        //MouseUp Event
        private void ReleaseRectangle(object sender, MouseButtonEventArgs e) {
            //Only trigger on Left Mouse Button
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            To = e.GetPosition(null);
            FinishRectangle();
        }

        //Mouse Move event (Commented out as much as possible for best Performance)
        private void DrawRectangle(object sender, MouseEventArgs e) {
            _drag = e.LeftButton == MouseButtonState.Pressed;

            //Draw Rectangle
            if (_drag) {
                //Set Crop Rectangle to Mouse Position
                To = e.GetPosition(null);

                //Width (w) and Height (h) of dragged Rectangle
                double w = Math.Abs(From.X - To.X);
                double h = Math.Abs(From.Y - To.Y);
                double left = Math.Min(From.X, To.X);
                double top = Math.Min(From.Y, To.Y);
                double right = Width - left - w;
                double bottom = Height - top - h;

                SelectionRectangle.Margin = new Thickness(left, top, right, bottom);
            }
        }

        #endregion

        #region Snap Helper

        //Finish drawing Rectangle
        private async void FinishRectangle() {
            //From and To Point -> PointToScreen for different DPI
            Point from = new Point((int)Math.Min(From.X, To.X), (int)Math.Min(From.Y, To.Y));
            Point to = new Point((int)Math.Max(From.X, To.X), (int)Math.Max(From.Y, To.Y));
            from = PointToScreen(from);
            to = PointToScreen(to);

            if (Math.Abs(To.X - From.X) < 9 || Math.Abs(To.Y - From.Y) < 9) {
                // Too small
                SelectionRectangle.Margin = new Thickness(99999);
            } else {
                //Prevent input
                IsEnabled = false;

                Cursor = Cursors.Arrow;

                //Fade out animation
                await Grid.AnimateAsync(OpacityProperty, Grid.Opacity, 0, 150);
                //Fade out render complete
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
                await Task.Delay(100);

                //Crop Image
                Crop(from, to);
            }
        }

        //Make Image from custom Coords
        private void Crop(Point from, Point to) {
            int w = (int)(to.X - from.X);
            int h = (int)(to.Y - from.Y);

            try {
                Rectangle size = new Rectangle((int)from.X, (int)from.Y, w, h);

                using (GifRecorder recorder = new GifRecorder(size)) {
                    bool? result = recorder.ShowDialog();

                    if (result != true) {
                        CloseSnap(false, 0, recorder.ErrorMsg);
                        return;
                    }

                    SelectionStream = recorder.Gif;
                }

                CloseSnap(true);
            } catch {
                CloseSnap(false, 0, strings.couldNotStartRecording);
            }
        }

        //Close Window with fade out animation
        private async void CloseSnap(bool result, int delay = 0, string errorMessage = null) {
            await this.AnimateAsync(OpacityProperty, Opacity, 0, 150, delay);

            try {
                if (result) {
                    await ScreenshotHelper.FinishGif(SelectionStream, HwndName);
                    try {
                        DialogResult = true;
                    } catch {
                        // not dialog
                    }
                    return;
                }
                if (Error) {
                    await Statics.ShowNotificationAsync(string.Format(strings.uploadingErrorGif, errorMessage),
                        NotificationWindow.NotificationType.Error);
                }
            } catch {
                // could not finish screenshot
            }
            try {
                DialogResult = false;
            } catch {
                // not dialog
            }
        }

        #endregion

        public void Dispose() {
            SelectionStream?.Dispose();
            SelectionStream = null;

            try {
                Close();
            } catch {
                //Window already closed
            }

            GC.Collect();
        }
    }
}