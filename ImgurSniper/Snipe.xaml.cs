using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for Snipe.xaml
    /// </summary>
    public partial class Snipe : Window {
        private string _dir;
        private ImgurIO _imgur;
        public static bool MagnifyingGlassEnabled {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "Magnifyer") {
                            return bool.Parse(config[1]);
                        }
                    }
                    return false;
                } catch(Exception) {
                    return false;
                }
            }
        }

        public static bool AllMonitors {
            get {
                try {
                    bool all = false;

                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "SnipeMonitor") {
                            all = config[1] == "All";
                            break;
                        }
                    }

                    return all;
                } catch(Exception) {
                    return false;
                }
            }
        }

        public Snipe() {
            InitializeComponent();

            _dir = FileIO._path;
            //Get configured Path
            string[] lines = FileIO.ReadConfig();
            foreach(string line in lines) {
                string[] config = line.Split(';');

                if(config[0] == "Path") {
                    _dir = config[1];
                    break;
                }
            }

            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Width = SystemParameters.WorkArea.Width;
            this.Left = ScreenshotWindow.screen.X;

            if(!Directory.Exists(_dir)) {
                Directory.CreateDirectory(_dir);
            }

            _imgur = new ImgurIO();

            Crop();
        }

        /// <summary>
        /// Make Screenshot, Let user Crop, Upload Picture and Copy Link to Clipboard
        /// </summary>
        private void Crop() {
            string[] lines = FileIO.ReadConfig();

            ScreenshotWindow window = new ScreenshotWindow(Screenshot.getScreenshot(AllMonitors));
            window.ShowDialog();
            this.Topmost = true;

            if(window.DialogResult == true) {
                Visibility = Visibility.Visible;

                byte[] cimg = window.CroppedImage;

                if(!FileIO.TokenExists && cimg.Length >= 10240000) {
                    ErrorToast.Show("Image Size exceeds 10MB, to increase this please Login to Imgur!", TimeSpan.FromSeconds(3));
                    DelayedClose(3300);
                    return;
                }

                string KB = string.Format("{0:0.#}", (cimg.Length / 1000d));
                SuccessToast.Show(string.Format("Processing Image... ({0}KB)", KB), TimeSpan.FromSeconds(1));

                try {
                    bool Imgur = true;

                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        switch(config[0]) {
                            case "SaveImages":
                                //Config: Save Image locally?
                                if(bool.Parse(config[1])) {
                                    long time = DateTime.Now.ToFileTimeUtc();
                                    File.WriteAllBytes(_dir + string.Format("\\Snipe_{0}.png", time), cimg);
                                }
                                break;
                            case "AfterSnipeAction":
                                //Config: Upload Image to Imgur or Copy to Clipboard?
                                if(config[1] != "Imgur") {
                                    Imgur = false;
                                }
                                break;
                        }
                    }

                    if(Imgur) {
                        UploadImageToImgur(cimg);
                    } else {
                        CopyImageToClipboard(cimg);
                    }

                } catch(Exception ex) {
                    File.Delete(FileIO._config);

                    ErrorToast.Show(string.Format("An unknown Error occured! (Show this to the Smart Computer Apes: \"{0}\")", ex),
                        TimeSpan.FromSeconds(3.5));
                }


                DelayedClose(3500);
            } else {
                DelayedClose(0);
            }
        }



        private async void UploadImageToImgur(byte[] cimg) {
            string link = await UploadImgur(cimg);

            if(link.StartsWith("http://")) {
                Clipboard.SetText(link);

                SuccessToast.Show("Link to Imgur copied to Clipboard!",
                    TimeSpan.FromSeconds(3.5));
            } else {
                ErrorToast.Show(string.Format("Error uploading Image to Imgur! ({0})", link),
                    TimeSpan.FromSeconds(3.5));
            }
        }


        private void CopyImageToClipboard(byte[] cimg) {
            CopyClipboard(cimg);
            SuccessToast.Show("Image was copied to Clipboard!",
                TimeSpan.FromSeconds(3.5));
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
