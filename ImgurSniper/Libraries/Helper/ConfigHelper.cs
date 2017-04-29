using System;
using System.Drawing.Imaging;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Input;

namespace ImgurSniper.Libraries.Helper {
    public static class ConfigHelper {
        #region Properties
        //Value whether ImgurSniper should strech over all screens or not
        public static bool AllMonitors => JsonConfig.AllMonitors;

        //The Image Format for normal Images
        public static ImageFormat ImageFormat => JsonConfig.ImageFormat;

        //Value whether ImgurSniper should open the uploaded Image in Browser after upload
        public static bool OpenAfterUpload => JsonConfig.OpenAfterUpload;

        //Key for ImgurSniper Image Shortcut
        public static Key ShortcutImgKey => JsonConfig.ShortcutImgKey;

        //Key for ImgurSniper GIF Shortcut
        public static Key ShortcutGifKey => JsonConfig.ShortcutGifKey;

        //Use PrintKey for ImgurSniper Shortcut?
        public static bool UsePrint => JsonConfig.UsePrint;

        //The Path where images should be saved (if enabled)
        public static string SaveImagesPath {
            get {
                string path = JsonConfig.SaveImagesPath;
                string ret = CanWrite(path) ? path : ConfigPath;

                return ret;
            }
        }

        //Value wether Images should be saved or not
        public static bool SaveImages => JsonConfig.SaveImages;

        //Value wether upload Images to Imgur or copy to Clipboard
        public static bool ImgurAfterSnipe => JsonConfig.ImgurAfterSnipe;

        //Last Time, ImgurSniper checked for Updates
        public static DateTime LastChecked => JsonConfig.LastChecked;

        //Text Language
        public static string Language => JsonConfig.Language;

        //Frames per Second of GIF Capture
        public static int GifFps => JsonConfig.GifFps;

        //Maximum GIF Length in Milliseconds
        public static int GifLength => JsonConfig.GifLength;

        //Show Mouse Cursor on Screenshot
        public static bool ShowMouse => JsonConfig.ShowMouse;

        //Value whether ImgurSniper should automatically search for Updates
        public static bool AutoUpdate => JsonConfig.AutoUpdate;

        //Compression of Image (0 being lowest, 100 being highest Quality)
        public static byte Compression => JsonConfig.Compression;
        #endregion

        #region Imgur Account

        //Does Imgur Refresh Token exist?
        public static bool TokenExists => File.Exists(TokenPath);

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

            if (string.IsNullOrWhiteSpace(token)) {
                return null;
            }

            token = Cipher.Decrypt(token, PassPhrase);

            return token;
        }

        #endregion

        public static Settings JsonConfig;

        public static string ConfigPath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");

        public static string ConfigFile
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper",
                "config.json");

        //Path to Installation Folder
        public static string InstallDir => AppDomain.CurrentDomain.BaseDirectory;

        //Salt for Cipher Encryption
        private const string PassPhrase = "ImgurSniper User-Login File_PassPhrase :)";

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
        public bool UsePrint = false;
        public bool ImgurAfterSnipe = true;
        public bool OpenAfterUpload = true;
        public bool SaveImages = false;

        public byte Compression = 90;

        public int GifFps = 10;
        public int GifLength = 12000;

        public string Language = null;
        public string SaveImagesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ImgurSniperImages");

        public Key ShortcutGifKey = Key.G;
        public Key ShortcutImgKey = Key.X;

        public DateTime LastChecked = DateTime.Now;

        public ImageFormat ImageFormat = ImageFormat.Png;
    }
}