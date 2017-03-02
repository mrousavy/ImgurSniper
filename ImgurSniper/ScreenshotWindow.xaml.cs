using ImgurSniper.Properties;
using mrousavy;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Path = System.Windows.Shapes.Path;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for ScreenshotWindow.xaml
    /// </summary>
    public partial class ScreenshotWindow {

        private bool _drag;

        public byte[] CroppedImage;
        public Point From, To;
        public string HwndName;
        //Magnifyer for Performance reasons disabled
        //private bool _enableMagnifyer = false;


        public ScreenshotWindow(bool allMonitors) {
            ShowActivated = false;

            InitializeComponent();

            Position(allMonitors);
            //LoadConfig();

            Loaded += WindowLoaded;
        }

        //Size of current Mouse Location screen
        public static Rectangle Screen => System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position).Bounds;

        //Size of whole Screen Array
        public static Rectangle AllScreens => SystemInformation.VirtualScreen;

        private async void WindowLoaded(object sender, RoutedEventArgs e) {
            //this.CaptureMouse();
            //grid.CaptureMouse();
            //PaintSurface.CaptureMouse();
            selectionRectangle.CaptureMouse();
            Topmost = true;

            Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            SelectedMode.Margin = new Thickness(
                bounds.Width / 2 - 50,
                bounds.Height / 2 - 25,
                bounds.Width / 2 - 50,
                bounds.Height / 2 - 25);

            Activate();
            Topmost = true;
            Focus();

            #region Escape

            try {
                //Register Global Escape Hotkey
                HotKey escapeHotKey = new HotKey(ModifierKeys.None, Key.Escape, this);
                escapeHotKey.HotKeyPressed += delegate {
                    CloseSnap(false, 0);
                    escapeHotKey.Dispose();
                    escapeHotKey = null;
                };
            } catch {
                //Register Escape Hotkey for this Window if Global Hotkey fails
                KeyDown += (o, ke) => {
                    if(ke.Key == Key.Escape) {
                        CloseSnap(false, 0);
                    }
                };
            }

            #endregion

            #region Ctrl A

            try {
                //Register Global Ctrl + A Hotkey
                HotKey ctrlAHotKey = new HotKey(ModifierKeys.Control, Key.A, this);
                ctrlAHotKey.HotKeyPressed += delegate {
                    SelectAllCmd();
                    ctrlAHotKey.Dispose();
                    ctrlAHotKey = null;
                };
            } catch {
                //Register Ctrl + A Hotkey for this Window if Global Hotkey fails
                KeyDown += (o, ke) => {
                    if((Control.ModifierKeys & Keys.Control) == Keys.Control) {
                        SelectAllCmd();
                    }
                };
            }

            #endregion

            #region Space

            try {
                //Register Space Hotkey
                HotKey spaceHotKey = new HotKey(ModifierKeys.None, Key.Space, this);
                spaceHotKey.HotKeyPressed += delegate { SwitchMode(); };
            } catch {
                //Register Space Hotkey for this Window if Global Hotkey fails
                KeyDown += (o, ke) => {
                    if(ke.Key == Key.Space) {
                        SwitchMode();
                    }
                };
            }

            #endregion

            //Prevent short flash of Toast
            await Task.Delay(100);
            toast.Visibility = Visibility.Visible;


            //Hide in Alt + Tab Switcher View
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)WinAPI.GetWindowLong(wndHelper.Handle, (int)WinAPI.GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)WinAPI.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinAPI.SetWindowLong(wndHelper.Handle, (int)WinAPI.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        //Position Window correctly
        private void Position(bool allMonitors) {
            Rectangle size = allMonitors ? AllScreens : Screen;

            Left = size.Left;
            Top = size.Top;
            Width = size.Width;
            Height = size.Height;
        }

        //Load from config File (ImgurSniper.UI)
        private void LoadConfig() {
            //_enableMagnifyer = FileIO.MagnifyingGlassEnabled;
            //if(_enableMagnifyer) {
            //    Magnifyer.Visibility = Visibility.Visible;
            //    VisualBrush b = (VisualBrush)MagnifyingEllipse.Fill;
            //    b.Visual = SnipperGrid;
            //}
        }

        private static Rectangle GetRectFromHandle(IntPtr whandle) {
            Rectangle windowSize = WinAPI.GetWindowRectangle(whandle);

            return windowSize;
        }

        //Set Magnifyer Position (Not used, huge Performance cost)
        public void Magnifier(Point pos) {
            //MagnifyerBrush.Viewbox = new Rect(pos.X - 25, pos.Y - 25, 50, 50);

            //double x = pos.X - 35;
            //double y = pos.Y - 80;

            //Magnifyer.Margin = new Thickness(x, y, this.Width - x - 70, this.Height - y - 70);
        }

        //Switch between Rectangle Snapping and Painting
        private void SwitchMode() {
            grid.IsEnabled = !grid.IsEnabled;
            PaintSurface.IsEnabled = !PaintSurface.IsEnabled;

            _currentPath = null;
            selectionRectangle.Margin = new Thickness(99999);

            //Stop animations by setting AnimationTimeline to null
            SelectedMode.BeginAnimation(OpacityProperty, null);

            //Set correct Selected Mode Indicator
            if(grid.IsEnabled) {
                grid.CaptureMouse();
                Cursor = Cursors.Cross;
                CropIcon.Background = Brushes.Gray;
                DrawIcon.Background = Brushes.Transparent;
            } else {
                PaintSurface.CaptureMouse();
                Cursor = Cursors.Pen;
                DrawIcon.Background = Brushes.Gray;
                CropIcon.Background = Brushes.Transparent;
            }

            //Fade Selected Mode View in
            FadeSelectedModeIn();
        }

        //Fade the Selected Mode (Drawing/Rectangle) in
        private void FadeSelectedModeIn() {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            anim.Completed += FadeSelectedModeOut;
            anim.From = SelectedMode.Opacity;
            anim.To = 0.7;

            SelectedMode.BeginAnimation(OpacityProperty, anim);
        }

        //Fade the Selected Mode (Drawing/Rectangle) out
        private void FadeSelectedModeOut(object sender, EventArgs e) {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25)) {
                BeginTime = TimeSpan.FromMilliseconds(1000),
                From = SelectedMode.Opacity,
                To = 0
            };

            SelectedMode.BeginAnimation(OpacityProperty, anim);
        }

        //Play Camera Shutter Sound
        private static void PlayShutter() {
            try {
                MediaPlayer player = new MediaPlayer { Volume = 30 };

                string path = System.IO.Path.Combine(FileIO._programFiles, "Resources\\Camera_Shutter.wav");

                player.Open(new Uri(path));
                player.Play();
            } catch {
                // ignored
            }
        }

        //Make image of whole Window with Ctrl + A
        private void SelectAllCmd() {
            selectionRectangle.Margin = new Thickness(0);

            From = new Point(Left, Top);
            To = new Point(Width, Height);
            FinishRectangle();
        }

        #region Rectangle Mouse Events

        //MouseDown Event
        private void StartDrawing(object sender, MouseButtonEventArgs e) {
            switch(e.ChangedButton) {
                case MouseButton.Right:
                    //!!Not yet fully implemented
                    RightClick();
                    break;
                case MouseButton.Left:
                    //Lock the from Point to the Mouse Position when started holding Mouse Button
                    From = e.GetPosition(this);
                    break;
            }
        }

        //Perform Right click -> Screenshot Window on cursor pos
        private void RightClick() {
            Cursor = Cursors.Hand;

            WinAPI.POINT point;
            WinAPI.User32.GetCursorPos(out point);

            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));

            anim.Completed += async delegate {
                Topmost = false;
                Opacity = 0;

                //For render complete
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                await Task.Delay(50);

                //Send Window to back, so WinAPI.User32.WindowFromPoint does not detect ImgurSniper as Window
                WinAPI.User32.SetWindowPos(new WindowInteropHelper(this).Handle, WinAPI.HWND_BOTTOM, 0, 0, 0, 0,
                    WinAPI.SWP_NOMOVE | WinAPI.SWP_NOSIZE | WinAPI.SWP_NOACTIVATE);

                IntPtr whandle = WinAPI.User32.WindowFromPoint(point);

                WinAPI.User32.SetForegroundWindow(whandle);
                WinAPI.User32.SetActiveWindow(whandle);

                Rectangle hwnd = GetRectFromHandle(whandle);

                const int nChars = 256;
                StringBuilder buff = new StringBuilder(nChars);
                if(WinAPI.User32.GetWindowText(whandle, buff, nChars) > 0) {
                    HwndName = buff.ToString();
                }

                int fromX = hwnd.Left;
                int fromY = hwnd.Top;
                int toX = hwnd.Right;
                int toY = hwnd.Bottom;

                Crop(fromX, fromY, toX, toY);
            };

            anim.From = grid.Opacity;
            anim.To = 0;

            grid.BeginAnimation(OpacityProperty, anim);
        }

        //MouseUp Event
        private void ReleaseRectangle(object sender, MouseButtonEventArgs e) {
            //Only trigger on Left Mouse Button
            if(e.ChangedButton != MouseButton.Left) {
                return;
            }

            To = e.GetPosition(this);
            FinishRectangle();
        }

        //Mouse Move event (Commented out as much as possible for best Performance)
        private void DrawRectangle(object sender, MouseEventArgs e) {
            _drag = e.LeftButton == MouseButtonState.Pressed;

            //this.Activate();

            //if(_enableMagnifyer)
            //    Magnifier(pos);

            //Draw Rectangle
            if(_drag) {
                //Set Crop Rectangle to Mouse Position
                To = e.GetPosition(this);

                //Width (w) and Height (h) of dragged Rectangle
                double w = Math.Abs(From.X - To.X);
                double h = Math.Abs(From.Y - To.Y);
                double left = Math.Min(From.X, To.X);
                double top = Math.Min(From.Y, To.Y);
                double right = Width - left - w;
                double bottom = Height - top - h;

                selectionRectangle.Margin = new Thickness(left, top, right, bottom);
            }

            //Window Cords Display (Disabled for Performance reasons)
            //this.coords.Content =
            //    $"x:{(int)e.GetPosition(this).X} | y:{(int)e.GetPosition(this).Y}";
        }

        #endregion

        #region Painting Mouse Events

        private Point _startPos;
        private Path _currentPath;

        //Draw on the Window
        private void Paint(object sender, MouseEventArgs e) {
            if(e.LeftButton == MouseButtonState.Pressed) {
                if(_currentPath == null) {
                    return;
                }

                PolyLineSegment pls =
                    (PolyLineSegment)((PathGeometry)_currentPath.Data).Figures.Last().Segments.Last();
                pls.Points.Add(e.GetPosition(this));
            }
        }

        //Mouse Down Event - Begin Painting
        private void BeginPaint(object sender, MouseButtonEventArgs e) {
            if(e.ButtonState == MouseButtonState.Pressed) {
                _startPos = e.GetPosition(this);


                _currentPath = new Path {
                    Data = new PathGeometry {
                        Figures = {
                            new PathFigure {
                                StartPoint = _startPos,
                                Segments = {new PolyLineSegment()}
                            }
                        }
                    },
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 4
                };

                PaintSurface.Children.Add(_currentPath);
            }
        }

        //Mouse Up Event - Stop Painting
        private void StopPaint(object sender, MouseButtonEventArgs e) {
            _currentPath = null;
        }

        #endregion

        #region Snap Helper

        //Finish drawing Rectangle
        private void FinishRectangle() {
            //Width (w) and Height (h) of dragged Rectangle
            int toX = (int)Math.Max(From.X, To.X);
            int toY = (int)Math.Max(From.Y, To.Y);
            int fromX = (int)Math.Min(From.X, To.X);
            int fromY = (int)Math.Min(From.Y, To.Y);

            if(Math.Abs(To.X - From.X) < 9 || Math.Abs(To.Y - From.Y) < 9) {
                toast.Show(strings.imgSize, TimeSpan.FromSeconds(3.3));
                selectionRectangle.Margin = new Thickness(99999);
            } else {
                Cursor = Cursors.Arrow;

                //Was cropping successful?
                Complete(fromX, fromY, toX, toY);

                IsEnabled = false;
            }
        }

        //Fade out window and shoot cropped screenshot
        private void Complete(int fromX, int fromY, int toX, int toY) {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));

            anim.Completed += async delegate {
                grid.Opacity = 0;
                //For render complete
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                await Task.Delay(50);

                Crop(fromX, fromY, toX, toY);
            };

            anim.From = grid.Opacity;
            anim.To = 0;

            grid.BeginAnimation(OpacityProperty, anim);
        }

        //Make Image from custom Coords
        private void Crop(int fromX, int fromY, int toX, int toY) {
            //Point to screen for Users with different DPI
            Point xy = PointToScreen(new Point(fromX, fromY));
            fromX = (int)xy.X;
            fromY = (int)xy.Y;
            Point wh = PointToScreen(new Point(toX, toY));
            toX = (int)wh.X;
            toY = (int)wh.Y;

            int w = toX - fromX;
            int h = toY - fromY;

            //Assuming from Point is already top left and not bottom right
            bool result = MakeImage(new Rectangle(fromX, fromY, w, h));

            if(!result) {
                toast.Show(strings.whoops, TimeSpan.FromSeconds(3.3));
                CloseSnap(false, 1500);
            } else {
                DialogResult = true;
            }
        }

        //"Crop" Rectangle
        private bool MakeImage(Rectangle size) {
            try {
                ImageSource source = Screenshot.getScreenshot(size);

                MemoryStream stream = new MemoryStream();

                Screenshot.MediaImageToDrawingImage(source)
                    .Save(stream,
                        FileIO.UsePNG ? ImageFormat.Png : ImageFormat.Jpeg);

                CroppedImage = stream.ToArray();

                PlayShutter();

                return true;
            } catch {
                return false;
            }
        }

        //Close Window with fade out animation
        private void CloseSnap(bool result, int delay) {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            anim.Completed += delegate {
                try {
                    DialogResult = result;
                } catch {
                    // ignored
                }
            };
            anim.From = Opacity;
            anim.To = 0;
            //Wait delay (ms) and then begin animation
            anim.BeginTime = TimeSpan.FromMilliseconds(delay);

            BeginAnimation(OpacityProperty, anim);
        }

        #endregion
    }
}
