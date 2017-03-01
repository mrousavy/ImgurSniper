using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace ImgurSniper {
    public static class FileIO {
        //Config Keys
        public enum ConfigType {
            AfterSnipeAction,
            SaveImages,
            Magnifyer,
            OpenAfterUpload,
            SnipeMonitor,
            Path,
            ImageFormat,
            RunOnBoot,
            UsePrint,
            IsInContextMenu
        }

        //Value whether Magnifying Glass should be enabled or not
        public static bool MagnifyingGlassEnabled {
            get {
                try {
                    return JsonConfig.MagnifyingGlassEnabled;
                } catch {
                    return false;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.MagnifyingGlassEnabled = value;
                JsonConfig = settings;
            }
        }

        //Value whether ImgurSniper should strech over all screens or not
        public static bool AllMonitors {
            get {
                try {
                    return JsonConfig.AllMonitors;
                } catch {
                    return true;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.AllMonitors = value;
                JsonConfig = settings;
            }
        }

        //Value whether ImgurSniper should use PNG Image Format
        public static bool UsePNG {
            get {
                try {
                    return JsonConfig.UsePNG;
                } catch {
                    return false;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.UsePNG = value;
                JsonConfig = settings;
            }
        }

        //Value whether ImgurSniper should open the uploaded Image in Browser after upload
        public static bool OpenAfterUpload {
            get {
                try {
                    return JsonConfig.OpenAfterUpload;
                } catch {
                    return true;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.OpenAfterUpload = value;
                JsonConfig = settings;
            }
        }

        //Key for ImgurSniper Shortcut
        public static Key ShortcutKey {
            get {
                try {
                    return JsonConfig.ShortcutKey;
                } catch {
                    return Key.X;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.ShortcutKey = value;
                JsonConfig = settings;
            }
        }

        //Use PrintKey for ImgurSniper Shortcut?
        public static bool UsePrint {
            get {
                try {
                    return JsonConfig.UsePrint;
                } catch {
                    return false;
                }
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
                try {
                    string path = JsonConfig.SaveImagesPath;

                    return string.IsNullOrWhiteSpace(path)
                        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            "ImgurSniper Images")
                        : path;
                } catch {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "ImgurSniper Images");
                }
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
                try {
                    return JsonConfig.SaveImages;
                } catch {
                    return false;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.SaveImages = value;
                JsonConfig = settings;
            }
        }

        //Value wether run ImgurSniper as a Background Task on Boot or not
        public static bool RunOnBoot {
            get {
                try {
                    return JsonConfig.RunOnBoot;
                } catch {
                    return true;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.RunOnBoot = value;
                JsonConfig = settings;
            }
        }

        //Value wether upload Images to Imgur or copy to Clipboard
        public static bool ImgurAfterSnipe {
            get {
                try {
                    return JsonConfig.ImgurAfterSnipe;
                } catch {
                    return true;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.ImgurAfterSnipe = value;
                JsonConfig = settings;
            }
        }

        //Value wether "Upload Image to Imgur" is already in Registry
        public static bool IsInContextMenu {
            get {
                try {
                    return JsonConfig.IsInContextMenu;
                } catch {
                    return false;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.IsInContextMenu = value;
                JsonConfig = settings;
            }
        }

        //Last Time, ImgurSniper checked for Updates
        public static DateTime LastChecked {
            get {
                try {
                    return JsonConfig.LastChecked;
                } catch {
                    return DateTime.Now;
                }
            }
            set {
                Settings settings = JsonConfig;
                settings.LastChecked = value;
                JsonConfig = settings;
            }
        }

        public static Settings JsonConfig {
            get {
                Exists();
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(ConfigFile));
            }
            set {
                Exists();
                File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(value));
            }
        }

        public static string ConfigPath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper");

        public static string ConfigFile
            =>
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper",
                    "config.json");

        //Path to Installation Folder
        public static string _programFiles => AppDomain.CurrentDomain.BaseDirectory;

        //Version of ImgurSniper
        public static string _fileVersion {
            get {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }

        //Salt for Cipher Encryption
        private static string _passPhrase => "ImgurSniper v" + _fileVersion + " User-Login File_PassPhrase :)";

        private static void Exists() {
            if(!Directory.Exists(ConfigPath)) {
                Directory.CreateDirectory(ConfigPath);
            }

            if(!File.Exists(ConfigFile)) {
                File.WriteAllText(ConfigFile, "{}");
            }
        }

        //Resets User Settings
        public static void WipeUserData() {
            JsonConfig = new Settings();
        }

        public class Settings {
            public bool AllMonitors = true;
            public int CurrentCommits = 999;
            public bool ImgurAfterSnipe = true;
            public bool IsInContextMenu;
            public DateTime LastChecked = DateTime.Now;
            public bool MagnifyingGlassEnabled = true;
            public bool OpenAfterUpload = true;
            public bool RunOnBoot = true;
            public bool SaveImages;

            public string SaveImagesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper Images");

            public Key ShortcutKey = Key.X;
            public bool UsePNG = true;
            public bool UsePrint;
        }

        #region Imgur Account

        //Does Imgur Refresh Token exist?
        public static bool TokenExists => File.Exists(TokenPath);

        //Path to Imgur User Refresh Token
        public static string TokenPath
            =>
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper",
                    "refreshtoken.imgurtoken");

        public static string ReadRefreshToken() {
            if(!File.Exists(TokenPath)) {
                File.Create(TokenPath);
                return null;
            }

            string token = File.ReadAllText(TokenPath);
            token = Cipher.Decrypt(token, _passPhrase);

            return token;
        }

        public static void WriteRefreshToken(string token) {
            string encrToken = Cipher.Encrypt(token, _passPhrase);
            File.WriteAllText(TokenPath, encrToken);
        }

        public static void DeleteToken() {
            if(File.Exists(TokenPath)) {
                File.Delete(TokenPath);
            }
        }

        #endregion
    }
}