using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.ScreenCapture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace ImgurSniper.Libraries.FFmpeg {
    public class FFmpegHelper : ExternalCliManager {
        public const string SourceNone = "None";
        public const string SourceGdiGrab = "GDI grab";
        public const string SourceVideoDevice = "screen-capture-recorder";
        public const string SourceAudioDevice = "virtual-audio-capturer";
        public const string DeviceSetupPath = "Recorder-devices-setup.exe";

        public const int Libmp3LameQscaleEnd = 9;

        public event Action RecordingStarted;

        public StringBuilder Output { get; }
        public ScreencastOptions Options { get; }

        private bool _recordingStarted;
        private int _closeTryCount;

        public FFmpegHelper(ScreencastOptions options) {
            Output = new StringBuilder();
            OutputDataReceived += FFmpegHelper_DataReceived;
            ErrorDataReceived += FFmpegHelper_DataReceived;
            Options = options;
            Helpers.CreateDirectoryFromFilePath(Options.OutputPath);
        }

        private void FFmpegHelper_DataReceived(object sender, DataReceivedEventArgs e) {
            lock (this) {
                if (!string.IsNullOrEmpty(e.Data)) {
                    Output.AppendLine(e.Data);

                    if (!_recordingStarted && e.Data.IndexOf("Press [q] to stop", StringComparison.InvariantCultureIgnoreCase) >= 0) {
                        _recordingStarted = true;
                        OnRecordingStarted();
                    }
                }
            }
        }

        public bool Record() {
            _recordingStarted = false;
            return Run(Options.FFmpeg.FFmpegPath, Options.GetFFmpegCommands());
        }

        protected void OnRecordingStarted() {
            RecordingStarted?.Invoke();
        }

        public bool EncodeGif(string input, string output) {
            bool result;
            string dirName = Path.GetDirectoryName(Options.FFmpeg.FFmpegPath);
            if (dirName == null)
                throw new ArgumentNullException(nameof(dirName));

            string palettePath = Path.Combine(dirName, "palette.png");

            try {
                // https://ffmpeg.org/ffmpeg-filters.html#palettegen-1
                result = Run(Options.FFmpeg.FFmpegPath, string.Format("-y -i \"{0}\" -vf \"palettegen=stats_mode={2}\" \"{1}\"", input, palettePath, Options.FFmpeg.GifStatsMode));

                if (result) {
                    result = File.Exists(palettePath) &&
                        Run(Options.FFmpeg.FFmpegPath, string.Format("-y -i \"{0}\" -i \"{1}\" -lavfi \"paletteuse=dither={3}\" \"{2}\"", input, palettePath, output, Options.FFmpeg.GifDither));
                }
            } finally {
                if (File.Exists(palettePath)) {
                    File.Delete(palettePath);
                }
            }

            return result;
        }

        private bool Run(string path, string args = null) {
            int errorCode = Open(path, args);
            bool result = errorCode == 0;
            if (Options.FFmpeg.ShowError && !result) {
                MessageBoxResult yesno = MessageBox.Show("FFmpeg encountered an unexpected Error!\n\rDo you want to see the FFmpeg Log?",
                    "FFmpeg.exe error!", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (yesno == MessageBoxResult.Yes) {
                    string tmp = Path.GetTempFileName() + ".txt";
                    File.WriteAllText(tmp, Output.ToString());
                    Process.Start(tmp);
                }
            }
            return result;
        }

        public DirectShowDevices GetDirectShowDevices() {
            DirectShowDevices devices = new DirectShowDevices();

            if (File.Exists(Options.FFmpeg.FFmpegPath)) {
                const string arg = "-list_devices true -f dshow -i dummy";
                Open(Options.FFmpeg.FFmpegPath, arg);
                string output = Output.ToString();
                string[] lines = output.Split(new[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.None);
                bool isVideo = true;
                Regex regex = new Regex("\\[dshow @ \\w+\\]  \"(.+)\"", RegexOptions.Compiled | RegexOptions.CultureInvariant);
                foreach (string line in lines) {
                    if (line.EndsWith("] DirectShow video devices", StringComparison.InvariantCulture)) {
                        isVideo = true;
                        continue;
                    }

                    if (line.EndsWith("] DirectShow audio devices", StringComparison.InvariantCulture)) {
                        isVideo = false;
                        continue;
                    }

                    Match match = regex.Match(line);

                    if (match.Success) {
                        string value = match.Groups[1].Value;

                        if (isVideo) {
                            devices.VideoDevices.Add(value);
                        } else {
                            devices.AudioDevices.Add(value);
                        }
                    }
                }
            }

            return devices;
        }

        public override void Close() {
            if (ProcessRunning) {
                if (_closeTryCount >= 2) {
                    Process.Kill();
                } else {
                    WriteInput("q");
                    _closeTryCount++;
                }
            }
        }
    }

    public class DirectShowDevices {
        public List<string> VideoDevices = new List<string>();
        public List<string> AudioDevices = new List<string>();
    }
}
