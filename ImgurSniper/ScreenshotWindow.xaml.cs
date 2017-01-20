using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Point = System.Windows.Point;

//TODO: Fix for different Resolution users
namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for ScreenshotWindow.xaml
    /// </summary>
    public partial class ScreenshotWindow : Window {

        //Size of current Mouse Location screen
        public static System.Drawing.Rectangle screen {
            get {
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
                return screen.Bounds;
            }
        }

        //Size of whole Screen Array
        public static System.Drawing.Rectangle allScreens => System.Windows.Forms.SystemInformation.VirtualScreen;

        public byte[] CroppedImage;
        public Point from, to;

        private bool _drag = false;
        private bool _enableMagnifyer = false;
        private string _path = FileIO._path;


        public ScreenshotWindow(bool AllMonitors) {
            this.ShowActivated = false;

            InitializeComponent();

            Position(AllMonitors);
            LoadConfig();


            this.Loaded += delegate {
                this.CaptureMouse();

                HotKey escapeHotKey = new HotKey(ModifierKeys.None, Key.Escape, this);
                escapeHotKey.HotKeyPressed += delegate {
                    CloseSnap(false, 0);
                    escapeHotKey.Dispose();
                    escapeHotKey = null;
                };

                HotKey ctrlAHotKey = new HotKey(ModifierKeys.Control, Key.A, this);
                ctrlAHotKey.HotKeyPressed += delegate {
                    SelectAllCmd();
                    ctrlAHotKey.Dispose();
                    ctrlAHotKey = null;
                };
            };
        }

        //Position Window correctly
        private void Position(bool AllMonitors) {
            Rectangle size = AllMonitors ? allScreens : screen;

            this.Left = size.Left;
            this.Top = size.Top;
            this.Height = size.Height;
            this.Width = size.Width;
        }

        //Load from config File (ImgurSniper.UI)
        private void LoadConfig() {
            _enableMagnifyer = Snipe.MagnifyingGlassEnabled;
            if(_enableMagnifyer) {
                Magnifyer.Visibility = Visibility.Visible;
                VisualBrush b = (VisualBrush)MagnifyingEllipse.Fill;
                b.Visual = SnipperGrid;
            }
        }

        //MouseDown Event
        private void StartDrawing(object sender, MouseButtonEventArgs e) {
            //Only trigger on Left Mouse Button
            if(e.ChangedButton != MouseButton.Left)
                return;

            //Lock the from Point to the Mouse Position when started holding Mouse Button
            from = e.GetPosition(this);
        }

        //MouseUp Event
        private void ReleaseRectangle(object sender, MouseButtonEventArgs e) {
            //Only trigger on Left Mouse Button
            if(e.ChangedButton != MouseButton.Left)
                return;

            to = e.GetPosition(this);
            FinishRectangle();
        }

        //Mouse Move event
        private void DrawRectangle(object sender, MouseEventArgs e) {
            _drag = e.LeftButton == MouseButtonState.Pressed;
            Point pos = e.GetPosition(this);

            //this.Activate();

            if(_enableMagnifyer)
                Magnifier(pos);

            //Draw Rectangle
            try {
                if(_drag) {
                    //Set Crop Rectangle to Mouse Position
                    to = pos;

                    //Width (w) and Height (h) of dragged Rectangle
                    double w = Math.Abs(from.X - to.X);
                    double h = Math.Abs(from.Y - to.Y);
                    double left = Math.Min(from.X, to.X);
                    double top = Math.Min(from.Y, to.Y);
                    double right = this.Width - left - w;
                    double bottom = this.Height - top - h;

                    selectionRectangle.Margin = new Thickness(left, top, right, bottom);
                }
            } catch(Exception ex) {
                toast.Show(
                    $"An error occured! (Show this to the smart Computer Apes: \"{ex.Message}\")",
                    TimeSpan.FromSeconds(3.3));
            }

            //Window Cords Display (Disabled for Performance reasons
            //this.coords.Content =
            //    string.Format("x:{0} | y:{1}", (int)pos.X, (int)pos.Y);
        }

        //Finish drawing Rectangle
        private void FinishRectangle() {
            //Width (w) and Height (h) of dragged Rectangle
            int toX = (int)Math.Max(from.X, to.X);
            int toY = (int)Math.Max(from.Y, to.Y);
            int fromX = (int)Math.Min(from.X, to.X);
            int fromY = (int)Math.Min(from.Y, to.Y);

            if(Math.Abs(to.X - from.X) < 7 || Math.Abs(to.Y - from.Y) < 7) {
                toast.Show("The Image Width and/or Height is too small!", TimeSpan.FromSeconds(3.3));
            } else {
                this.Cursor = Cursors.Arrow;

                //Was cropping successful?
                Complete(fromX, fromY, toX, toY);

                this.IsEnabled = false;
            }
        }


        //Set Magnifyer Position
        public void Magnifier(Point pos) {
            MagnifyerBrush.Viewbox = new Rect(pos.X - 25, pos.Y - 25, 50, 50);

            double x = pos.X - 35;
            double y = pos.Y - 80;

            Magnifyer.Margin = new Thickness(x, y, this.Width - x - 70, this.Height - y - 70);
        }

        //Close Window with fade out animation
        private async void CloseSnap(bool result, int delay) {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            anim.Completed += delegate {
                try {
                    DialogResult = result;
                } catch(Exception) { }
            };
            anim.From = ContentGrid.Opacity;
            anim.To = 0;

            //Wait delay (ms) and then begin animation
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            ContentGrid.BeginAnimation(OpacityProperty, anim);
        }

        //Fade out window and shoot cropped screenshot
        private void Complete(int fromX, int fromY, int toX, int toY) {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));

            anim.Completed += async delegate {
                grid.Opacity = 0;
                //For render complete
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
            Point xy = this.PointToScreen(new Point(fromX, fromY));
            fromX = (int)xy.X;
            fromY = (int)xy.Y;
            Point wh = this.PointToScreen(new Point(toX, toY));
            toX = (int)wh.X;
            toY = (int)wh.Y;

            int w = toX - fromX;
            int h = toY - fromY;

            //Assuming from Point is already top left and not bottom right
            bool result = MakeImage(new System.Drawing.Rectangle(fromX, fromY, w, h));

            if(!result) {
                toast.Show("Whoops, something went wrong!", TimeSpan.FromSeconds(3.3));
                CloseSnap(false, 1500);
            } else {
                DialogResult = true;
            }
        }

        //Play Camera Shutter Sound
        private static void PlayShutter() {
            try {
                MediaPlayer player = new MediaPlayer();
                player.Volume = 30;

                string path = System.IO.Path.Combine(FileIO._programFiles, "Resources\\Camera_Shutter.wav");

                player.Open(new Uri(path));
                player.Play();
            } catch(Exception) {
                // ignored
            }
        }

        //"Crop" Rectangle
        private bool MakeImage(System.Drawing.Rectangle size) {
            try {
                ImageSource source = Screenshot.getScreenshot(size);

                System.IO.MemoryStream stream = new System.IO.MemoryStream();

                Screenshot.MediaImageToDrawingImage(source)
                    .Save(stream,
                        Snipe.UsePNG ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg);

                CroppedImage = stream.ToArray();

                PlayShutter();

                return true;
            } catch(Exception) {
                return false;
            }
        }

        private void SelectAllCmd() {
            selectionRectangle.Margin = new Thickness(0);

            from = new Point(0, 0);
            to = new Point(this.Width, this.Height);
            FinishRectangle();
        }
    }
}
