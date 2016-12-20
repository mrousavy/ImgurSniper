using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for Snipe.xaml
    /// </summary>
    public partial class Snipe : Window {
        private string _dir, _keyPath;

        public Snipe() {
            InitializeComponent();

            this.Top = SystemParameters.PrimaryScreenWidth - this.Height;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Left = 0;

            _dir = Directory.GetCurrentDirectory();
            _keyPath = Path.Combine(_dir, "ImgurAppKey.txt");

            //TODO: Imgur Login
            if(File.Exists(_keyPath)) {
            } else {
            }

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

                File.WriteAllBytes(_dir + "\\test.png", cimg);
                Process.Start(_dir + "\\test.png");

                try {
                    string link = await Upload(cimg);
                    System.Windows.Clipboard.SetText(link);
                    toast.Show("Link to Imgur copied to Clipboard!");
                } catch(Exception) {
                    //TODO: Fancy error
                    System.Windows.Forms.MessageBox.Show("File size too Large", "Error");
                }
            }
            this.Close();
        }


        /// <summary>
        /// Upload bytes (Image) to Imgur
        /// </summary>
        /// <param name="image">The image to upload</param>
        private async Task<string> Upload(byte[] image) {
            //TODO: Upload Image to Imgur and return Link
            return "";
        }
    }
}
