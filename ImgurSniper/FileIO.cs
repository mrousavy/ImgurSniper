using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace ImgurSniper {
    public static class FileIO {
        //Value whether ImgurSniper should strech over all screens or not
        public static bool AllMonitors {
            get {
                return JsonConfig.AllMonitors;
            }
            set {
                Settings settings = JsonConfig;
                settings.AllMonitors = value;
                JsonConfig = settings;
            }
        }

        //The Image Format for normal Images
        public static ImageFormat ImageFormat {
            get {
                return JsonConfig.ImageFormat;
            }
            set {
                Settings settings = JsonConfig;
                settings.ImageFormat = value;
                JsonConfig = settings;
            }
        }

        //Value whether ImgurSniper should open the uploaded Image in Browser after upload
        public static bool OpenAfterUpload {
            get {
                return JsonConfig.OpenAfterUpload;
            }
            set {
                Settings settings = JsonConfig;
                settings.OpenAfterUpload = value;
                JsonConfig = settings;
            }
        }

        //Key for ImgurSniper Image Shortcut
        public static Key ShortcutImgKey {
            get {
                return JsonConfig.ShortcutImgKey;
            }
            set {
                Settings settings = JsonConfig;
                settings.ShortcutImgKey = value;
                JsonConfig = settings;
            }
        }

        //Key for ImgurSniper GIF Shortcut
        public static Key ShortcutGifKey {
            get {
                return JsonConfig.ShortcutGifKey;
            }
            set {
                Settings settings = JsonConfig;
                settings.ShortcutGifKey = value;
                JsonConfig = settings;
            }
        }

        //Use PrintKey for ImgurSniper Shortcut?
        public static bool UsePrint {
            get {
                return JsonConfig.UsePrint;
            }
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
            get {
                return JsonConfig.SaveImages;
            }
            set {
                Settings settings = JsonConfig;
                settings.SaveImages = value;
                JsonConfig = settings;
            }
        }

        //Value wether upload Images to Imgur or copy to Clipboard
        public static bool ImgurAfterSnipe {
            get {
                return JsonConfig.ImgurAfterSnipe;
            }
            set {
                Settings settings = JsonConfig;
                settings.ImgurAfterSnipe = value;
                JsonConfig = settings;
            }
        }

        //Last Time, ImgurSniper checked for Updates
        public static DateTime LastChecked {
            get {
                return JsonConfig.LastChecked;
            }
            set {
                Settings settings = JsonConfig;
                settings.LastChecked = value;
                JsonConfig = settings;
            }
        }

        //Text Language
        public static string Language {
            get {
                return JsonConfig.Language;
            }
            set {
                Settings settings = JsonConfig;
                settings.Language = value;
                JsonConfig = settings;
            }
        }

        //Frames per Second of GIF Capture
        public static int GifFps {
            get {
                return JsonConfig.GifFps;
            }
            set {
                Settings settings = JsonConfig;
                settings.GifFps = value;
                JsonConfig = settings;
            }
        }

        //Maximum GIF Length in Milliseconds
        public static int GifLength {
            get {
                return JsonConfig.GifLength;
            }
            set {
                Settings settings = JsonConfig;
                settings.GifLength = value;
                JsonConfig = settings;
            }
        }

        //Show Mouse Cursor on Screenshot
        public static bool ShowMouse {
            get {
                return JsonConfig.ShowMouse;
            }
            set {
                Settings settings = JsonConfig;
                settings.ShowMouse = value;
                JsonConfig = settings;
            }
        }

        public static Settings JsonConfig {
            get {
                try {
                    Exists();
                    return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ConfigFile));
                } catch {
                    MessageBox.Show(
                        "An Error occured, please make sure that you have\nread and Write Access to the following Path:\n" +
                        ConfigFile,
                        "Fatal ImgurSniper Configuration File Error");
                    throw new Exception(
                        "An Error occured, please make sure that you have\nread and Write Access to the following Path:\n" +
                        ConfigFile);
                }
            }
            set {
                try {
                    Exists();
                    File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(value));
                } catch {
                    MessageBox.Show(
                        "An Error occured, please make sure that you have\nread and Write Access to the following Path:\n" +
                        ConfigFile,
                        "Fatal ImgurSniper Configuration File Error");
                }
            }
        }

        public static string ConfigPath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");

        public static string ConfigFile
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper",
                "config.json");

        //Path to Installation Folder
        public static string ProgramFiles => AppDomain.CurrentDomain.BaseDirectory;

        //Version of ImgurSniper
        public static string FileVersion {
            get {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }

        //Salt for Cipher Encryption
        private static string PassPhrase => "ImgurSniper v" + FileVersion + " User-Login File_PassPhrase :)";

        private static void Exists() {
            if(!Directory.Exists(ConfigPath)) {
                Directory.CreateDirectory(ConfigPath);
            }

            if(!File.Exists(ConfigFile)) {
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
                if(accessRules == null) {
                    return false;
                }

                foreach(FileSystemAccessRule rule in accessRules) {
                    if((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write) {
                        continue;
                    }

                    if(rule.AccessControlType == AccessControlType.Allow) {
                        writeAllow = true;
                    } else if(rule.AccessControlType == AccessControlType.Deny) {
                        writeDeny = true;
                    }
                }

                return writeAllow && !writeDeny;
            } catch {
                return false;
            }
        }

        public class Settings {
            public bool AllMonitors = true;
            public bool AutoUpdate = true;
            public bool ShowMouse = false;
            public bool UpdateAvailable = false;
            public bool UsePrint = false;
            public bool ImgurAfterSnipe = true;
            public bool IsInContextMenu = false;
            public bool MagnifyingGlassEnabled = true;
            public bool OpenAfterUpload = true;
            public bool RunOnBoot = true;
            public bool SaveImages = false;

            public int CurrentCommits = 999;
            public int GifFps = 10;
            public int GifLength = 10000;

            public string Language = null;
            public string SaveImagesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ImgurSniperImages");

            public Key ShortcutGifKey = Key.G;
            public Key ShortcutImgKey = Key.X;

            public DateTime LastChecked = DateTime.Now;

            public ImageFormat ImageFormat = ImageFormat.Png;
        }

        #region Imgur Account

        //Does Imgur Refresh Token exist?
        public static bool TokenExists => File.Exists(TokenPath);

        //Path to Imgur User Refresh Token
        public static string TokenPath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper",
                "refreshtoken.imgurtoken");

        public static string ReadRefreshToken() {
            if(!File.Exists(TokenPath)) {
                File.Create(TokenPath);
                return null;
            }

            string token = File.ReadAllText(TokenPath);
            token = Cipher.Decrypt(token, PassPhrase);

            return token;
        }

        #endregion
    }
}