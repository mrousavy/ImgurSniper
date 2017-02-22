using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ImgurSniper.UI {
    public static class FileIO {

        //Value whether Magnifying Glass should be enabled or not
        public static bool MagnifyingGlassEnabled {
            get {
                try {
                    return (bool)Settings.Default.MagnifyingGlassEnabled;
                } catch {
                    return false;
                }
            }
        }
        //Value whether ImgurSniper should strech over all screens or not
        public static bool AllMonitors {
            get {
                try {
                    return (bool)Settings.Default.AllMonitors;
                } catch {
                    return true;
                }
            }
        }
        //Value whether ImgurSniper should use PNG Image Format
        public static bool UsePNG {
            get {
                try {
                    return (bool)Settings.Default.UsePNG;
                } catch {
                    return false;
                }
            }
        }
        //Value whether ImgurSniper should open the uploaded Image in Browser after upload
        public static bool OpenAfterUpload {
            get {
                try {
                    return (bool)Settings.Default.OpenAfterUpload;
                } catch {
                    return true;
                }
            }
        }
        //Key for ImgurSniper Shortcut
        public static System.Windows.Input.Key ShortcutKey {
            get {
                try {
                    return (System.Windows.Input.Key)Enum.Parse(
                        typeof(System.Windows.Input.Key),
                        (Settings.Default.ShortcutKey).ToString());
                } catch {
                    return System.Windows.Input.Key.X;
                }
            }
        }
        //Use PrintKey for ImgurSniper Shortcut?
        public static bool UsePrint {
            get {
                try {
                    return Settings.Default.UsePrint;
                } catch {
                    return false;
                }
            }
        }
        //The Path where images should be saved (if enabled)
        public static string SaveImagesPath {
            get {
                try {
                    string path = Settings.Default.SaveImagesPath;

                    return string.IsNullOrWhiteSpace(path) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper Images") : path;
                } catch {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper Images");
                }
            }
        }
        //Value wether Images should be saved or not
        public static bool SaveImages {
            get {
                try {
                    return (bool)Settings.Default.SaveImages;
                } catch {
                    return false;
                }
            }
        }
        //Value wether run ImgurSniper as a Background Task on Boot or not
        public static bool RunOnBoot {
            get {
                try {
                    return (bool)Settings.Default.RunOnBoot;
                } catch {
                    return true;
                }
            }
        }
        //Value wether upload Images to Imgur or copy to Clipboard
        public static bool ImgurAfterSnipe {
            get {
                try {
                    return (bool)Settings.Default.ImgurAfterSnipe;
                } catch {
                    return true;
                }
            }
        }
        //Value wether "Upload Image to Imgur" is already in Registry
        public static bool IsInContextMenu {
            get {
                try {
                    return Settings.Default.IsInContextMenu;
                } catch {
                    return false;
                }
            }
        }

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

        //Config Keys
        public enum ConfigType { UsePNG, MagnifyingGlassEnabled, AllMonitors, OpenAfterUpload, ShortcutKey, UsePrint, SaveImages, RunOnBoot, ImgurAfterSnipe, IsInContextMenu, SaveImagesPath }

        //Saves a value in User Settings
        public static void SaveConfig(ConfigType type, object content) {
            Settings.Default[type.ToString()] = content;
            Settings.Default.Save();
        }

        //Resets User Settings
        public static void WipeUserData() {
            Settings.Default.Reset();
        }

        #region Imgur Account
        //Does Imgur Refresh Token exist?
        public static bool TokenExists => File.Exists(TokenPath);

        //Path to Imgur User Refresh Token
        public static string TokenPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImgurSniper", "refreshtoken.imgurtoken");

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
            string encr_token = Cipher.Encrypt(token, _passPhrase);
            File.WriteAllText(TokenPath, encr_token);
        }

        public static void DeleteToken() {
            if(File.Exists(TokenPath))
                File.Delete(TokenPath);
        }
        #endregion
    }
}
