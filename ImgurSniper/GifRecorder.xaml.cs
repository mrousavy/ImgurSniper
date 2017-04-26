using ImgurSniper.Libraries.FFmpeg;
using ImgurSniper.Libraries.Helper;
using ImgurSniper.Libraries.ScreenCapture;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for GifRecorder.xaml
    /// </summary>
    public partial class GifRecorder : IDisposable {
        private readonly Rectangle _size;
        private readonly bool _progressIndicatorEnabled;
        private ScreenRecorder _recorder;
        private string _outputMp4, _outputGif;
        private System.Timers.Timer _progressTimer;
        private int _elapsed = 0;
        private CancellationTokenSource cts;
        private bool _stopRequested = false;

        public byte[] Gif;

        public GifRecorder(Rectangle size) {
            InitializeComponent();

            _size = size;

            Left = size.Left - 2;
            Top = size.Top - 2;
            Width = size.Width + 4;
            Height = size.Height + 4;

            Outline.Width = Width;
            Outline.Height = Height;

            int progressBarWidth = size.Width - 40;
            _progressIndicatorEnabled = progressBarWidth > 0;

            if (_progressIndicatorEnabled) {
                ProgressBar.Width = progressBarWidth;
            } else {
                ProgressBar.Visibility = Visibility.Collapsed;
                DoneButton.Visibility = Visibility.Collapsed;
            }

            ProgressBar.Maximum = FileIO.GifLength / 50;
            cts = new CancellationTokenSource();

            //Space for ProgressBar
            Height += 30;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            BeginAnimation(OpacityProperty, Animations.FadeIn);
            StartRecording();
        }

        private void FadeOut(bool result) {
            Dispatcher.Invoke(() => {
                DoubleAnimation fadeOut = Animations.FadeOut;
                fadeOut.Completed += delegate {
                    try {
                        DialogResult = result;
                    } catch {
                        Close();
                    }
                };
                BeginAnimation(OpacityProperty, fadeOut);
            });
        }

        private void ShowProgressBar() {
            DoubleAnimation fadeOut = Animations.FadeOut;
            fadeOut.Completed += delegate {
                OkButton.Visibility = Visibility.Collapsed;

                CircularProgressBar.Visibility = Visibility.Visible;
                CircularProgressBar.BeginAnimation(OpacityProperty, Animations.FadeIn);
            };
            OkButton.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void UpdateProgress(object sender, System.Timers.ElapsedEventArgs e) {
            try {
                Dispatcher.Invoke(() => {
                    if (_recorder == null || !_recorder.IsRecording) {
                        _progressTimer.Dispose();
                        return;
                    }

                    ProgressBar.Value = _elapsed++;
                });
            } catch {
                // cannot use dispatcher on this window anymore
            }
        }

        private async void StopGifClick(object sender, MouseButtonEventArgs e) {
            _stopRequested = true;
            cts.Cancel();

            await StopRecording();
        }

        //Start Recording Video
        private void StartRecording() {
            new Thread(() => {
                try {
                    _outputMp4 = Path.Combine(Path.GetTempPath(), "screencapture.mp4");
                    if (File.Exists(_outputMp4))
                        File.Delete(_outputMp4);
                    _outputGif = Path.Combine(Path.GetTempPath(), "screencapture.gif");
                    if (File.Exists(_outputGif))
                        File.Delete(_outputGif);

                    Screenshot screenshot = new Screenshot() {
                        CaptureCursor = true,
                        RemoveOutsideScreenArea = true,
                        CaptureShadow = true,
                        CaptureClientArea = false,
                        AutoHideTaskbar = false,
                        ShadowOffset = 20
                    };

                    string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ffmpeg.exe");

                    FFmpegOptions ffmpeg = new FFmpegOptions(ffmpegPath) {
                        VideoCodec = FFmpegVideoCodec.gif
                    };

                    ScreencastOptions options = new ScreencastOptions {
                        CaptureArea = _size,
                        DrawCursor = true,
                        GIFFPS = FileIO.GifFps,
                        ScreenRecordFPS = 30,
                        OutputPath = _outputMp4,
                        FFmpeg = ffmpeg,
                        Duration = FileIO.GifLength / 1000f
                    };
                    _recorder = new ScreenRecorder(options, screenshot, _size);

                    //If Progressbar is enabled
                    if (_progressIndicatorEnabled) {
                        _recorder.RecordingStarted += delegate {
                            //Update Progress
                            _progressTimer = new System.Timers.Timer {
                                Interval = 50
                            };
                            _progressTimer.Elapsed += UpdateProgress;
                            _progressTimer.Start();
                        };
                    }

                    _recorder.StartRecording();
                } catch {
                    FadeOut(false);
                }

                if (!_stopRequested)
                    Dispatcher.Invoke(StopRecording);
            }).Start();
        }

        //Stop recording Video, begin encoding as GIF and Save
        private async Task StopRecording() {
            ShowProgressBar();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            new Thread(() => {
                try {
                    _recorder.StopRecording();

                    //MP4 -> GIF
                    _recorder.FFmpegEncodeAsGIF(_outputGif);

                    Gif = File.ReadAllBytes(_outputGif);

                    _recorder.Dispose();
                    _recorder = null;

                    tcs.SetResult(true);
                } catch {
                    tcs.SetResult(false);
                }
            }).Start();

            await tcs.Task;

            MakeGif();
        }

        private void MakeGif() {
            if (_recorder.IsRecording || !File.Exists(_outputGif))
                FadeOut(false);

            try {
                Gif = File.ReadAllBytes(_outputGif);

                FadeOut(true);
            } catch {
                FadeOut(false);
            }
        }


        //IDisposable
        public void Dispose() {
            Gif = null;

            try {
                if (File.Exists(_outputGif)) {
                    File.Delete(_outputGif);
                }
                if (File.Exists(_outputMp4)) {
                    File.Delete(_outputMp4);
                }
            } catch {
                // could not delete
            }


            try {
                if (_recorder != null && _recorder.IsRecording)
                    _recorder.StopRecording();
            } catch {
                // unexpected error on stop recording
            }

            _recorder?.Dispose();
            _recorder = null;

            try {
                Close();
            } catch {
                //Window already closed
            }

            GC.Collect();
        }

    }
}