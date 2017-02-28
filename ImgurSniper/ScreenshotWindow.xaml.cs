using mrousavy;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Point = System.Windows.Point;

//TODO: Fix for different Resolution users
namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for ScreenshotWindow.xaml
    /// </summary>
    public partial class ScreenshotWindow : Window {

        //Size of current Mouse Location screen
        public static Rectangle screen {
            get {
                var screen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
                return screen.Bounds;
            }
        }

        //Size of whole Screen Array
        public static Rectangle allScreens => System.Windows.Forms.SystemInformation.VirtualScreen;

        public byte[] CroppedImage;
        public Point from, to;
        public string HwndName;

        private bool _drag = false;
        private bool _focusOnLoad;
        //Magnifyer for Performance reasons disabled
        //private bool _enableMagnifyer = false;


        public ScreenshotWindow(bool AllMonitors, bool Focus) {
            this.ShowActivated = false;
            _focusOnLoad = Focus;

            InitializeComponent();

            Position(AllMonitors);
            //LoadConfig();

            this.Loaded += WindowLoaded;
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e) {
            //this.CaptureMouse();
            //grid.CaptureMouse();
            //PaintSurface.CaptureMouse();
            selectionRectangle.CaptureMouse();
            this.Topmost = true;

            Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            SelectedMode.Margin = new Thickness(
                (bounds.Width / 2) - 50,
                (bounds.Height / 2) - 25,
                (bounds.Width / 2) - 50,
                (bounds.Height / 2) - 25);

            if(_focusOnLoad) {
                this.Activate();
                this.Focus();
            }

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
                this.KeyDown += (o, ke) => {
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
                this.KeyDown += (o, ke) => {
                    if(((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control)) {
                        SelectAllCmd();
                    }
                };
            }
            #endregion

            #region Space
            try {
                //Register Space Hotkey
                HotKey spaceHotKey = new HotKey(ModifierKeys.None, Key.Space, this);
                spaceHotKey.HotKeyPressed += delegate {
                    SwitchMode();
                };
            } catch {
                //Register Space Hotkey for this Window if Global Hotkey fails
                this.KeyDown += (o, ke) => {
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
        private void Position(bool AllMonitors) {
            Rectangle size = AllMonitors ? allScreens : screen;

            this.Left = size.Left;
            this.Top = size.Top;
            this.Width = size.Width;
            this.Height = size.Height;
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

        private Rectangle GetRectFromHandle(IntPtr whandle) {
            Rectangle WindowSize = WinAPI.GetWindowRectangle(whandle);

            return WindowSize;
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
                    @from = e.GetPosition(this);
                    break;
            }
        }

        //Perform Right click -> Screenshot Window on cursor pos
        private void RightClick() {
            this.Cursor = Cursors.Hand;

            WinAPI.POINT point;
            WinAPI.User32.GetCursorPos(out point);

            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));

            anim.Completed += delegate {
                this.Topmost = false;
                this.Opacity = 0;

                //For render complete
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);

                //Send Window to back, so WinAPI.User32.WindowFromPoint does not detect ImgurSniper as Window
                WinAPI.User32.SetWindowPos(new WindowInteropHelper(this).Handle, WinAPI.HWND_BOTTOM, 0, 0, 0, 0, WinAPI.SWP_NOMOVE | WinAPI.SWP_NOSIZE | WinAPI.SWP_NOACTIVATE);

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
            if(e.ChangedButton != MouseButton.Left)
                return;

            to = e.GetPosition(this);
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
                to = e.GetPosition(this);

                //Width (w) and Height (h) of dragged Rectangle
                double w = Math.Abs(from.X - to.X);
                double h = Math.Abs(from.Y - to.Y);
                double left = Math.Min(from.X, to.X);
                double top = Math.Min(from.Y, to.Y);
                double right = this.Width - left - w;
                double bottom = this.Height - top - h;

                selectionRectangle.Margin = new Thickness(left, top, right, bottom);
            }

            //Window Cords Display (Disabled for Performance reasons)
            //this.coords.Content =
            //    $"x:{(int)e.GetPosition(this).X} | y:{(int)e.GetPosition(this).Y}";
        }

        #endregion

        #region Painting Mouse Events
        private Point _startPos;
        private System.Windows.Shapes.Path _currentPath;

        //Draw on the Window
        private void Paint(object sender, MouseEventArgs e) {
            if(e.LeftButton == MouseButtonState.Pressed) {
                if(_currentPath == null)
                    return;

                PolyLineSegment pls = (PolyLineSegment)((PathGeometry)_currentPath.Data).Figures.Last().Segments.Last();
                pls.Points.Add(e.GetPosition(this));
            }
        }

        //Mouse Down Event - Begin Painting
        private void BeginPaint(object sender, MouseButtonEventArgs e) {
            if(e.ButtonState == MouseButtonState.Pressed) {
                _startPos = e.GetPosition(this);


                _currentPath = new System.Windows.Shapes.Path {
                    Data = new PathGeometry {
                        Figures = { new PathFigure
                    {
                        StartPoint = _startPos,
                        Segments = { new PolyLineSegment() }
                    }}
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
            int toX = (int)Math.Max(from.X, to.X);
            int toY = (int)Math.Max(from.Y, to.Y);
            int fromX = (int)Math.Min(from.X, to.X);
            int fromY = (int)Math.Min(from.Y, to.Y);

            if(Math.Abs(to.X - from.X) < 9 || Math.Abs(to.Y - from.Y) < 9) {
                toast.Show(Properties.strings.imgSize, TimeSpan.FromSeconds(3.3));
                selectionRectangle.Margin = new Thickness(99999);
            } else {
                this.Cursor = Cursors.Arrow;

                //Was cropping successful?
                Complete(fromX, fromY, toX, toY);

                this.IsEnabled = false;
            }
        }

        //Fade out window and shoot cropped screenshot
        private void Complete(int fromX, int fromY, int toX, int toY) {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));

            anim.Completed += delegate {
                grid.Opacity = 0;
                //For render complete
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);

                Crop(fromX, fromY, toX, toY);
            };

            anim.From = grid.Opacity;
            anim.To = 0;

            grid.BeginAnimation(OpacityProperty, anim);
        }

        //Make Image from custom Coords
        private void Crop(int fromX, int fromY, int toX, int toY) {
            //Point to screen for Users with different DPI
            Point xy = this.PointToScreen(new Point(fromX, fromY));
            fromX = (int)xy.X;
            fromY = (int)xy.Y;
            Point wh = this.PointToScreen(new Point(toX, toY));
            toX = (int)wh.X;
            toY = (int)wh.Y;

            int w = toX - fromX;
            int h = toY - fromY;

            //Assuming from Point is already top left and not bottom right
            bool result = MakeImage(new Rectangle(fromX, fromY, w, h));

            if(!result) {
                toast.Show(Properties.strings.whoops, TimeSpan.FromSeconds(3.3));
                CloseSnap(false, 1500);
            } else {
                DialogResult = true;
            }
        }

        //"Crop" Rectangle
        private bool MakeImage(Rectangle size) {
            try {
                ImageSource source = Screenshot.getScreenshot(size);

                System.IO.MemoryStream stream = new System.IO.MemoryStream();

                Screenshot.MediaImageToDrawingImage(source)
                    .Save(stream,
                        FileIO.UsePNG ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg);

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
            anim.From = this.Opacity;
            anim.To = 0;
            //Wait delay (ms) and then begin animation
            anim.BeginTime = TimeSpan.FromMilliseconds(delay);

            this.BeginAnimation(OpacityProperty, anim);
        }
        #endregion

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
                CropIcon.Background = System.Windows.Media.Brushes.Gray;
                DrawIcon.Background = System.Windows.Media.Brushes.Transparent;
            } else {
                PaintSurface.CaptureMouse();
                DrawIcon.Background = System.Windows.Media.Brushes.Gray;
                CropIcon.Background = System.Windows.Media.Brushes.Transparent;
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
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            anim.BeginTime = TimeSpan.FromMilliseconds(1000);
            anim.From = SelectedMode.Opacity;
            anim.To = 0;

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

            from = new Point(0, 0);
            to = new Point(this.Width, this.Height);
            FinishRectangle();
        }
    }
}
