using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for Snipe.xaml
    /// </summary>
    public partial class Snipe : Window {
        private string _dir {
            get {
                return FileIO._path;
            }
        }
        private ImgurIO _imgur;

        public Snipe() {
            InitializeComponent();

            this.Top = 0;
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Left = 0;

            if(!Directory.Exists(_dir)) {
                Directory.CreateDirectory(_dir);
            }

            _imgur = new ImgurIO();

            Crop();
        }

        /// <summary>
        /// Make Screenshot, Let user Crop, Upload Picture and Copy Link to Clipboard
        /// </summary>
        private async void Crop() {
            ScreenshotWindow window = new ScreenshotWindow(Screenshot.getScreenshot());
            window.ShowDialog();

            if(window.DialogResult == true) {
                byte[] cimg = window.CroppedImage;

                long time = DateTime.Now.ToFileTimeUtc();
                File.WriteAllBytes(_dir + string.Format("\\Snipe_{0}.png", time), cimg);

                string response = await _imgur.Upload(cimg);

                if(response.StartsWith("Error:")) {
                    //Some Error happened

                    toast.Show(response);
                } else {
                    //Copy Link to Clipboard
                    Clipboard.SetText(response);

                    toast.Show("Link to Imgur copied to Clipboard!");
                }
            }
            DelayedClose(3300);
        }

        private async void DelayedClose(int Delay) {
            await Task.Delay(TimeSpan.FromMilliseconds(Delay));
            this.Close();
        }
    }
}
