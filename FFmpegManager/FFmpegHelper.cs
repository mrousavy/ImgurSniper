using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace FFmpegManager {
    internal static class FFmpegHelper {
        //Needs requireAdministrator in manifest if write protected Directory
        internal static string Documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        internal static string FFmpegDirectory = Path.Combine(Documents, "ImgurSniper");
        internal static string FFmpegPath = Path.Combine(FFmpegDirectory, "ffmpeg.exe");
        internal const string FFmpegUrl = "https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-20190502-7eba264-win64-static.zip";

        internal static bool FFmpegInstalled {
            get => File.Exists(FFmpegPath);
        }

        internal static void RemoveFFmpeg() {
            if (Directory.Exists(FFmpegDirectory))
                Directory.Delete(FFmpegDirectory, true);
            if (File.Exists(FFmpegPath))
                File.Delete(FFmpegPath);
        }

        internal static async Task<string> DownloadFFmpeg(DownloadProgressChangedEventHandler progressChanged = null) {
            string tmp = Path.GetTempFileName();

            using (WebClient client = new WebClient()) {
                client.DownloadProgressChanged += progressChanged;
                await client.DownloadFileTaskAsync(new Uri(FFmpegUrl), tmp);
            }

            return tmp;
        }

        internal static void InstallFFmpeg(string archive) {
            if (!Directory.Exists(FFmpegDirectory))
                Directory.CreateDirectory(FFmpegDirectory);
            if (File.Exists(FFmpegPath))
                File.Delete(FFmpegPath);

            string tmp = Path.GetTempFileName();
            File.Delete(tmp);

            using (ZipArchive file = ZipFile.OpenRead(archive)) {
                foreach (ZipArchiveEntry entry in file.Entries) {
                    if (entry.Name == "ffmpeg.exe") {
                        entry.ExtractToFile(tmp);
                        break;
                    }
                }
            }
            File.Delete(archive);

            if (File.Exists(tmp)) {
                File.Move(tmp, FFmpegPath);
            } else {
                throw new Exception("Could not extract FFmpeg.exe!");
            }
        }
    }
}
