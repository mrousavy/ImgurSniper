using System;
using System.IO;
using System.Threading;
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

        public ScreenshotWindow(ImageSource source) {
            InitializeComponent();

            this.Left = 0;
            this.Top = 0;
            this.Height = source.Height;
            this.Width = source.Width;
            this.img.Source = source;

            //this.Height /= 2;
            //this.Width /= 2;
        }

        private void img_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            //Lock the from Point to the Mouse Position when started holding Mouse Button
            from = e.GetPosition(this);
        }

        private void img_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            to = e.GetPosition(this);
            double width = Math.Abs(from.X - to.X);
            double height = Math.Abs(from.Y - to.Y);
            Int32Rect rect = new Int32Rect((int)from.X, (int)from.Y, (int)width, (int)height);

            if(to.X == from.X && to.Y == to.Y) {
                errorToast.Show("The Image Width and Height cannot be 0!", TimeSpan.FromSeconds(3.3));
            } else {
                //Crop the Image with current Size
                bool response = MakeImage(rect);

                //Was cropping successful?
                if(response) {
                    errorToast.Show("Uploading Image...", TimeSpan.FromSeconds(1.5));

                    CloseSnap(true, errorToast.Duration.Milliseconds + 30);
                } else {
                    errorToast.Show("Whoops, something went wrong!", TimeSpan.FromSeconds(3.3));
                }
            }
        }

        /// <summary>
        /// Close Window with fade out animation
        /// </summary>
        /// <param name="result">Dialog Result</param>
        /// <param name="delay">Delay in milliseconds to close the window</param>
        private void CloseSnap(bool result, int delay) {
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.25));
            anim.Completed += delegate {
                DialogResult = result;
            };
            anim.From = 0.7;
            anim.To = 1;

            if(delay == 0) {
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            } else {
                //Ugly main Thread wait (For delayed closing)..
                new Thread(() => {
                    Thread.Sleep(delay);
                    Application.Current.Dispatcher.Invoke(() => {
                        this.BeginAnimation(UIElement.OpacityProperty, anim);
                    });
                }).Start();
            }
        }

        private bool MakeImage(Int32Rect area) {
            try {
                BitmapImage src = img.Source as BitmapImage;
                src.CacheOption = BitmapCacheOption.OnLoad;

                CroppedBitmap croppedImage = new CroppedBitmap(src, area);

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

        private void Cancel(object sender, System.Windows.Input.KeyEventArgs e) {
            if(e.Key == System.Windows.Input.Key.Escape) {
                CloseSnap(false, 0);
            }
        }

        private void img_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            Point pos = e.GetPosition(this);

            //Set Crop Rectangle to Mouse Position (only if key is down obv.)
            if(e.LeftButton == MouseButtonState.Pressed)
                to = pos;

            //Width (x) and Height (y) of dragged window
            double x = Math.Abs(from.X - to.X);
            double y = Math.Abs(from.Y - to.Y);



            //Window Cords Display
            this.coords.Content = "x:" + pos.X + " | " + "y:" + pos.Y;
        }
    }
}
