using ImgurSniper.Libraries.FFmpeg;
using ImgurSniper.Libraries.Helper;
using System;

namespace ImgurSniper.Libraries.ScreenCapture {
    public class ScreenRecorder : IDisposable {

        public bool IsRecording { get; private set; }

        public string CachePath { get; }

        public ScreencastOptions Options { get; set; }

        public event Action RecordingStarted;

        private readonly FFmpegHelper _ffmpegCli;

        public ScreenRecorder(ScreencastOptions options) {
            if (string.IsNullOrEmpty(options.OutputPath)) {
                throw new Exception("Screen recorder cache path is empty.");
            }

            CachePath = options.OutputPath;

            Options = options;

            _ffmpegCli = new FFmpegHelper(Options);
            _ffmpegCli.RecordingStarted += OnRecordingStarted;
        }

        public void StartRecording() {
            if (!IsRecording) {
                IsRecording = true;

                _ffmpegCli.Record();
            }

            IsRecording = false;
        }

        public void StopRecording() {
            _ffmpegCli?.Close();
        }

        public bool FFmpegEncodeAsGif(string path) {
            Helpers.CreateDirectoryFromFilePath(path);
            return _ffmpegCli.EncodeGif(Options.OutputPath, path);
        }

        protected void OnRecordingStarted() {
            RecordingStarted?.Invoke();
        }

        public void Dispose() {
            _ffmpegCli?.Close();
            _ffmpegCli?.Dispose();
        }
    }
}