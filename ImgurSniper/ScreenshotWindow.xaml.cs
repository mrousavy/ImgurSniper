using System.IO;
using System.Windows;
using System.Windows.Media;
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
        }

        private void img_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            from = e.GetPosition(this);
        }

        private void img_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            to = e.GetPosition(this);

            if(to.X == from.X && to.Y == to.Y) {
                System.Windows.Forms.MessageBox.Show("The Image Width and Height cannot be 0!", "Image too small");
            } else {

                int width = (int)this.Width, height = (int)this.Height;
                //while(CroppedImage.Length > 2000000) {
                    MakeImage(width, height);

                    //width -= 10;
                    //height -= 10;
                //}

                DialogResult = true;
            }
        }

        private void MakeImage(int width, int height) {
            BitmapImage src = img.Source as BitmapImage;
            src.CacheOption = BitmapCacheOption.OnLoad;

            CroppedBitmap croppedImage = new CroppedBitmap(src, new Int32Rect((int)from.X, (int)from.Y, (int)(to.X - from.X), (int)(to.Y - from.Y)));

            //BitmapImage resized = Screenshot.ResizeImage(croppedImage, width, height);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 100;
            using(MemoryStream stream = new MemoryStream()) {
                encoder.Frames.Add(BitmapFrame.Create(croppedImage.Source));
                encoder.Save(stream);
                CroppedImage = stream.ToArray();
                stream.Close();
            }
        }

        private void img_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            this.coords.Content = "x:" + e.GetPosition(this).X + " | " + "y:" + e.GetPosition(this).Y;
        }
    }
}
