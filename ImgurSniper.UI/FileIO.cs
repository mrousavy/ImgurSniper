using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ImgurSniper.UI {
    public static class FileIO {

        public static string _fileVersion {
            get {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }
        public static string _path {
            get {
                string Documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(Documents, "ImgurSniper");
            }
        }
        public static string _programFiles {
            get {
                string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                return Path.Combine(ProgramFiles, "ImgurSniper");
            }
        }

        private static string _config {
            get {
                string FilePath = Path.Combine(_path, "Config.imgursniperconfig");
                return FilePath;
            }
        }
        private static string _passPhrase {
            get {
                return "ImgurSniper v" + _fileVersion + " User-Login File_PassPhrase :)";
            }
        }


        public static string _tokenFile {
            get {
                string FilePath = Path.Combine(_path, "Token.imgursnipertoken");
                return FilePath;
            }
        }

        public static bool TokenExists {
            get {
                return File.Exists(_tokenFile);
            }
        }
        public enum ConfigType { AfterSnipeAction, SaveImages, Magnifyer, OpenAfterUpload, SnipeMonitor, Path, ImageFormat }


        /// <summary>
        /// Encryptes the Input ClientID and ClientSecret and saves it to the file
        /// </summary>
        /// <param name="ClientID">Imgur ClientID</param>
        /// <param name="ClientSecret">Imgur ClientSecret</param>
        public static void SaveConfig(ConfigType type, string content) {
            new Thread(() => {
                string[] lines = ReadConfig();

                bool found = false;

                for(int i = 0; i < lines.Length; i++) {
                    string[] tmp = lines[i].Split(';');

                    if(tmp[0] == type.ToString()) {
                        lines[i] = tmp[0] + ";" + content;
                        found = true;
                        break;
                    }
                }

                if(!found) {
                    File.AppendAllLines(_config, new string[] { type.ToString() + ";" + content });
                } else {
                    File.WriteAllLines(_config, lines);
                }
            }).Start();
        }


        //Check if all ImgurSniper Binaries are Installed
        public static bool CheckFileIntegrity() {
            try {
                bool ret = (
                    File.Exists(_programFiles + "\\ImgurSniper.exe") &&
                    File.Exists(_programFiles + "\\Imgur.API.dll") &&
                    File.Exists(_programFiles + "\\Newtonsoft.Json.dll") &&
                    File.Exists(_programFiles + "\\Toast.dll") &&
                    File.Exists(_programFiles + "\\Resources\\Camera_Shutter.wav"));

                return ret;
            } catch(Exception) {
                return false;
            }
        }

        public static void WipeUserData() {
            Directory.Delete(_path, true);
        }


        /// <summary>
        /// Reads ClientID and ClientSecret from Encrypted File and Decryptes it
        /// </summary>
        /// <returns>A ImgurData Model with the decrypted ClientID and Secret</returns>
        public static string[] ReadConfig() {
            if(!File.Exists(_config)) {
                using(File.Create(_config)) { }
                return new string[] { };
            }

            string[] lines = File.ReadAllLines(_config);

            return lines;
        }


        public static string ReadRefreshToken() {
            if(!TokenExists) {
                return null;
            }

            try {
                string token = File.ReadAllText(_tokenFile);

                token = Cipher.Decrypt(token, _passPhrase);

                return token;
            } catch(Exception) {
                try {
                    File.Delete(_tokenFile);
                } catch(Exception) { }
                return null;
            }
        }


        public static void WriteRefreshToken(string token) {
            new Thread(() => {
                if(!TokenExists) {
                    using(File.Create(_tokenFile)) { }
                }

                try {
                    string encr_token = Cipher.Encrypt(token, _passPhrase);

                    File.WriteAllText(_tokenFile, encr_token);
                } catch(Exception) {
                    File.Delete(_tokenFile);
                }
            }).Start();
        }

        public static void DeleteToken() {
            try {
                File.Delete(_tokenFile);
            } catch(Exception) { }
        }
    }
}
