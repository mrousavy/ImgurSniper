using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
        private int _currentFrames, _totalFrames;
        private readonly IntPtr _desktop = NativeMethods.GetDesktopWindow();
        private readonly bool _showMouse = FileIO.ShowMouse;
        private List<Image> _images;

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

            if (_progressIndicatorEnabled) {
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
                _images = new List<Image>();

                #region Method 1: Timer

                _currentFrames = 0;
                _totalFrames = (int)(_fps * (_gifLength.TotalMilliseconds / 1000D));
                // ReSharper disable once PossibleLossOfFraction
                _timer = new Timer(1000 / _fps);

                if (_progressIndicatorEnabled)
                    ProgressBar.Maximum = _totalFrames;

                ThreadStart action = new ThreadStart(Frame);

                //Every Frame
                _timer.Elapsed += delegate {
                    new Thread(action).Start();
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


        private async void Frame() {
            try {
                //Finish GIF
                if (_stopped || _currentFrames >= _totalFrames) {
                    _timer.Stop();

                    await Dispatcher.BeginInvoke(new Action(ShowProgressBar));

                    await CreateGif(_images);

                    _timer.Dispose();
                    return;
                }

                try {
                    //Add Frames
                    _images.Add(Screenshot.GetScreenshotNative(_desktop, _size, _showMouse));
                } catch {
                    // frame skip
                } finally {
                    _currentFrames++;

                    if (_progressIndicatorEnabled)
                        await Dispatcher.BeginInvoke(new Action(delegate { ProgressBar.Value = _currentFrames; }));
                }
            } catch {
                await Dispatcher.BeginInvoke(new Action(delegate {
                    _timer.Stop();
                    FadeOut(false);
                }));
            }
        }


        private async Task CreateGif(List<Image> images) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            new Thread(() => {
                try {
                    using (MemoryStream stream = new MemoryStream()) {
                        //NGif vs GifBitmapEncoder: 
                        //  NGif is slower
                        //  GifBitmapEncoder is not made for creating GIFs

                        #region NGif
                        //AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
                        //gifEncoder.SetRepeat(0);
                        //gifEncoder.SetFrameRate(_fps);
                        //gifEncoder.Start(stream);

                        //foreach (Bitmap bitmap in bitmaps) {
                        //    using (Bitmap compressed = Image.FromStream(ImageHelper.CompressImage(bitmap, ImageFormat.Gif, 30)) as Bitmap) {
                        //        gifEncoder.AddFrame(compressed);
                        //    }
                        //    bitmap.Dispose();
                        //}

                        //gifEncoder.Finish();
                        #endregion

                        #region GifBitmapEncoder
                        GifBitmapEncoder encoder = new GifBitmapEncoder();

                        foreach (Image bitmap in images) {
                            MemoryStream compressed = ImageHelper.CompressImage(bitmap, ImageFormat.Gif, 30);
                            BitmapFrame frame = BitmapFrame.Create(
                                compressed,
                                BitmapCreateOptions.DelayCreation,
                                BitmapCacheOption.OnLoad);

                            encoder.Frames.Add(frame);
                        }

                        encoder.Save(stream);

                        //Clean unclosed Streams up
                        GC.Collect();
                        #endregion

                        Gif = stream.ToArray();

                        tcs.SetResult(true);
                    }
                } catch {
                    tcs.SetResult(false);
                }
            }).Start();

            await tcs.Task;
        }

        private void FinishGif(object sender, MouseButtonEventArgs e) {
            ShowProgressBar();
            _stopped = true;
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
    }
}