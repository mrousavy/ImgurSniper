using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ImgurSniper {
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
        private static string _passPhrase {
            get {
                return "ImgurSniper v" + _fileVersion + " User-Login File_PassPhrase :)";
            }
        }

        public static string _config {
            get {
                string FilePath = Path.Combine(_path, "Config.imgursniperconfig");
                return FilePath;
            }
        }

        //Path to Imgur Refresh Token
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

            string token = File.ReadAllText(_tokenFile);

            token = Cipher.Decrypt(token, _passPhrase);

            return token;
        }
    }
}
