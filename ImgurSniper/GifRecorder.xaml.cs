using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Timer = System.Timers.Timer;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for GifRecorder.xaml
    /// </summary>
    public partial class GifRecorder : IDisposable {
        private readonly int _fps = FileIO.GifFps;
        private readonly Rectangle _size;
        private TimeSpan _gifLength;
        private bool _stopped;
        private readonly bool _progressIndicatorEnabled;
        private Timer _timer;

        public byte[] Gif;

        public GifRecorder(Rectangle size, TimeSpan gifLength) {
            InitializeComponent();

            _size = size;
            _gifLength = gifLength;

            Left = size.Left - 2;
            Top = size.Top - 2;
            Width = size.Width + 4;
            Height = size.Height + 4;

            Outline.Width = Width;
            Outline.Height = Height;

            int progressBarWidth = size.Width - 40;
            _progressIndicatorEnabled = progressBarWidth > 0;

            if(_progressIndicatorEnabled) {
                ProgressBar.Width = progressBarWidth;
            } else {
                ProgressBar.Visibility = Visibility.Collapsed;
                DoneButton.Visibility = Visibility.Collapsed;
            }

            //Space for ProgressBar
            Height += 30;

            Loaded += delegate {
                BeginAnimation(OpacityProperty, Animations.FadeIn);
                Record();
            };
        }


        public void Dispose() {
            Gif = null;

            try {
                Close();
            } catch {
                //Window already closed
            }

            GC.Collect();
        }


        private void FadeOut(bool result) {
            DoubleAnimation fadeOut = Animations.FadeOut;
            fadeOut.Completed += delegate {
                try {
                    DialogResult = result;
                } catch {
                    Close();
                }
            };
            BeginAnimation(OpacityProperty, fadeOut);
        }

        private void Record() {
            try {
                //Each Frame with TimeStamp
                List<BitmapFrame> bitmapframes = new List<BitmapFrame>();

                #region Method 1: Timer

                int currentFrames = 0;
                int totalFrames = (int)(_fps * (_gifLength.TotalMilliseconds / 1000D));
                MemoryStream stream;
                MemoryStream gifStream = new MemoryStream();
                // ReSharper disable once PossibleLossOfFraction
                _timer = new Timer(1000 / _fps);

                if(_progressIndicatorEnabled)
                    ProgressBar.Maximum = totalFrames;

                //Every Frame
                _timer.Elapsed += delegate {
                    new Thread(() => {
                        try {
                            //Finish GIF
                            if(_stopped || currentFrames >= totalFrames) {
                                _timer.Stop();

                                GifBitmapEncoder encoder = new GifBitmapEncoder();
                                foreach(BitmapFrame frame in bitmapframes) {
                                    encoder.Frames.Add(frame);
                                }

                                encoder.Save(gifStream);

                                Gif = gifStream.ToArray();

                                _timer.Dispose();
                                return;
                            }

                            //Add Frames
                            stream = new MemoryStream();

                            using(Bitmap tmp = Screenshot.GetScreenshotWithMouse(_size)) {
                                tmp.Save(stream, ImageFormat.Gif);
                            }

                            BitmapFrame bitmap = BitmapFrame.Create(
                                stream,
                                BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.OnLoad);

                            bitmapframes.Add(bitmap);

                            currentFrames++;

                            if(_progressIndicatorEnabled)
                                Dispatcher.BeginInvoke(new Action(delegate { ProgressBar.Value = currentFrames; }));
                        } catch {
                            Dispatcher.BeginInvoke(new Action(delegate {
                                _timer.Stop();
                                FadeOut(false);
                            }));
                        }
                    }).Start();
                };

                _timer.Disposed += delegate { Dispatcher.BeginInvoke(new Action(delegate { FadeOut(true); })); };

                _timer.Start();

                #endregion

                #region Method 2: Async ForLoop

                //for(int i = 0; i < 1000; i++) {
                //    MemoryStream stream = new MemoryStream();

                //    Screenshot.MediaImageToDrawingImage(Screenshot.getScreenshot(size))
                //        .Save(stream, ImageFormat.Gif);

                //    BitmapFrame bitmap = BitmapFrame.Create(
                //        stream,
                //        BitmapCreateOptions.PreservePixelFormat,
                //        BitmapCacheOption.OnLoad);

                //    encoder.Frames.Add(bitmap);
                //}

                #endregion

                #region Method 3: Expression Encoder .WMV

                //Path for temporary WMV
                //string tmp = Path.Combine(
                //    @"C:\Users\b4dpi\Documents\ImgurSniper",
                //    "imgursnipertempvid.wav");

                //if(File.Exists(tmp))
                //    File.Delete(tmp);

                ////Initialize Capture job
                //ScreenCaptureJob screenCaptureJob = new ScreenCaptureJob {
                //    CaptureRectangle = size,
                //    ShowFlashingBoundary = true,
                //    ScreenCaptureVideoProfile = { FrameRate = FileIO.GifFps },
                //    CaptureMouseCursor = true,
                //    OutputScreenCaptureFileName = tmp,
                //    Duration = duration
                //};
                //screenCaptureJob.ShowFlashingBoundary = false;

                ////Record
                //screenCaptureJob.Start();

                //screenCaptureJob.ScreenCaptureFinished += delegate {
                //    GifBitmapEncoder encoder = new GifBitmapEncoder();

                //    Close();
                //};

                #endregion
            } catch {
                FadeOut(false);
            }
        }

        private void FinishGif(object sender, MouseButtonEventArgs e) {
            _stopped = true;
        }
    }
}