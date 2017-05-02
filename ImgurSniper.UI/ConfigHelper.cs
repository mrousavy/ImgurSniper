using Newtonsoft.Json;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Input;

namespace ImgurSniper.UI {
    public static class ConfigHelper {
        #region Properties
        //Value whether ImgurSniper should strech over all screens or not
        public static bool AllMonitors {
            get => JsonConfig.AllMonitors;
            set {
                Settings settings = JsonConfig;
                settings.AllMonitors = value;
                JsonConfig = settings;
            }
        }

        //The Image Format for normal Images
        public static ImageFormat ImageFormat {
            get => JsonConfig.ImageFormat;
            set {
                Settings settings = JsonConfig;
                settings.ImageFormat = value;
                JsonConfig = settings;
            }
        }

        //Value whether ImgurSniper should open the uploaded Image in Browser after upload
        public static bool OpenBrowserAfterUpload {
            get => JsonConfig.OpenBrowserAfterUpload;
            set {
                Settings settings = JsonConfig;
                settings.OpenBrowserAfterUpload = value;
                JsonConfig = settings;
            }
        }

        //Value whether ImgurSniper should open the uploaded Image in Browser after upload
        public static bool OpenFileAfterSnap {
            get => JsonConfig.OpenFileAfterSnap;
            set {
                Settings settings = JsonConfig;
                settings.OpenFileAfterSnap = value;
                JsonConfig = settings;
            }
        }

        //Key for ImgurSniper Image Shortcut
        public static Key ShortcutImgKey {
            get => JsonConfig.ShortcutImgKey;
            set {
                Settings settings = JsonConfig;
                settings.ShortcutImgKey = value;
                JsonConfig = settings;
            }
        }

        //Key for ImgurSniper GIF Shortcut
        public static Key ShortcutGifKey {
            get => JsonConfig.ShortcutGifKey;
            set {
                Settings settings = JsonConfig;
                settings.ShortcutGifKey = value;
                JsonConfig = settings;
            }
        }

        //Use PrintKey for ImgurSniper Shortcut?
        public static bool UsePrint {
            get => JsonConfig.UsePrint;
            set {
                Settings settings = JsonConfig;
                settings.UsePrint = value;
                JsonConfig = settings;
            }
        }

        //The Path where images should be saved (if enabled)
        public static string SaveImagesPath {
            get {
                string path = JsonConfig.SaveImagesPath;
                string ret = CanWrite(path) ? path : ConfigPath;

                return ret;
            }
            set {
                Settings settings = JsonConfig;
                settings.SaveImagesPath = value;
                JsonConfig = settings;
            }
        }

        //Value wether Images should be saved or not
        public static bool SaveImages {
            get => JsonConfig.SaveImages;
            set {
                Settings settings = JsonConfig;
                settings.SaveImages = value;
                JsonConfig = settings;
            }
        }

        //Value wether upload Images to Imgur or copy to Clipboard
        public static bool ImgurAfterSnipe {
            get => JsonConfig.ImgurAfterSnipe;
            set {
                Settings settings = JsonConfig;
                settings.ImgurAfterSnipe = value;
                JsonConfig = settings;
            }
        }

        //Last Time, ImgurSniper checked for Updates
        public static DateTime LastChecked {
            get => JsonConfig.LastChecked;
            set {
                Settings settings = JsonConfig;
                settings.LastChecked = value;
                JsonConfig = settings;
            }
        }

        //Text Language
        public static string Language {
            get => JsonConfig.Language;
            set {
                Settings settings = JsonConfig;
                settings.Language = value;
                JsonConfig = settings;
            }
        }

        //Frames per Second of GIF Capture
        public static int GifFps {
            get => JsonConfig.GifFps;
            set {
                Settings settings = JsonConfig;
                settings.GifFps = value;
                JsonConfig = settings;
            }
        }

        //Maximum GIF Length in Milliseconds
        public static int GifLength {
            get => JsonConfig.GifLength;
            set {
                Settings settings = JsonConfig;
                settings.GifLength = value;
                JsonConfig = settings;
            }
        }

        //Value whether Magnifying Glass should be enabled or not
        public static bool MagnifyingGlassEnabled {
            get => JsonConfig.MagnifyingGlassEnabled;
            set {
                Settings settings = JsonConfig;
                settings.MagnifyingGlassEnabled = value;
                JsonConfig = settings;
            }
        }

        //Value whether ImgurSniper should automatically search for Updates
        public static bool AutoUpdate {
            get => JsonConfig.AutoUpdate;
            set {
                Settings settings = JsonConfig;
                settings.AutoUpdate = value;
                JsonConfig = settings;
            }
        }

        //Value wether run ImgurSniper as a Background Task on Boot or not
        public static bool RunOnBoot {
            get => JsonConfig.RunOnBoot;
            set {
                Settings settings = JsonConfig;
                settings.RunOnBoot = value;
                JsonConfig = settings;
            }
        }

        //Count of Commits for this ImgurSniper Version (for checking for Updates)
        public static int CurrentCommits {
            get => JsonConfig.CurrentCommits;
            set {
                Settings settings = JsonConfig;
                settings.CurrentCommits = value;
                JsonConfig = settings;
            }
        }

        //Count of total Commits on GitHub
        public static int TotalCommits {
            get => JsonConfig.TotalCommits;
            set {
                Settings settings = JsonConfig;
                settings.TotalCommits = value;
                JsonConfig = settings;
            }
        }

        //Is an Update Available and not yet downloaded?
        public static bool UpdateAvailable {
            get => JsonConfig.UpdateAvailable;
            set {
                Settings settings = JsonConfig;
                settings.UpdateAvailable = value;
                JsonConfig = settings;
            }
        }

        //Show Mouse Cursor on Screenshot
        public static bool ShowMouse {
            get => JsonConfig.ShowMouse;
            set {
                Settings settings = JsonConfig;
                settings.ShowMouse = value;
                JsonConfig = settings;
            }
        }

        //Quality of Image (1 being lowest, 100 being highest Quality)
        public static long Quality {
            get => JsonConfig.Quality;
            set {
                Settings settings = JsonConfig;
                settings.Quality = value;
                JsonConfig = settings;
            }
        }
        #endregion

        #region Imgur Account
        //Path to Imgur User Refresh Token
        public static string TokenPath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper",
                "refreshtoken.imgurtoken");

        public static string ReadRefreshToken() {
            if (!File.Exists(TokenPath)) {
                File.Create(TokenPath);
                return null;
            }

            string token = File.ReadAllText(TokenPath);

            if (string.IsNullOrWhiteSpace(token))
                return null;

            token = Cipher.Decrypt(token, PassPhrase);

            return token;
        }

        public static void WriteRefreshToken(string token) {
            string encrToken = Cipher.Encrypt(token, PassPhrase);
            File.WriteAllText(TokenPath, encrToken);
        }

        public static void DeleteToken() {
            if (File.Exists(TokenPath)) {
                File.Delete(TokenPath);
            }
        }
        #endregion

        public static Settings JsonConfig;

        public static string ConfigPath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");

        public static string ConfigFile
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper", "config.json");

        public static string InstallDir => AppDomain.CurrentDomain.BaseDirectory;

        //Salt for Cipher Encryption
        private static string PassPhrase => "ImgurSniper User-Login File_PassPhrase :)";


        //Save current Config to config.json
        public static void Save() {
            Exists();
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(JsonConfig));
        }

        public static void Exists() {
            if (!Directory.Exists(ConfigPath)) {
                Directory.CreateDirectory(ConfigPath);
            }

            if (!File.Exists(ConfigFile)) {
                File.WriteAllText(ConfigFile, "{}");
            }
        }

        //Check for Write Access to Directory
        public static bool CanWrite(string path) {
            try {
                bool writeAllow = false;
                bool writeDeny = false;
                DirectorySecurity accessControlList = Directory.GetAccessControl(path);
                AuthorizationRuleCollection accessRules = accessControlList?.GetAccessRules(true, true,
                    typeof(SecurityIdentifier));
                if (accessRules == null) {
                    return false;
                }

                foreach (FileSystemAccessRule rule in accessRules) {
                    if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write) {
                        continue;
                    }

                    switch (rule.AccessControlType) {
                        case AccessControlType.Allow:
                            writeAllow = true;
                            break;
                        default:
                            writeDeny = true;
                            break;
                    }
                }

                return writeAllow && !writeDeny;
            } catch {
                return false;
            }
        }
    }
    public class Settings {
        public bool AllMonitors = true;
        public bool AutoUpdate = true;
        public bool ShowMouse = true;
        public bool UpdateAvailable;
        public bool UsePrint;
        public bool ImgurAfterSnipe = true;
        public bool MagnifyingGlassEnabled;
        public bool OpenBrowserAfterUpload = true;
        public bool OpenFileAfterSnap;
        public bool RunOnBoot = true;
        public bool SaveImages;

        public int CurrentCommits = 999;
        public int TotalCommits = 999;
        public int GifFps = 10;
        public int GifLength = 12000;

        public long Quality = 90;

        public string Language;
        public string SaveImagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "ImgurSniperImages");

        public Key ShortcutGifKey = Key.G;
        public Key ShortcutImgKey = Key.X;

        public DateTime LastChecked = DateTime.Now;

        public ImageFormat ImageFormat = ImageFormat.Png;
    }
}