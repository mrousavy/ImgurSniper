using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Clipboard = System.Windows.Clipboard;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for Snipe.xaml
    /// </summary>
    public partial class Snipe : Window {
        private string _dir;
        private ImgurIO _imgur;
        private NotifyIcon _nicon;


        public Snipe() {
            InitializeComponent();

            Initialize();

            Position();

            this.Loaded += async delegate {
                //Prevent short flash of Toasts
                await Task.Delay(100);
                ErrorToast.Visibility = Visibility.Visible;
                SuccessToast.Visibility = Visibility.Visible;
            };

            Start();
        }

        private void Start() {
            string[] args = Environment.GetCommandLineArgs();
            bool upload = false, autostart = false;
            string image = null;

            foreach(string arg in args) {
                if(arg.ToLower().Contains("autostart")) {
                    autostart = true;
                    break;
                }

                if(arg.ToLower().Contains("upload"))
                    upload = true;

                if(File.Exists(arg))
                    image = arg;
            }

            if(autostart) {
                InitializeTray();
            } else if(upload && image != null) {
                InstantUpload(image);
            } else {
                Crop(true, false);
            }
        }

        private void InitializeTray() {
            this.Visibility = Visibility.Hidden;

            Key sKey = FileIO.ShortcutKey;
            bool usePrint = FileIO.UsePrint;

            HotKey hk = null;
            try {
                hk = usePrint
                     ? new HotKey(ModifierKeys.None, Key.PrintScreen, this)
                     : new HotKey(ModifierKeys.Control | ModifierKeys.Shift, sKey, this);
                hk.HotKeyPressed += OpenFromShortcut;
            } catch {
                //ignored
            }

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(Properties.strings.settings, delegate {
                try {
                    Process.Start(Path.Combine(FileIO._programFiles, "ImgurSniper.UI.exe"));
                } catch {
                    // ignored
                }
            });
            menu.MenuItems.Add(Properties.strings.exit, delegate {
                System.Windows.Application.Current.Shutdown();
            });

            _nicon = new NotifyIcon {
                Icon = Properties.Resources.Logo,
                ContextMenu = menu,
                Visible = true,
                Text = Properties.strings.clickorpress + (usePrint ? Properties.strings.printKeyShortcut : string.Format(Properties.strings.ctrlShiftShortcut, sKey)) + Properties.strings.toSnipeNew
            };
            _nicon.MouseClick += (sender, e) => {
                if(e.Button == MouseButtons.Left)
                    if(hk == null)
                        Crop(false, true);
                    else
                        OpenFromShortcut(hk);
            };

            System.Windows.Application.Current.Exit += delegate {
                _nicon.Icon = null;
                _nicon.Visible = false;
                _nicon.Dispose();
                _nicon = null;
            };
        }

        //Open Snipe by Shortcut (Ctrl + Shift + I or Print)
        private void OpenFromShortcut(HotKey h) {
            h.HotKeyPressed -= OpenFromShortcut;
            Crop(false, false);
            h.HotKeyPressed += OpenFromShortcut;
        }

        private async void InstantUpload(string path) {
            await Task.Delay(550);

            string lpath = path.ToLower();
            if(lpath.EndsWith(".jpeg") ||
                lpath.EndsWith(".jpg") ||
                lpath.EndsWith(".png") ||
                lpath.EndsWith(".gif") ||
                lpath.EndsWith(".apng") ||
                lpath.EndsWith(".tiff") ||
                lpath.EndsWith(".xcf") ||
                lpath.EndsWith(".pdf")) {

                byte[] byteImg = File.ReadAllBytes(path);

                string KB = $"{(byteImg.Length / 1024d):0.#}";
                SuccessToast.Show(string.Format(Properties.strings.uploading, KB), TimeSpan.FromDays(10));

                await UploadImageToImgur(byteImg, "");
            } else {
                await ErrorToast.ShowAsync(Properties.strings.errorFileType, TimeSpan.FromSeconds(5));
            }
            DelayedClose(0);
        }

        //Initialize important Variables
        private void Initialize() {
            _dir = FileIO.SaveImagesPath;
            if(string.IsNullOrWhiteSpace(_dir)) {
                _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");
            }

            if(!Directory.Exists(_dir)) {
                Directory.CreateDirectory(_dir);
            }

            _imgur = new ImgurIO();
        }

        //Position Window correctly
        private void Position() {
            this.Top = SystemParameters.WorkArea.Height - this.Height;
            this.Width = SystemParameters.WorkArea.Width;
            this.Left = Screen.PrimaryScreen.Bounds.X;
        }

        //Make Screenshot, Let user Crop, Upload Picture and Copy Link to Clipboard
        private async void Crop(bool CloseOnFinish, bool FocusNewWindow) {
            this.Visibility = Visibility.Visible;
            this.BringIntoView();
            this.Topmost = true;

            ScreenshotWindow window = new ScreenshotWindow(FileIO.AllMonitors, FocusNewWindow);
            window.ShowDialog();

            if(window.DialogResult == true) {

                byte[] cimg = window.CroppedImage;

                if(cimg.Length >= 10240000 && !FileIO.TokenExists) {
                    await ErrorToast.ShowAsync(Properties.strings.imgToBig, TimeSpan.FromSeconds(3));
                    return;
                }

                try {
                    //Config: Save Image locally?
                    if(FileIO.SaveImages) {
                        long time = DateTime.Now.ToFileTimeUtc();
                        string extension = FileIO.UsePNG ? ".png" : ".jpeg";
                        File.WriteAllBytes(_dir + $"\\Snipe_{time}{extension}", cimg);
                    }

                    //Config: Upload Image to Imgur or Copy to Clipboard?
                    if(FileIO.ImgurAfterSnipe) {
                        string KB = $"{(cimg.Length / 1024d):0.#}";
                        SuccessToast.Show(string.Format(Properties.strings.uploading, KB), TimeSpan.FromDays(10));

                        await UploadImageToImgur(cimg, window.HwndName);
                    } else {
                        CopyImageToClipboard(cimg);

                        SuccessToast.Show(Properties.strings.imgclipboard, TimeSpan.FromDays(10));
                    }

                } catch(Exception ex) {
                    ErrorToast.Show(string.Format(Properties.strings.otherErrorMsg, ex),
                        TimeSpan.FromSeconds(3.5));
                }
            }

            try {
                if(CloseOnFinish)
                    DelayedClose(0);
                else
                    this.Visibility = Visibility.Hidden;
            } catch {
                System.Windows.Application.Current.Shutdown();
            }
        }

        //Upload byte[] to imgur and give user a response
        private async Task UploadImageToImgur(byte[] cimg, string WindowName) {
            string link = await UploadImgur(cimg, WindowName);

            if(link.StartsWith("http://")) {
                Clipboard.SetText(link);
                PlayBlop();

                //Catch internal toast exceptions & process start exception
                try {
                    if(FileIO.OpenAfterUpload)
                        Process.Start(link);

                    await SuccessToast.ShowAsync(Properties.strings.linkclipboard,
                        TimeSpan.FromSeconds(5));
                } catch { }
            } else {
                //Catch internal toast exceptions
                try {
                    await ErrorToast.ShowAsync(string.Format(Properties.strings.uploadingError, link),
                    TimeSpan.FromSeconds(5));
                } catch { }
            }
        }

        //Copy the Byte[] to the Clipboard
        private void CopyImageToClipboard(byte[] cimg) {
            CopyClipboard(cimg);
            SuccessToast.Show(Properties.strings.imgclipboard,
                TimeSpan.FromSeconds(3.5));
        }

        //Upload Image to Imgur and returns URL to Imgur
        private async Task<string> UploadImgur(byte[] cimg, string WindowName) {
            string response = await _imgur.Upload(cimg, WindowName);

            //Copy Link to Clipboard
            Clipboard.SetText(response);

            return response;
        }

        //Parse byte[] to Image and write to Clipboard
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

        //Close Window with short delay
        private async void DelayedClose(int Delay) {
            await Task.Delay(TimeSpan.FromMilliseconds(Delay));
            //this.Close();
            System.Windows.Application.Current.Shutdown(0);
        }

        //Play the Blop Success Sound
        private void PlayBlop() {
            try {
                MediaPlayer player = new MediaPlayer { Volume = 30 };

                string path = Path.Combine(FileIO._programFiles, "Resources\\Blop.wav");

                player.Open(new Uri(path));
                player.Play();
            } catch { }
        }
    }
}
