using Hotkeys;
using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.Native;
using ImgurSniper.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for Snipe.xaml
    /// </summary>
    public partial class Snipe {
        private static readonly Action ActionTroubleshoot =
            delegate { Process.Start(Path.Combine(ConfigHelper.ProgramFiles, "ImgurSniper.UI.exe"), "Troubleshooting"); };
        public static readonly Action DisposeNotification =
            delegate {
                Notification = null;
                GC.Collect();
            };

        private static Notification _internalNotification;
        private string _dir;
        private bool _gif;
        private NotifyIcon _nicon;
        private ImgurUploader _imgur;

        private static Notification Notification {
            get => _internalNotification;
            set {
                if (value == null) {
                    _internalNotification = null;
                } else {
                    _internalNotification?.Close();
                    _internalNotification = value;
                }
            }
        }

        public Snipe() {
            InitializeComponent();

#if DEBUG
            Notification = new Notification("ImgurSniper initialized!", Notification.NotificationType.Success, true,
                null);
            Notification.Show();
#endif

            Initialize();
        }

        //Initialize important Variables
        private void Initialize() {
            _dir = ConfigHelper.SaveImagesPath;
            _imgur = new ImgurUploader();
        }

        //Start Everything and load CMD Args
        private void Start() {
            bool upload = false, autostart = false;
            List<string> uploadFiles = null;
            List<string> args = new List<string>(Environment.GetCommandLineArgs());

            if (args.Count > 0) {
                uploadFiles = new List<string>();


                // "-autostart" or "/autostart" -> "autostart"
                Regex regexParam = new Regex("^(-/)");
                args = new List<string>(args.Select(arg => regexParam.Replace(arg, "")));

                foreach (string arg in args) {
                    //Tray mode
                    if (arg.ToLower() == "autostart") {
                        autostart = true;
                        break;
                    }

                    //GIF Mode
                    if (arg.ToLower() == "gif") {
                        _gif = true;
                    }

                    //Instant Upload Mode
                    if (arg.ToLower() == "upload") {
                        upload = true;
                    }

                    //To definetly be sure, arg is a File
                    if (File.Exists(arg) && (arg.Contains("/") || arg.Contains("\\"))) {
                        //Only allow Image files
                        if (ImageHelper.IsImage(arg.ToLower())) {
                            uploadFiles.Add(arg);
                        }
                    }
                }
            }

            UpdateCheck();

            if (uploadFiles == null || uploadFiles.Count < 1)
                uploadFiles = null;
            GC.Collect();

            if (autostart) {
                InitializeTray();
            } else if (upload && uploadFiles.Count != 0) {
                InstantUpload(uploadFiles);
            } else {
                Crop(true, _gif);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //Hide in Alt + Tab Switcher View
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)NativeMethods.GetWindowLong(wndHelper.Handle, (int)NativeMethods.GetWindowLongFields.GwlExstyle);

            exStyle |= (int)NativeMethods.ExtendedWindowStyles.WsExToolwindow;
            NativeMethods.SetWindowLong(wndHelper.Handle, (int)NativeMethods.GetWindowLongFields.GwlExstyle, (IntPtr)exStyle);

            //Start ImgurSniper
            Start();
        }

        private void UpdateCheck() {
#if !DEBUG
            try {
                //If automatically update, and last checked was more than 2 days ago
                if (ConfigHelper.AutoUpdate && (DateTime.Now - ConfigHelper.LastChecked) > TimeSpan.FromDays(2))
                    Process.Start(Path.Combine(ConfigHelper.ProgramFiles, "ImgurSniper.UI.exe"), "Update");
            } catch { }
#endif
        }

        private async void InitializeTray() {
            //Wait for Dispatcher to utilize (Apparently it won't work without a Delay)
            await Task.Delay(1000);

            Key imgKey = ConfigHelper.ShortcutImgKey;
            Key gifKey = ConfigHelper.ShortcutGifKey;
            bool usePrint = ConfigHelper.UsePrint;

            HotKey imgHotKey = null;
            try {
                imgHotKey = usePrint
                    ? new HotKey(ModifierKeys.None, Key.PrintScreen, this)
                    : new HotKey(ModifierKeys.Control | ModifierKeys.Shift, imgKey, this);
                imgHotKey.HotKeyPressed += OpenFromShortcutImg;
            } catch {
                //ignored
            }
            try {
                HotKey gifHotKey = new HotKey(ModifierKeys.Control | ModifierKeys.Shift, gifKey, this);
                gifHotKey.HotKeyPressed += OpenFromShortcutGif;
            } catch {
                //ignored
            }

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            //Icons
            Image iconGif = null, iconHelp = null, iconSettings = null, iconExit = null;

            try {
                iconGif = ImageHelper.LoadImage(Path.Combine(ConfigHelper.ProgramFiles, "Resources", "iconGif.png"));
                iconHelp = ImageHelper.LoadImage(Path.Combine(ConfigHelper.ProgramFiles, "Resources", "iconHelp.png"));
                iconSettings = ImageHelper.LoadImage(Path.Combine(ConfigHelper.ProgramFiles, "Resources", "iconSettings.png"));
                iconExit = ImageHelper.LoadImage(Path.Combine(ConfigHelper.ProgramFiles, "Resources", "iconExit.png"));
            } catch {
                //Images not found
            }

            //Item: GIF
            ToolStripItem gifMenuItem = contextMenu.Items.Add(strings.gif);
            gifMenuItem.Image = iconGif;
            gifMenuItem.Click += delegate { Crop(false, true); };
            gifMenuItem.Font = new Font(gifMenuItem.Font, gifMenuItem.Font.Style | System.Drawing.FontStyle.Bold);

            //Item: -
            contextMenu.Items.Add(new ToolStripSeparator());

            //Item: Help
            ToolStripItem helpMenuItem = contextMenu.Items.Add(strings.help);
            helpMenuItem.Image = iconHelp;
            helpMenuItem.Click += delegate {
                try {
                    Process.Start(Path.Combine(ConfigHelper.ProgramFiles, "ImgurSniper.UI.exe"), "Help");
                } catch {
                    // ignored
                }
            };

            //Item: Settings
            ToolStripItem settingsMenuItem = contextMenu.Items.Add(strings.settings);
            settingsMenuItem.Image = iconSettings;
            settingsMenuItem.Click += delegate {
                try {
                    Process.Start(Path.Combine(ConfigHelper.ProgramFiles, "ImgurSniper.UI.exe"));
                } catch {
                    // ignored
                }
            };

            //Item: Exit
            ToolStripItem exitMenuItem = contextMenu.Items.Add(strings.exit);
            exitMenuItem.Image = iconExit;
            exitMenuItem.Click += delegate { Application.Current.Shutdown(); };

            //NotifyIcon
            _nicon = new NotifyIcon {
                Icon = Properties.Resources.Logo,
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = strings.clickorpress +
                       (usePrint ? strings.printKeyShortcut : string.Format(strings.ctrlShiftShortcut, imgKey)) +
                       strings.toSnipeNew
            };
            _nicon.MouseClick += (sender, e) => {
                if (e.Button == MouseButtons.Left) {
                    if (imgHotKey == null) {
                        Crop(false, false);
                    } else {
                        OpenFromShortcutImg(imgHotKey);
                    }
                }
            };

            Application.Current.Exit += delegate {
                _nicon.Icon = null;
                _nicon.Visible = false;
                _nicon.Dispose();
                _nicon = null;

                iconGif?.Dispose();
                iconHelp?.Dispose();
                iconSettings?.Dispose();
                iconExit?.Dispose();
            };
        }

        //Open Snipe by Shortcut (Ctrl + Shift + X or Print)
        private void OpenFromShortcutImg(HotKey h) {
            h.HotKeyPressed -= OpenFromShortcutImg;
            Crop(false, false);
            h.HotKeyPressed += OpenFromShortcutImg;
        }

        //Open GIF by Shortcut (Ctrl + Shift + G or Print)
        private void OpenFromShortcutGif(HotKey h) {
            h.HotKeyPressed -= OpenFromShortcutGif;
            Crop(false, true);
            h.HotKeyPressed += OpenFromShortcutGif;
        }

        private async void InstantUpload(List<string> files) {
            await Task.Delay(500);

            if (files.Count > 1) {
                //////UPLOAD MULTIPLE IMAGES
                try {
                    //Binary Image
                    List<byte[]> images = new List<byte[]>();
                    //Image IDs

                    double size = 0;
                    foreach (string file in files) {
                        byte[] image = File.ReadAllBytes(file);
                        images.Add(image);
                        size += image.Length;
                    }

                    //Image Size
                    string kb = $"{size / 1024d:0.#}";

                    //Key = Album ID | Value = Album Delete Hash (Key = Value if User is logged in)
                    KeyValuePair<string, string> albumInfo = await _imgur.CreateAlbum();


                    Notification = new Notification("", Notification.NotificationType.Progress, false, null);
                    Notification.Show();

                    int index = 1;
                    //Upload each image
                    for (int i = 0; i < images.Count; i++) {
                        try {
                            Notification.contentLabel.Text = string.Format(strings.uploadingFiles, kb, index,
                                files.Count);
                        } catch (Exception e) {
                            Debug.Write(e.Message);
                            //this image was not uploaded
                        }
                        index++;
                    }

                    await OpenAlbum(albumInfo.Key);

                    Notification?.Close();
                } catch {
                    //Unsupported File Type? Internet connection error?
                    Notification = new Notification(strings.errorInstantUpload, Notification.NotificationType.Error,
                        true, ActionTroubleshoot);
                    await Notification.ShowAsync();
                }
            } else {
                //////UPLOAD SINGLE IMAGE
                try {
                    //Binary Image
                    byte[] byteImg = File.ReadAllBytes(files[0]);

                    //Image Size
                    string kb = $"{byteImg.Length / 1024d:0.#}";

                    //e.g. "Uploading Image (123KB)"
                    Notification = new Notification(string.Format(strings.uploading, kb),
                        Notification.NotificationType.Progress, false, null);
                    Notification.Show();

                    string link = await _imgur.Upload(byteImg, "");
                    await HandleLink(link);

                    Notification?.Close();
                } catch {
                    //Unsupported File Type? Internet connection error?
                    Notification = new Notification(strings.errorInstantUpload, Notification.NotificationType.Error,
                        true, ActionTroubleshoot);
                    await Notification.ShowAsync();
                }
            }

            DelayedClose(0);
        }

        //Make Screenshot, Let user Crop, Upload Picture and Copy Link to Clipboard
        private async void Crop(bool closeOnFinish, bool gif) {
            //Visibility = Visibility.Visible;
            //BringIntoView();
            //Topmost = true;

            if (gif) {
                await CaptureGif();
            } else {
                await CaptureImage();
            }

            if (closeOnFinish) {
                DelayedClose(0);
            }

            Notification = null;
            GC.Collect();
        }

        //Open Image Capture Window
        private async Task CaptureImage() {
            using (ScreenshotWindow window = new ScreenshotWindow(ConfigHelper.AllMonitors)) {
                window.ShowDialog();

                if (window.DialogResult == true) {
                    try {
                        //Config: Save Image locally?
                        if (ConfigHelper.SaveImages) {
                            try {
                                //Save File with unique name
                                long time = DateTime.Now.ToFileTimeUtc();
                                string extension = "." + ConfigHelper.ImageFormat.ToString().ToLower();
                                string filename = _dir + $"\\Snipe_{time}{extension}";
                                File.WriteAllBytes(filename, window.CroppedImage);

                                if (ConfigHelper.OpenAfterUpload) {
                                    //If name contains Spaces, Arguments get seperated by the Space
                                    if (filename.Contains(" ")) {
                                        //Open Image itself
                                        Process.Start(filename);
                                    } else {
                                        //Open Explorer and Highlight Image
                                        Process.Start("explorer.exe", $"/select,\"{filename}\"");
                                    }
                                }
                            } catch {
                                // ignored
                            }
                        }

                        bool imgurAfterSnipe = ConfigHelper.ImgurAfterSnipe;

                        //Config: Upload Image to Imgur or Copy to Clipboard?
                        if (imgurAfterSnipe) {
                            //Log user in (Imgur)
                            ImgurUploader imgur = new ImgurUploader();
                            await imgur.Login();

#if DEBUG
                            bool tooBig = false;
#else
                            //10 MB = 10.485.760 Bytes      => Imgur's max. File Size
                            bool tooBig = window.CroppedImage.Length >= 10485760;
#endif
                            if (tooBig) {
                                //Could not upload to imgur
                                Notification = new Notification(strings.imgTooBig, Notification.NotificationType.Error, true,
                                    ActionTroubleshoot);
                                await Notification.ShowAsync();
                                return;
                            }

                            //Progress Indicator
                            string kb = $"{window.CroppedImage.Length / 1024d:0.#}";
                            Notification = new Notification(string.Format(strings.uploading, kb),
                                Notification.NotificationType.Progress, false, null);
                            Notification.Show();

                            //Upload Binary
                            string link = await imgur.Upload(window.CroppedImage, window.HwndName);
                            await HandleLink(link);

                            Notification?.Close();
                        } else {
                            //Copy Binary Image to Clipboard
                            CopyClipboard(window.CroppedImage);

                            Notification = new Notification(strings.imgclipboard, Notification.NotificationType.Success,
                                true, null);
                            await Notification.ShowAsync();
                        }
                    } catch (Exception ex) {
                        Notification = new Notification(strings.errorMsg, Notification.NotificationType.Error, true,
                            ActionTroubleshoot);
                        await Notification.ShowAsync();

                        MessageBox.Show(string.Format(strings.otherErrorMsg, ex.Message),
                            strings.errorMsg);
                    }
                } else {
                    if (window.Error) {
                        Notification = new Notification(strings.uploadingError, Notification.NotificationType.Error,
                            true,
                            ActionTroubleshoot);
                        await Notification.ShowAsync();
                    }
                }
            }

            Notification = null;
            GC.Collect();
        }

        //Open GIF Capture Window
        private async Task CaptureGif() {
            using (GifWindow window = new GifWindow(ConfigHelper.AllMonitors)) {
                window.ShowDialog();

                if (window.DialogResult == true) {
                    try {
                        bool imgurAfterSnipe = ConfigHelper.ImgurAfterSnipe;

                        //Config: Save Image locally?
                        if (ConfigHelper.SaveImages) {
                            try {
                                //Save File with unique name
                                long time = DateTime.Now.ToFileTimeUtc();
                                string filename = _dir + $"\\Snipe_{time}.gif";
                                File.WriteAllBytes(filename, window.CroppedGif);

                                if (ConfigHelper.OpenAfterUpload) {
                                    //If name contains Spaces, Arguments get seperated by the Space
                                    if (filename.Contains(" ")) {
                                        //Open Image itself
                                        Process.Start(filename);
                                    } else {
                                        //Open Explorer and Highlight Image
                                        Process.Start("explorer.exe", $"/select,\"{filename}\"");
                                    }
                                }
                            } catch {
                                // ignored
                            }
                        }

                        //Config: Upload Image to Imgur or Copy to Clipboard?
                        if (imgurAfterSnipe) {
                            //Log user in (Imgur)
                            ImgurUploader imgur = new ImgurUploader();
                            await imgur.Login();

#if DEBUG
                            bool tooBig = false;
#else
                            //10 MB = 10.485.760 Bytes      => Imgur's max. File Size
                            bool tooBig = window.CroppedGif.Length >= 10485760;
#endif
                            if (tooBig) {
                                Notification = new Notification(strings.imgTooBigGif, Notification.NotificationType.Error, true,
                                    ActionTroubleshoot);
                                await Notification.ShowAsync();
                                return;
                            }

                            string kb = $"{window.CroppedGif.Length / 1024d:0.#}";
                            Notification = new Notification(string.Format(strings.uploadingGif, kb),
                                Notification.NotificationType.Progress, false, null);
                            Notification.Show();

                            string link = await _imgur.Upload(window.CroppedGif, window.HwndName);
                            await HandleLink(link);

                            Notification?.Close();
                        } else {
                            CopyClipboard(window.CroppedGif);

                            Notification = new Notification(strings.imgclipboardGif,
                                Notification.NotificationType.Success, true, null);
                            await Notification.ShowAsync();
                        }
                    } catch (Exception ex) {
                        Notification = new Notification(strings.errorMsg, Notification.NotificationType.Error, true,
                            ActionTroubleshoot);
                        await Notification.ShowAsync();

                        MessageBox.Show(string.Format(strings.otherErrorMsg, ex.Message),
                            strings.errorMsg);
                    }
                } else {
                    if (window.Error) {
                        Notification = new Notification(strings.uploadingErrorGif, Notification.NotificationType.Error,
                            true,
                            ActionTroubleshoot);
                        await Notification.ShowAsync();
                    }
                }
            }

            Notification = null;
            GC.Collect();
        }

        //Upload byte[] to imgur and give user a response
        private async Task HandleLink(string link) {
            //Link is http or https
            if (link.StartsWith("http")) {
                Clipboard.SetText(link);
                PlayBlop();

                Action action = delegate { Process.Start(link); };

                if (ConfigHelper.OpenAfterUpload) {
                    Process.Start(link);
                    action = null;
                }

                Notification = new Notification(strings.linkclipboard, Notification.NotificationType.Success, true,
                    action);
                await Notification.ShowAsync();
            } else {
                Notification = new Notification(string.Format(strings.uploadingError, link),
                    Notification.NotificationType.Error, true, ActionTroubleshoot);
                await Notification.ShowAsync();
            }
        }

        //Open an Album with the ID
        private async Task OpenAlbum(string albumId) {
            //Default Imgur Album URL
            string link = "http://imgur.com/a/" + albumId;

            Clipboard.SetText(link);
            PlayBlop();

            Action action = delegate { Process.Start(link); };

            if (ConfigHelper.OpenAfterUpload) {
                Process.Start(link);
                action = null;
            }

            Notification = new Notification(strings.linkclipboard, Notification.NotificationType.Success, true, action);
            await Notification.ShowAsync();
        }

        //Parse byte[] to Image and write to Clipboard
        private static void CopyClipboard(byte[] cimg) {
            //Parse byte[] to Images
            BitmapImage image = new BitmapImage();
            using (MemoryStream mem = new MemoryStream(cimg)) {
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
        private async void DelayedClose(int delay) {
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            //this.Close();
            Application.Current.Shutdown(0);
        }

        //Play the Blop Success Sound
        private static void PlayBlop() {
            try {
                MediaPlayer player = new MediaPlayer { Volume = 30 };
                player.MediaEnded += delegate {
                    player.Close();
                };

                string path = Path.Combine(ConfigHelper.ProgramFiles, "Resources\\Blop.wav");

                player.Open(new Uri(path));
                player.Play();
            } catch {
                // ignored
            }
        }
    }
}
