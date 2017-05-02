using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.Hotkeys;
using ImgurSniper.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace ImgurSniper.Libraries.Start {
    public static class StartTray {
        private static HotKey _imgHotKey, _gifHotKey;
        private static NotifyIcon _nicon;
        private static Image _iconGif, _iconHelp, _iconSettings, _iconExit;
        private static bool _isDisposed;

        public static async Task Initialize(EntryWindow caller) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            IntPtr handle = new WindowInteropHelper(caller).Handle;

            Key imgKey = ConfigHelper.ShortcutImgKey;
            Key gifKey = ConfigHelper.ShortcutGifKey;
            bool usePrint = ConfigHelper.UsePrint;

            try {
                _imgHotKey = usePrint
                    ? new HotKey(handle, ModifierKeys.None, Key.PrintScreen)
                    : new HotKey(handle, ModifierKeys.Control | ModifierKeys.Shift, imgKey);
                _imgHotKey.HotKeyPressed += OpenFromShortcutImg;
            } catch {
                //ignored
            }
            try {
                _gifHotKey = new HotKey(handle, ModifierKeys.Control | ModifierKeys.Shift, gifKey);
                _gifHotKey.HotKeyPressed += OpenFromShortcutGif;
            } catch {
                //ignored
            }

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            //Icons
            try {
                _iconGif = ImageHelper.LoadImage(Path.Combine(ConfigHelper.InstallDir, "Resources", "iconGif.png"));
                _iconHelp = ImageHelper.LoadImage(Path.Combine(ConfigHelper.InstallDir, "Resources", "iconHelp.png"));
                _iconSettings = ImageHelper.LoadImage(Path.Combine(ConfigHelper.InstallDir, "Resources", "iconSettings.png"));
                _iconExit = ImageHelper.LoadImage(Path.Combine(ConfigHelper.InstallDir, "Resources", "iconExit.png"));
            } catch {
                //Images not found
            }

            //Item: GIF
            ToolStripItem gifMenuItem = contextMenu.Items.Add(strings.gif);
            gifMenuItem.Image = _iconGif;
            gifMenuItem.Click += delegate { OpenFromShortcutGif(); };
            gifMenuItem.Font = new Font(gifMenuItem.Font, gifMenuItem.Font.Style | FontStyle.Bold);

            //Item: -
            contextMenu.Items.Add(new ToolStripSeparator());

            //Item: Help
            ToolStripItem helpMenuItem = contextMenu.Items.Add(strings.help);
            helpMenuItem.Image = _iconHelp;
            helpMenuItem.Click += delegate {
                try {
                    Process.Start(Path.Combine(ConfigHelper.InstallDir, "ImgurSniper.UI.exe"), "help");
                } catch {
                    // ignored
                }
            };

            //Item: Settings
            ToolStripItem settingsMenuItem = contextMenu.Items.Add(strings.settings);
            settingsMenuItem.Image = _iconSettings;
            settingsMenuItem.Click += delegate {
                try {
                    Process.Start(Path.Combine(ConfigHelper.InstallDir, "ImgurSniper.UI.exe"));
                } catch {
                    // ignored
                }
            };

            //Item: Exit
            ToolStripItem exitMenuItem = contextMenu.Items.Add(strings.exit);
            exitMenuItem.Image = _iconExit;
            exitMenuItem.Click += delegate {
                DisposeJunk();
                tcs.SetResult(true);
            };

            //NotifyIcon
            _nicon = new NotifyIcon {
                Icon = Resources.Logo,
                ContextMenuStrip = contextMenu,
                Visible = true,
                Text = strings.clickorpress +
                       (usePrint ? strings.printKeyShortcut : string.Format(strings.ctrlShiftShortcut, imgKey)) +
                       strings.toSnipeNew
            };
            _nicon.MouseClick += (sender, e) => {
                if (e.Button == MouseButtons.Left) {
                    OpenFromShortcutImg();
                }
            };

            System.Windows.Application.Current.Exit += (sender, e) => {
                DisposeJunk();
            };

            await tcs.Task;
        }

        private static void OpenFromShortcutGif(HotKey obj = null) {
            using (GifWindow window = new GifWindow()) {
                window.ShowDialog();
            }
        }

        private static void OpenFromShortcutImg(HotKey obj = null) {
            using (ScreenshotWindow window = new ScreenshotWindow()) {
                window.ShowDialog();
            }
        }

        private static void DisposeJunk() {
            if (_isDisposed)
                return;

            using (_nicon) {
                using (_nicon.Icon) { }
                _nicon.Icon = null;
                _nicon.Visible = false;
            }
            _nicon = null;

            using (_iconGif) { }
            using (_iconExit) { }
            using (_iconHelp) { }
            using (_iconSettings) { }

            _isDisposed = true;
        }
    }
}
