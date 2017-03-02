using ImgurSniper.Properties;
using mrousavy;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for Snipe.xaml
    /// </summary>
    public partial class Snipe {
        private int _counter;
        private string _dir;
        private ImgurIO _imgur;
        private NotifyIcon _nicon;


        public Snipe() {
            InitializeComponent();

            Initialize();

            Position();

            Start();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            //Prevent short flash of Toasts
            await Task.Delay(100);
            ErrorToast.Visibility = Visibility.Visible;
            SuccessToast.Visibility = Visibility.Visible;


            //Hide in Alt + Tab Switcher View
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)WinAPI.GetWindowLong(wndHelper.Handle, (int)WinAPI.GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)WinAPI.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinAPI.SetWindowLong(wndHelper.Handle, (int)WinAPI.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
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

                if(arg.ToLower().Contains("upload")) {
                    upload = true;
                }

                if(File.Exists(arg)) {
                    image = arg;
                }
            }

            if(autostart) {
                InitializeTray();
            } else if(upload && image != null) {
                InstantUpload(image);
            } else {
                Crop(true);
            }
        }

        private async void InitializeTray() {
            Visibility = Visibility.Hidden;

            //Wait for Dispatcher to utilize (Apparently it won't work without a Delay)
            await Task.Delay(1000);

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
            menu.MenuItems.Add(strings.settings, delegate {
                try {
                    Process.Start(Path.Combine(FileIO._programFiles, "ImgurSniper.UI.exe"));
                } catch {
                    // ignored
                }
            });
            menu.MenuItems.Add(strings.exit, delegate { Application.Current.Shutdown(); });

            _nicon = new NotifyIcon {
                Icon = Properties.Resources.Logo,
                ContextMenu = menu,
                Visible = true,
                Text = strings.clickorpress +
                       (usePrint ? strings.printKeyShortcut : string.Format(strings.ctrlShiftShortcut, sKey)) +
                       strings.toSnipeNew
            };
            _nicon.MouseClick += (sender, e) => {
                if(e.Button == MouseButtons.Left) {
                    if(hk == null) {
                        Crop(false);
                    } else {
                        OpenFromShortcut(hk);
                    }
                }
            };

            Application.Current.Exit += delegate {
                _nicon.Icon = null;
                _nicon.Visible = false;
                _nicon.Dispose();
                _nicon = null;
            };
        }

        //Open Snipe by Shortcut (Ctrl + Shift + I or Print)
        private void OpenFromShortcut(HotKey h) {
            h.HotKeyPressed -= OpenFromShortcut;
            Crop(false);
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

                string kb = $"{byteImg.Length / 1024d:0.#}";
                SuccessToast.Show(string.Format(strings.uploading, kb), TimeSpan.FromDays(10));

                await UploadImageToImgur(byteImg, "");
            } else {
                await ErrorToast.ShowAsync(strings.errorFileType, TimeSpan.FromSeconds(5));
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
            Top = SystemParameters.WorkArea.Height - Height;
            Width = SystemParameters.WorkArea.Width;
            Left = Screen.PrimaryScreen.Bounds.X;
        }

        //Make Screenshot, Let user Crop, Upload Picture and Copy Link to Clipboard
        private async void Crop(bool closeOnFinish) {
            Visibility = Visibility.Visible;
            BringIntoView();
            Topmost = true;
            _counter++;
            int local = _counter;


            ScreenshotWindow window = new ScreenshotWindow(FileIO.AllMonitors);
            window.ShowDialog();


            if(window.DialogResult == true) {
                byte[] cimg = window.CroppedImage;

                if(cimg.Length >= 10240000 && !FileIO.TokenExists) {
                    await ErrorToast.ShowAsync(strings.imgToBig, TimeSpan.FromSeconds(3));
                    return;
                }

                try {
                    bool imgurAfterSnipe = FileIO.ImgurAfterSnipe;

                    //Config: Save Image locally?
                    if(FileIO.SaveImages) {
                        try {
                            long time = DateTime.Now.ToFileTimeUtc();
                            string extension = FileIO.UsePNG ? ".png" : ".jpeg";
                            string filename = _dir + $"\\Snipe_{time}{extension}";
                            File.WriteAllBytes(filename, cimg);

                            if(imgurAfterSnipe) {
                                Process.Start(filename);
                            }
                        } catch {
                            // ignored
                        }
                    }

                    //Config: Upload Image to Imgur or Copy to Clipboard?
                    if(imgurAfterSnipe) {
                        string kb = $"{cimg.Length / 1024d:0.#}";
                        SuccessToast.Show(string.Format(strings.uploading, kb), TimeSpan.FromDays(10));

                        await UploadImageToImgur(cimg, window.HwndName);
                    } else {
                        CopyClipboard(cimg);

                        await SuccessToast.ShowAsync(strings.imgclipboard, TimeSpan.FromSeconds(3));
                    }
                } catch(Exception ex) {
                    ErrorToast.Show(string.Format(strings.otherErrorMsg, ex),
                        TimeSpan.FromSeconds(3.5));
                }
            }

            try {
                if(closeOnFinish) {
                    DelayedClose(0);
                } else {
                    if(local == _counter) {
                        Visibility = Visibility.Hidden;
                    }
                }
            } catch {
                Application.Current.Shutdown();
            }
        }

        //Upload byte[] to imgur and give user a response
        private async Task UploadImageToImgur(byte[] cimg, string windowName) {
            string link = await _imgur.Upload(cimg, windowName);

            //Link is http or https
            if(link.StartsWith("http")) {
                Clipboard.SetText(link);
                PlayBlop();

                if(FileIO.OpenAfterUpload) {
                    Process.Start(link);
                }

                await SuccessToast.ShowAsync(strings.linkclipboard,
                    TimeSpan.FromSeconds(3));
            } else {
                await ErrorToast.ShowAsync(string.Format(strings.uploadingError, link),
                    TimeSpan.FromSeconds(5));
            }
        }

        //Parse byte[] to Image and write to Clipboard
        private static void CopyClipboard(byte[] cimg) {
            //Parse byte[] to Images
            BitmapImage image = new BitmapImage();
            using(MemoryStream mem = new MemoryStream(cimg)) {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();

            //Copy whole Image to Clipboard
            Clipboard.SetImage(image);
        }

        //Close Window with short delay
        private static async void DelayedClose(int delay) {
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            //this.Close();
            Application.Current.Shutdown(0);
        }

        //Play the Blop Success Sound
        private static void PlayBlop() {
            try {
                MediaPlayer player = new MediaPlayer { Volume = 30 };

                string path = Path.Combine(FileIO._programFiles, "Resources\\Blop.wav");

                player.Open(new Uri(path));
                player.Play();
            } catch {
                // ignored
            }
        }
    }
}
