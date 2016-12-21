using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ImgurSniper {
    public static class ClientKeyIO {

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
        public static string ClientID {
            get {
                return "766263aaa4c9882";
            }
        }
        public static string ClientSecret {
            get {
                return "1f16f21e51e499422fb90ae670a1974e88f7c6ae";
            }
        }

        private static string _file {
            get {
                string FilePath = Path.Combine(_path, "User.imgursniperlogin");
                return FilePath;
            }
        }
        private static string _passPhrase {
            get {
                return "ImgurSniper v" + _fileVersion + " User-Login File_PassPhrase :)";
            }
        }


        /// <summary>
        /// Encryptes the Input ClientID and ClientSecret and saves it to the file
        /// </summary>
        /// <param name="ClientID">Imgur ClientID</param>
        /// <param name="ClientSecret">Imgur ClientSecret</param>
        public static void SaveToFile(string ClientID, string ClientSecret) {
            string encr_ClientID = Cipher.Encrypt(ClientID, _passPhrase);
            string encr_ClientSecret = Cipher.Encrypt(ClientSecret, _passPhrase);

            string[] lines = new string[] { encr_ClientID, encr_ClientSecret };

            File.WriteAllLines(_file, lines);
        }


        /// <summary>
        /// Reads ClientID and ClientSecret from Encrypted File and Decryptes it
        /// </summary>
        /// <returns>A ImgurData Model with the decrypted ClientID and Secret</returns>
        public static ImgurData ReadFromFile() {
            string[] lines = File.ReadAllLines(_file);

            if(lines.Length != 2)
                throw new Exception("Invalid File Format!");

            string ClientID = Cipher.Decrypt(lines[0], _passPhrase);
            string ClientSecret = Cipher.Decrypt(lines[1], _passPhrase);

            return new ImgurData(ClientID, ClientSecret);
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
