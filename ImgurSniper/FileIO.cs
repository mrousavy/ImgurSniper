using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace ImgurSniper {
    public static class FileIO {

        //Value whether Magnifying Glass should be enabled or not
        public static bool MagnifyingGlassEnabled {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "Magnifyer") {
                            return bool.Parse(config[1]);
                        }
                    }
                    return false;
                } catch(Exception) {
                    return false;
                }
            }
        }
        //Value whether ImgurSniper should strech over all screens or not
        public static bool AllMonitors {
            get {
                try {
                    bool all = true;

                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "SnipeMonitor") {
                            all = config[1] == "All";
                            break;
                        }
                    }

                    return all;
                } catch(Exception) {
                    return false;
                }
            }
        }
        //Value whether ImgurSniper should use PNG Image Format
        public static bool UsePNG {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "ImageFormat") {
                            return config[1] == "PNG";
                        }
                    }

                    return false;
                } catch(Exception) {
                    return false;
                }
            }
        }
        //Value whether ImgurSniper should open the uploaded Image in Browser after upload
        public static bool OpenAfterUpload {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "OpenAfterUpload") {
                            return bool.Parse(config[1]);
                        }
                    }

                    return false;
                } catch(Exception) {
                    return false;
                }
            }
        }
        //Key for ImgurSniper Shortcut
        public static Key ShortcutKey {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "ShortcutKey") {
                            return (Key)Enum.Parse(typeof(Key), config[1]);
                        }
                    }

                    return Key.X;
                } catch(Exception) {
                    return Key.X;
                }
            }
        }
        //Use PrintKey for ImgurSniper Shortcut?
        public static bool UsePrint {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "UsePrint") {
                            return bool.Parse(config[1]);
                        }
                    }

                    return false;
                } catch(Exception) {
                    return false;
                }
            }
        }
        //The Path where images should be saved (if enabled)
        public static string SaveImagesPath {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "Path") {
                            return config[1];
                        }
                    }

                    return "";
                } catch(Exception) {
                    return "";
                }
            }
        }
        //Value wether Images should be saved or not
        public static bool SaveImages {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "SaveImages") {
                            return bool.Parse(config[1]);
                        }
                    }

                    return false;
                } catch(Exception) {
                    return false;
                }
            }
        }
        //Value wether run ImgurSniper as a Background Task on Boot or not
        public static bool RunOnBoot {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "RunOnBoot") {
                            return bool.Parse(config[1]);
                        }
                    }

                    return true;
                } catch(Exception) {
                    return true;
                }
            }
        }
        //Value wether upload Images to Imgur or copy to Clipboard
        public static bool ImgurOnUpload {
            get {
                try {
                    string[] lines = FileIO.ReadConfig();
                    foreach(string line in lines) {
                        string[] config = line.Split(';');

                        if(config[0] == "AfterSnipeAction") {
                            if(config[1] == "Clipboard")
                                return false;
                            else
                                return true;
                        }
                    }

                    return true;
                } catch(Exception) {
                    return true;
                }
            }
        }


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
                //string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                //ProgramFiles = Path.Combine(ProgramFiles, "ImgurSniper");
                return AppDomain.CurrentDomain.BaseDirectory;
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
