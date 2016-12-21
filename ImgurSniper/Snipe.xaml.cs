using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Width = SystemParameters.WorkArea.Width;
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
            this.Topmost = true;

            if(window.DialogResult == true) {
                Visibility = Visibility.Visible;

                byte[] cimg = window.CroppedImage;

                string response = "Error: Could not Read Config or Upload Image!";
                string successmessage = "";

                try {
                    string[] lines = FileIO.ReadConfig();
                    if(lines.Length < 1) {
                        response = await UploadImgur(cimg);
                        successmessage = "Link to Imgur copied to Clipboard!";
                    }

                    foreach(string line in lines) {
                        string[] config = line.Split(':');

                        if(config[0] == "SaveImages") {
                            //Config: Save Image locally?
                            if(bool.Parse(config[1])) {
                                long time = DateTime.Now.ToFileTimeUtc();
                                File.WriteAllBytes(_dir + string.Format("\\Snipe_{0}.png", time), cimg);
                            }
                        } else if(config[0] == "AfterSnipeAction") {
                            //Config: Upload Image to Imgur or Copy to Clipboard?

                            if(config[1] == "Imgur") {
                                response = await UploadImgur(cimg);
                                successmessage = "Link to Imgur copied to Clipboard!";
                            } else {
                                CopyClipboard(cimg);
                                response = "Image was copied to Clipboard!";
                                successmessage = "Image was copied to Clipboard!";
                            }
                        }
                    }
                } catch(Exception) {
                    File.Delete(FileIO._config);
                }


                if(response.StartsWith("Error:")) {
                    //Some Error happened, show Error Message (response)
                    toast.Show(response);
                } else {
                    var converter = new BrushConverter();
                    var brush = (Brush)converter.ConvertFromString("#2196F3");
                    toast.Background = brush;
                    toast.Show(successmessage);
                }
                DelayedClose(3300);
            } else {
                DelayedClose(0);
            }
        }


        /// <summary>
        /// Upload Image to Imgur
        /// </summary>
        /// <returns>Imgur URL</returns>
        private async Task<string> UploadImgur(byte[] cimg) {
            string response = await _imgur.Upload(cimg);

            //Copy Link to Clipboard
            Clipboard.SetText(response);

            return response;
        }


        private void CopyClipboard(byte[] cimg) {
            //Parse byte[] to Images
            var image = new System.Windows.Media.Imaging.BitmapImage();
            using(var mem = new MemoryStream(cimg)) {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();

            //Copy whole Image to Clipboard
            Clipboard.SetImage(image);
        }

        private async void DelayedClose(int Delay) {
            await Task.Delay(TimeSpan.FromMilliseconds(Delay));
            this.Close();
        }
    }
}
