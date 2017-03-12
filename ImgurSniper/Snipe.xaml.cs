using ImgurSniper.Properties;
using mrousavy;
using System;
using System.Collections.Generic;
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
        private static Notification _notification {
            get {
                return _internalNotification;
            }
            set {
                if(_internalNotification != null)
                    _internalNotification.Close();

                _internalNotification = value;
            }
        }
        private static Notification _internalNotification;


        public Snipe() {
            InitializeComponent();

            Initialize();

            Position();

            Start();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            //Prevent short flash of Toasts
            await Task.Delay(100);
            //TODO: DEBUG
            //ErrorToast.Visibility = Visibility.Visible;
            //SuccessToast.Visibility = Visibility.Visible;


            //Hide in Alt + Tab Switcher View
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)WinAPI.GetWindowLong(wndHelper.Handle, (int)WinAPI.GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)WinAPI.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinAPI.SetWindowLong(wndHelper.Handle, (int)WinAPI.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);


            //Notification n = new Notification("Hallo", true, false);
            //n.Show();
            //await Task.Delay(1000);
            //n.contentLabel.Content = "Test";
            //n.Close();
        }

        private void Start() {
            string[] args = Environment.GetCommandLineArgs();
            bool upload = false, autostart = false;
            List<string> uploadFiles = new List<string>();

            foreach(string arg in args) {
                if(arg.ToLower().Contains("autostart")) {
                    autostart = true;
                    break;
                }

                if(arg.ToLower().Contains("upload")) {
                    upload = true;
                }

                //To definetly be sure, arg is a File
                if(File.Exists(arg) && (arg.Contains("/") || arg.Contains("\\"))) {
                    //In debug mode, the vshost exe is passed as argument
                    if(!arg.ToLower().EndsWith("exe"))
                        uploadFiles.Add(arg);
                }
            }

            UpdateCheck();

            if(autostart) {
                InitializeTray();
            } else if(upload && uploadFiles.Count != 0) {
                InstantUpload(uploadFiles);
            } else {
                Crop(true);
            }
        }

        private void UpdateCheck() {
#if !DEBUG
            try {
                Process p = new Process();

                p.StartInfo = new ProcessStartInfo {
                    FileName = Path.Combine(FileIO._programFiles, "ImgurSniper.UI.exe"),
                    Arguments = "Update"
                };

                p.Start();
            } catch { }
#endif
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
            menu.MenuItems.Add(strings.help, delegate {
                try {
                    Process p = new Process();

                    p.StartInfo = new ProcessStartInfo {
                        FileName = Path.Combine(FileIO._programFiles, "ImgurSniper.UI.exe"),
                        Arguments = "Help"
                    };

                    p.Start();
                } catch {
                    // ignored
                }
            });
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

        private async void InstantUpload(List<string> files) {
            await Task.Delay(550);

            if(files.Count > 1) {
                //////UPLOAD MULTIPLE IMAGES
                try {
                    //Binary Image
                    List<byte[]> images = new List<byte[]>();
                    //Image IDs
                    List<string> ids = new List<string>();

                    double size = 0;
                    foreach(string file in files) {
                        byte[] image = File.ReadAllBytes(file);
                        images.Add(image);
                        size += image.Length;
                    }

                    //Image Size
                    string kb = $"{size / 1024d:0.#}";

                    //Key = Album ID | Value = Album Delete Hash (Key = Value if User is logged in)
                    KeyValuePair<string, string> albumInfo = await _imgur.CreateAlbum();


                    _notification = new Notification("", Notification.NotificationType.Progress, false);
                    _notification.Show();

                    int index = 1;
                    //Upload each image
                    foreach(byte[] file in images) {
                        try {
                            //e.g. "Uploading Images (123KB) (1 of 2)"
                            //SuccessToast.Show(string.Format(strings.uploadingFiles, kb, index, files.Count), TimeSpan.FromDays(10));
                            _notification.contentLabel.Text = string.Format(strings.uploadingFiles, kb, index, files.Count);

                            string id = await _imgur.UploadId(file, albumInfo.Value);
                            ids.Add(id);
                        } catch(Exception e) {
                            Debug.Write(e.Message);
                            //this image was not uploaded
                        }
                        index++;
                    }

                    await OpenAlbum(albumInfo.Key);
                } catch {
                    //Unsupported File Type? Internet connection error?
                    _notification = new Notification(strings.errorInstantUpload, Notification.NotificationType.Error, true);
                    await _notification.ShowAsync();
                    //await ErrorToast.ShowAsync(strings.errorInstantUpload, TimeSpan.FromSeconds(5));
                }
            } else {
                //////UPLOAD SINGLE IMAGE
                try {
                    //Binary Image
                    byte[] byteImg = File.ReadAllBytes(files[0]);

                    //Image Size
                    string kb = $"{byteImg.Length / 1024d:0.#}";

                    //e.g. "Uploading Image (123KB)"
                    _notification = new Notification(string.Format(strings.uploading, kb), Notification.NotificationType.Progress, true);
                    _notification.Show();
                    //SuccessToast.Show(string.Format(strings.uploading, kb), TimeSpan.FromDays(10));

                    await UploadImageToImgur(byteImg, "");
                } catch {
                    //Unsupported File Type? Internet connection error?
                    _notification = new Notification(strings.errorInstantUpload, Notification.NotificationType.Error, true);
                    await _notification.ShowAsync();
                    //await ErrorToast.ShowAsync(strings.errorInstantUpload, TimeSpan.FromSeconds(5));
                }
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
                    _notification = new Notification(strings.imgToBig, Notification.NotificationType.Error, true);
                    await _notification.ShowAsync();
                    //await ErrorToast.ShowAsync(strings.imgToBig, TimeSpan.FromSeconds(3));
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
                        _notification = new Notification(string.Format(strings.uploading, kb), Notification.NotificationType.Progress, true);
                        _notification.Show();
                        //SuccessToast.Show(string.Format(strings.uploading, kb), TimeSpan.FromDays(10));

                        await UploadImageToImgur(cimg, window.HwndName);
                    } else {
                        CopyClipboard(cimg);

                        _notification = new Notification(strings.imgclipboard, Notification.NotificationType.Success, true);
                        await _notification.ShowAsync();
                        //await SuccessToast.ShowAsync(strings.imgclipboard, TimeSpan.FromSeconds(3));
                    }
                } catch(Exception ex) {
                    _notification = new Notification(string.Format(strings.otherErrorMsg, ex), Notification.NotificationType.Error, true);
                    await _notification.ShowAsync();
                    //ErrorToast.Show(string.Format(strings.otherErrorMsg, ex),
                    //    TimeSpan.FromSeconds(3.5));
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

                _notification = new Notification(strings.linkclipboard, Notification.NotificationType.Success, true);
                await _notification.ShowAsync();
                //await SuccessToast.ShowAsync(strings.linkclipboard,
                //    TimeSpan.FromSeconds(3));
            } else {
                _notification = new Notification(string.Format(strings.uploadingError, link), Notification.NotificationType.Error, true);
                await _notification.ShowAsync();
                //await ErrorToast.ShowAsync(string.Format(strings.uploadingError, link),
                //    TimeSpan.FromSeconds(5));
            }
        }


        //Open an Album with the ID
        private async Task OpenAlbum(string albumId) {
            //Default Imgur Album URL
            string link = "http://imgur.com/a/" + albumId;

            Clipboard.SetText(link);
            PlayBlop();

            if(FileIO.OpenAfterUpload) {
                Process.Start(link);
            }

            //await SuccessToast.ShowAsync(strings.linkclipboard,
            //    TimeSpan.FromSeconds(3));
            _notification = new Notification(strings.linkclipboard, Notification.NotificationType.Success, true);
            await _notification.ShowAsync();
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
