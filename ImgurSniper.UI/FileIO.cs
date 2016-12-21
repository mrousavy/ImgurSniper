using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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

        public enum ConfigType { AfterSnipeAction }


        /// <summary>
        /// Encryptes the Input ClientID and ClientSecret and saves it to the file
        /// </summary>
        /// <param name="ClientID">Imgur ClientID</param>
        /// <param name="ClientSecret">Imgur ClientSecret</param>
        public static void SaveToFile(ConfigType type, string content) {
            string encr_content = Cipher.Encrypt(content, _passPhrase);

            string[] lines = ReadFromFile();

            bool found = false;

            for(int i = 0; i < lines.Length; i++) {
                string[] tmp = lines[i].Split(':');

                if(tmp[0] == type.ToString()) {
                    lines[i] = tmp[0] + ":" + content;
                    found = true;
                    break;
                }
            }
            if(!found) {
                File.AppendAllLines(_config, new string[] { type.ToString() + ":" + content });
            } else {
                File.WriteAllLines(_config, lines);
            }
        }


        /// <summary>
        /// Reads ClientID and ClientSecret from Encrypted File and Decryptes it
        /// </summary>
        /// <returns>A ImgurData Model with the decrypted ClientID and Secret</returns>
        public static string[] ReadFromFile() {
            string[] lines = File.ReadAllLines(_config);

            for(int i = 0; i < lines.Length; i++) {
                lines[i] = Cipher.Decrypt(lines[i], _passPhrase);
            }

            return lines;
        }


        /// <summary>
        /// A Model for handling ClientID and ClientSecret
        /// </summary>
        public class ImgurData {
            public string ClientID { get; set; }
            public string ClientSecret { get; set; }

            public ImgurData(string ClientID, string ClientSecret) {
                this.ClientID = ClientID;
                this.ClientSecret = ClientSecret;
            }
        }
    }
}
