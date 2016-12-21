using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for ScreenshotWindow.xaml
    /// </summary>
    public partial class ScreenshotWindow : Window {
        public byte[] CroppedImage;
        public Point from, to;
        private bool drag = false;

        public ScreenshotWindow(ImageSource source) {
            InitializeComponent();

            this.Left = 0;
            this.Top = 0;
            this.Height = (int)source.Height;
            this.Width = (int)source.Width;
            this.img.Source = source;

            //this.Activate();
        }

        private void StartDrawing(object sender, MouseButtonEventArgs e) {
            //Lock the from Point to the Mouse Position when started holding Mouse Button
            from = e.GetPosition(this);

            //selectionRectangle.Margin = new Thickness(from.X, from.Y, this.Width - from.X - 30, this.Height - from.Y - 30);
            selectionRectangle.Visibility = Visibility.Visible;
        }

        private void ReleaseRectangle(object sender, MouseButtonEventArgs e) {
            to = e.GetPosition(this);

            //Width (w) and Height (h) of dragged Rectangle
            double w = Math.Abs(from.X - to.X);
            double h = Math.Abs(from.Y - to.Y);
            double x = Math.Min(from.X, to.X);
            double y = Math.Min(from.Y, to.Y);

            Int32Rect rect = new Int32Rect((int)x, (int)y, (int)w, (int)h);

            if(to.X == from.X || to.Y == from.Y) {
                toast.Show("The Image Width and Height cannot be 0!", TimeSpan.FromSeconds(3.3));
            } else {
                this.Cursor = Cursors.Arrow;
                //Crop the Image with current Size
                bool response = MakeImage(rect);

                //Was cropping successful?
                if(response) {
                    var converter = new System.Windows.Media.BrushConverter();
                    var brush = (Brush)converter.ConvertFromString("#2196F3");

                    toast.Background = brush;
                    toast.Show("Uploading Image...", TimeSpan.FromSeconds(1.5));

                    CloseSnap(true, 1500);
                } else {
                    toast.Show("Whoops, something went wrong!", TimeSpan.FromSeconds(3.3));
                }
                this.IsEnabled = false;
            }
        }

        private void DrawRectangle(object sender, MouseEventArgs e) {
            drag = e.LeftButton == MouseButtonState.Pressed;

            //Draw Rectangle
            try {
                if(drag) {

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
            } catch(Exception ex) {
                toast.Show("An error occured! (Show this to the smart Computer Apes: \"" + ex.Message + "\")", TimeSpan.FromSeconds(3.3));
            }

            //Window Cords Display
            this.coords.Content = "x:" + to.X + " | " + "y:" + to.Y;
        }

        /// <summary>
        /// Close Window with fade out animation
        /// </summary>
        /// <param name="result">Dialog Result</param>
        /// <param name="delay">Delay in milliseconds to close the window</param>
        private async void CloseSnap(bool result, int delay) {
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.25));
            anim.Completed += delegate {
                DialogResult = result;
            };
            anim.From = 0.7;
            anim.To = 1;

            //Wait delay (ms) and then begin animation
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }


        private bool MakeImage(Int32Rect area) {
            try {
                //Copy Image over
                BitmapImage src = img.Source as BitmapImage;
                src.CacheOption = BitmapCacheOption.OnLoad;

                //Crop Image
                CroppedBitmap croppedImage = new CroppedBitmap(src, area);

                //Save Image
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 100;
                using(MemoryStream stream = new MemoryStream()) {
                    encoder.Frames.Add(BitmapFrame.Create(croppedImage));
                    encoder.Save(stream);
                    CroppedImage = stream.ToArray();
                    stream.Close();
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
