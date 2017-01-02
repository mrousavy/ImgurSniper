using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
        public static System.Drawing.Rectangle allScreens {
            get {
                return new System.Drawing.Rectangle(
                    (int)SystemParameters.VirtualScreenLeft,
                    (int)SystemParameters.VirtualScreenTop,
                    (int)SystemParameters.VirtualScreenWidth,
                    (int)SystemParameters.VirtualScreenHeight);
            }
        }

        public byte[] CroppedImage;
        public Point from, to;

        private bool _drag = false;
        private bool _enableMagnifyer = false;
        private string _path = FileIO._path;


        public ScreenshotWindow(bool AllMonitors) {
            InitializeComponent();

            Position(AllMonitors);
            LoadConfig();

            this.Loaded += delegate {
                this.Activate();
                this.Focus();
            };
        }

        //Position Window correctly
        private void Position(bool AllMonitors) {
            System.Drawing.Rectangle size;

            if(AllMonitors) {
                size = allScreens;
            } else {
                size = screen;
            }

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

            //The Factor for custom Windows Scaling users
            double factorX = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            double factorY = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M22;

            //Width (w) and Height (h) of dragged Rectangle
            double w = Math.Abs(from.X - to.X) * factorX;
            double h = Math.Abs(from.Y - to.Y) * factorY;
            double x = Math.Min(from.X, to.X) * factorX;
            double y = Math.Min(from.Y, to.Y) * factorY;

            if(Math.Abs(to.X - from.X) < 7 || Math.Abs(to.Y - from.Y) < 7) {
                toast.Show("The Image Width and/or Height is too small!", TimeSpan.FromSeconds(3.3));
            } else {
                this.Cursor = Cursors.Arrow;

                //Was cropping successful?
                Complete((int)x, (int)y, (int)w, (int)h);

                this.IsEnabled = false;
            }
        }

        //Mouse Move event
        private void DrawRectangle(object sender, MouseEventArgs e) {
            _drag = e.LeftButton == MouseButtonState.Pressed;
            Point pos = e.GetPosition(this);

            this.Activate();

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
                    string.Format("An error occured! (Show this to the smart Computer Apes: \"{0}\")", ex.Message),
                    TimeSpan.FromSeconds(3.3));
            }

            //Window Cords Display
            this.coords.Content =
                string.Format("x:{0} | y:{1}", (int)pos.X, (int)pos.Y);
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
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            anim.Completed += delegate {
                DialogResult = result;
            };
            anim.From = ContentGrid.Opacity;
            anim.To = 0;

            //Wait delay (ms) and then begin animation
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            ContentGrid.BeginAnimation(OpacityProperty, anim);
        }

        //Fade out window and shoot cropped screenshot
        private void Complete(int x, int y, int w, int h) {
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));

            anim.Completed += delegate {
                Crop(x, y, w, h);
            };

            anim.From = ContentGrid.Opacity;
            anim.To = 0;

            ContentGrid.BeginAnimation(OpacityProperty, anim);
        }

        //Make Image from custom Coords
        private void Crop(int x, int y, int w, int h) {
            bool result = MakeImage(x, y, w, h);

            if(!result) {
                toast.Show("Whoops, something went wrong!", TimeSpan.FromSeconds(3.3));
                CloseSnap(false, 1500);
            } else {
                DialogResult = true;
            }
        }

        //"Crop" Rectangle
        private bool MakeImage(int x, int y, int w, int h) {
            try {
                ImageSource source = Screenshot.getScreenshot(new System.Drawing.Rectangle(x, y, w, h));

                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                Screenshot.MediaImageToDrawingImage(source).Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                CroppedImage = stream.ToArray();

                return true;
            } catch(Exception) {
                return false;
            }
        }

        //Escape Key closes Window
        private void Cancel(object sender, KeyEventArgs e) {
            if(e.Key == Key.Escape) {
                CloseSnap(false, 0);
            }
        }
    }
}
