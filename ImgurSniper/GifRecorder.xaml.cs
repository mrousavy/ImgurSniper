using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
        private int _currentFrames, _totalFrames, _lastFrameTime;
        private readonly IntPtr _desktop = NativeMethods.GetDesktopWindow();
        private readonly bool _showMouse = FileIO.ShowMouse;
        //private MagickImageCollection _images;
        private List<Tuple<Image, int>> _images;
        private Stopwatch _stopwatch;

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
                //_images = new MagickImageCollection();
                _images = new List<Tuple<Image, int>>();

                #region Method 1: Timer

                _currentFrames = 0;
                _totalFrames = (int)(_fps * (_gifLength.TotalMilliseconds / 1000D));
                // ReSharper disable once PossibleLossOfFraction
                _timer = new Timer(1000 / _fps);

                if (_progressIndicatorEnabled)
                    ProgressBar.Maximum = _totalFrames;

                _stopwatch = new Stopwatch();
                ThreadStart action = Frame;

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

        //Capture 1 Screenshot, 1 Frame
        private async void Frame() {
            if (!_stopwatch.IsRunning)
                _stopwatch.Start();

            try {
                //Finish GIF
                if (_stopped || _currentFrames >= _totalFrames) {
                    _timer.Stop();

                    //Show Progressing Progress Indicator
                    await Dispatcher.BeginInvoke(new Action(ShowProgressBar));

                    //Finalize Gif
                    await CreateGif();

                    //Dispose the Timer and finish GIF Recording
                    _timer.Dispose();
                    return;
                }

                try {
                    //Add Frames

                    #region MagickImage
                    #region Compressed
                    //MemoryStream stream = ImageHelper.CompressImage(Screenshot.GetScreenshotNative(_desktop, _size, _showMouse), ImageFormat.Gif, 30);
                    //MagickImage image = new MagickImage(stream) {
                    //    AnimationDelay = 100 / FileIO.GifFps,
                    //    Quality = 30
                    //};
                    //_images.Add(image);
                    #endregion

                    #region Raw
                    //using (MemoryStream stream = new MemoryStream()) {
                    //    Screenshot.GetScreenshotNative(_desktop, _size, _showMouse).Save(stream, ImageFormat.Gif);
                    //    MagickImage image = new MagickImage(stream) {
                    //        AnimationDelay = 100 / _fps
                    //    };
                    //    _images.Add(image);
                    //}

                    #endregion
                    #endregion

                    #region Image
                    int delay;
                    int elapsed = (int)_stopwatch.ElapsedMilliseconds;

                    if (_lastFrameTime != 0)
                        delay = elapsed - _lastFrameTime;
                    else
                        delay = 1;
                    #region Compressed
                    //Error? Image is missing a frame
                    //MemoryStream stream =
                    //    ImageHelper.CompressImage(Screenshot.GetScreenshotNative(_desktop, _size, _showMouse),
                    //        ImageFormat.Gif, 30);
                    //Image img = Image.FromStream(stream);
                    //_images.Add(new Tuple<Image, int>(img, delay));
                    #endregion

                    #region Raw
                    _images.Add(new Tuple<Image, int>(Screenshot.GetScreenshotNative(_desktop, _size, _showMouse), delay));
                    #endregion
                    _lastFrameTime = elapsed;
                    #endregion

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


        private async Task CreateGif() {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            new Thread(() => {
                try {
                    //NGif vs GifBitmapEncoder vs Magick.NET vs GifWriter: 
                    //  NGif is slower
                    //  GifBitmapEncoder is not made for creating GIFs
                    //  Magick.NET is easier to use and especially made for creating GIFs
                    //  GifWriter has better handling for Frame Delay and minimal Code

                    #region NGif
                    //using (MemoryStream stream = new MemoryStream()) {
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

                    //Gif = stream.ToArray();
                    //}
                    #endregion

                    #region GifBitmapEncoder
                    //using (MemoryStream stream = new MemoryStream()) {
                    //GifBitmapEncoder encoder = new GifBitmapEncoder();

                    //foreach (Image bitmap in images) {
                    //    MemoryStream compressed = ImageHelper.CompressImage(bitmap, ImageFormat.Gif, 30);
                    //    BitmapFrame frame = BitmapFrame.Create(
                    //        compressed,
                    //        BitmapCreateOptions.DelayCreation,
                    //        BitmapCacheOption.OnLoad);

                    //    encoder.Frames.Add(frame);
                    //}

                    //encoder.Save(stream);

                    //Gif = stream.ToArray();
                    //}
                    #endregion

                    #region Magick.NET

                    //// Reduce colors
                    //QuantizeSettings settings = new QuantizeSettings() {
                    //    Colors = 128
                    //};
                    //_images.Quantize(settings);

                    //// Optimize GIF
                    //_images.OptimizePlus();

                    //// "Save" GIF
                    //Gif = _images.ToByteArray();

                    //// Dispose GIF
                    //_images.Clear();
                    //_images.Dispose();
                    #endregion

                    #region GifWriter
                    using (MemoryStream stream = new MemoryStream()) {
                        using (GifWriter writer = new GifWriter(stream, 1000 / _fps)) {
                            foreach (Tuple<Image, int> tuple in _images)
                                writer.WriteFrame(tuple.Item1, tuple.Item2);
                            Gif = stream.ToArray();
                        }
                    }
                    #endregion

                    //Cleanup
                    GC.Collect();

                    tcs.SetResult(true);
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


        //IDisposable
        public void Dispose() {
            Gif = null;

            _images?.Clear();
            //_images?.Dispose();

            _timer?.Dispose();
            _timer = null;

            try {
                Close();
            } catch {
                //Window already closed
            }

            GC.Collect();
        }

    }
}