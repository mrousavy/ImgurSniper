using ImgurSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for Snipe.xaml
    /// </summary>
    public partial class Snipe : Window {
        private string _dir, _keyPath, _applicationId;

        public Snipe() {
            InitializeComponent();
            this.Hide();

            _dir = Directory.GetCurrentDirectory();
            _keyPath = Path.Combine(_dir, "ImgurAppKey.txt");

            //_applicationId = "4d2e45c3e7d07dc";

            if(File.Exists(_keyPath)) {
                _applicationId = File.ReadAllText(_keyPath);
            } else {
                using(FileStream fs = File.Create(_keyPath)) { }
                System.Windows.Forms.MessageBox.Show("Your Imgur API App/Client ID was not found.\n" +
                    "Please write your ID into " + _keyPath, "No API ID", MessageBoxButtons.OK);
                Process.Start(@"https://api.imgur.com/oauth2/addclient");
                Process.GetCurrentProcess().Kill();
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

                try {
                    string link = await Upload(cimg);
                    System.Windows.Clipboard.SetText(link);
                } catch(Exception) {
                    System.Windows.Forms.MessageBox.Show("File size too Large", "Error");
                }
                this.Close();
            }
        }


        /// <summary>
        /// Upload bytes (Image) to Imgur
        /// </summary>
        /// <param name="image">The image to upload</param>
        private async Task<string> Upload(byte[] image) {
            Imgur imgur = new Imgur(_applicationId);
            ImgurImage x = await imgur.UploadImageAnonymous(
                new MemoryStream(image),
                "ImgurSniper Upload @" + DateTime.Now.ToString(),
                "ImgurSniper Upload @" + DateTime.Now.ToString(),
                "Screenshot from ImgurSniper (www.github.com/mrousavy/ImgurSniper)");

            return x.Link;
        }
    }
}
