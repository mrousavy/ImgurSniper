#region License Information (GPL v3)

/*
    Source code provocatively stolen from ShareX: https://github.com/ShareX/ShareX.
    (Seriously, awesome work over there, I took some parts of the Code to make
    ImgurSniper.)
    Their License:

    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2017 ShareX Team
    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


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
                    // ReSharper disable once AccessToModifiedClosure
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

                string path = Path.Combine(ConfigHelper.InstallDir, "Resources\\Blop.wav");

                player.Open(new Uri(path));
                player.Play();
            } catch {
                // ignored
            }
        }


        public static void WriteError(Exception ex) {
            try {
                string errorFile = Path.Combine(ConfigHelper.DocumentsDirectory, "error.txt");
                string nl = Environment.NewLine;
                string errorDetails = $"!ImgurSniper Error @{DateTime.Now}" + nl +
                        $"    Error Message: {ex.Message}" + nl + nl +
                        $"    Error Stacktrace: {ex.StackTrace}";

                if (File.Exists(errorFile)) {
                    File.AppendAllText(errorFile,
                        nl + nl + "---------------------------------------------------------" + errorDetails);
                } else {
                    File.WriteAllText(errorFile,
                        $"Details for an Exception in ImgurSniper. " +
                        "You can tell me about this error on http://www.github.com/mrousavy/ImgurSniper/issues so I can fix it as soon as possible!"
                        + nl + errorDetails);
                }
            } catch { }
        }
    }
}
