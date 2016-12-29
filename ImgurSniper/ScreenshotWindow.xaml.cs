using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for ScreenshotWindow.xaml
    /// </summary>
    public partial class ScreenshotWindow : Window {
        public static System.Drawing.Rectangle screen {
            get {
                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
                return screen.Bounds;
            }
        }

        public byte[] CroppedImage;
        public Point from, to;

        private bool _drag = false;
        private bool _enableMagnifyer = false;


        public ScreenshotWindow(ImageSource source) {
            InitializeComponent();



            this.Left = screen.X;
            this.Top = 0;
            this.Height = (int)source.Height;
            this.Width = (int)source.Width;
            this.img.Source = source;

            this.Loaded += delegate {
                this.Activate();
                this.Focus();
            };

            VisualBrush b = (VisualBrush)MagnifyingEllipse.Fill;
            b.Visual = SnipperGrid;

            LoadConfig();
        }


        private void LoadConfig() {
            try {
                string[] lines = FileIO.ReadConfig();

                foreach(string line in lines) {
                    string[] config = line.Split(':');

                    if(config[0] == "Magnifyer") {
                        _enableMagnifyer = bool.Parse(config[1]);

                        if(_enableMagnifyer)
                            Magnifyer.Visibility = Visibility.Visible;

                        return;
                    }
                }
            } catch(Exception) {
                _enableMagnifyer = false;
            }
        }

        private void StartDrawing(object sender, MouseButtonEventArgs e) {
            //Lock the from Point to the Mouse Position when started holding Mouse Button
            from = e.GetPosition(this);
        }

        private void ReleaseRectangle(object sender, MouseButtonEventArgs e) {
            to = e.GetPosition(this);

            //The Factor for custom Windows Scaling users
            double factor = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            //Width (w) and Height (h) of dragged Rectangle
            double w = Math.Abs(from.X - to.X) * factor;
            double h = Math.Abs(from.Y - to.Y) * factor;
            double x = Math.Min(from.X, to.X) * factor;
            double y = Math.Min(from.Y, to.Y) * factor;

            if(Math.Abs(to.X - from.X) < 7 || Math.Abs(to.Y - from.Y) < 7) {
                toast.Show("The Image Width and/or Height is too small!", TimeSpan.FromSeconds(3.3));
            } else {
                this.Cursor = Cursors.Arrow;


                //Crop the Image with current Size
                bool response = MakeImage((int)x, (int)y, (int)w, (int)h);

                //Was cropping successful?
                if(response) {
                    var converter = new BrushConverter();
                    var brush = (Brush)converter.ConvertFromString("#2196F3");

                    toast.Background = brush;
                    toast.Show(string.Format("Processing Image ({0}x{1})...", (int)w, (int)h), TimeSpan.FromSeconds(1.5));

                    CloseSnap(true, 1500);
                } else {
                    toast.Show("Whoops, something went wrong!", TimeSpan.FromSeconds(3.3));

                    CloseSnap(false, 1500);
                }
                this.IsEnabled = false;
            }
        }

        public void Magnifier(Point pos) {
            MagnifyerBrush.Viewbox = new Rect(pos.X - 25, pos.Y - 25, 50, 50);

            double x = pos.X - 35;
            double y = pos.Y - 80;

            Magnifyer.Margin = new Thickness(x, y, this.Width - x - 70, this.Height - y - 70);
        }

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

        /// <summary>
        /// Close Window with fade out animation
        /// </summary>
        /// <param name="result">Dialog Result</param>
        /// <param name="delay">Delay in milliseconds to close the window</param>
        private async void CloseSnap(bool result, int delay) {
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            anim.Completed += delegate {
                DialogResult = result;
            };
            anim.From = 0.7;
            anim.To = 1;

            //Wait delay (ms) and then begin animation
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            this.BeginAnimation(OpacityProperty, anim);
        }


        private bool MakeImage(int x, int y, int w, int h) {
            try {
                ////Copy Image over
                //BitmapImage src = img.Source as BitmapImage;
                //src.CacheOption = BitmapCacheOption.OnLoad;

                ////Crop Image
                //CroppedBitmap croppedImage = new CroppedBitmap(src, area);

                ////Save Image
                //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                //encoder.QualityLevel = 100;
                //using(MemoryStream stream = new MemoryStream()) {
                //    encoder.Frames.Add(BitmapFrame.Create(croppedImage));
                //    encoder.Save(stream);
                //    CroppedImage = stream.ToArray();
                //    stream.Close();
                //}



                byte[] bimage = Screenshot.ImageToByte(Screenshot.MediaImageToDrawingImage(img.Source));

                using(MemoryStream stream = new MemoryStream(bimage)) {
                    System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(x, y, w, h);
                    System.Drawing.Bitmap src = System.Drawing.Image.FromStream(stream) as System.Drawing.Bitmap;
                    System.Drawing.Bitmap target = new System.Drawing.Bitmap(cropRect.Width, cropRect.Height);

                    using(System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(target)) {
                        g.DrawImage(src, new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                                         cropRect,
                                         System.Drawing.GraphicsUnit.Pixel);
                    }

                    CroppedImage = Screenshot.ImageToByte(target);
                }



                return true;
            } catch(Exception) {
                return false;
            }
        }

        private void Cancel(object sender, KeyEventArgs e) {
            if(e.Key == Key.Escape) {
                CloseSnap(false, 0);
            }
        }
    }
}
