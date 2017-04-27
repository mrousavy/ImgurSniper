using System;
using System.IO;
using System.Windows.Media;

namespace ImgurSniper.Libraries.Helper {
    public static class Helpers {
        public static void CreateDirectoryFromFilePath(string path) {
            if (!string.IsNullOrEmpty(path)) {
                CreateDirectoryFromDirectoryPath(Path.GetDirectoryName(path));
            }
        }
        public static void CreateDirectoryFromDirectoryPath(string path) {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path)) {
                try {
                    Directory.CreateDirectory(path);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
        }
        public static string GetAbsolutePath(string path) {
            path = ExpandFolderVariables(path);

            if (!Path.IsPathRooted(path)) // Is relative path?
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            return Path.GetFullPath(path);
        }
        public static string ExpandFolderVariables(string path) {
            if (!string.IsNullOrEmpty(path)) {
                try {
                    GetEnums<Environment.SpecialFolder>().ForEach(x => path = path.Replace($"%{x}%", Environment.GetFolderPath(x), StringComparison.InvariantCultureIgnoreCase));
                    path = Environment.ExpandEnvironmentVariables(path);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

            return path;
        }
        public static string GetVariableFolderPath(string path) {
            if (!string.IsNullOrEmpty(path)) {
                try {
                    GetEnums<Environment.SpecialFolder>().ForEach(x => path = path.Replace(Environment.GetFolderPath(x), $"%{x}%", StringComparison.InvariantCultureIgnoreCase));
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

            return path;
        }
        public static T[] GetEnums<T>() {
            return (T[])Enum.GetValues(typeof(T));
        }

        //Play the Blop Success Sound
        public static void PlayBlop() {
            try {
                MediaPlayer player = new MediaPlayer { Volume = 30 };
                player.MediaEnded += delegate {
                    player.Close();
                };

                string path = Path.Combine(ConfigHelper.ProgramFiles, "Resources\\Blop.wav");

                player.Open(new Uri(path));
                player.Play();
            } catch {
                // ignored
            }
        }
    }
}
