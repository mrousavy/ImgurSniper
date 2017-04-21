﻿using Gif.Components;
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
                List<Bitmap> bitmaps = new List<Bitmap>();
                bool showMouse = FileIO.ShowMouse;

                #region Method 1: Timer

                int currentFrames = 0;
                int totalFrames = (int)(_fps * (_gifLength.TotalMilliseconds / 1000D));
                MemoryStream gifStream = new MemoryStream();
                // ReSharper disable once PossibleLossOfFraction
                _timer = new Timer(1000 / _fps);

                if (_progressIndicatorEnabled)
                    ProgressBar.Maximum = totalFrames;

                //Every Frame
                _timer.Elapsed += delegate {
                    new Thread(async () => {
                        try {
                            //Finish GIF
                            if (_stopped || currentFrames >= totalFrames) {
                                _timer.Stop();

                                await Dispatcher.BeginInvoke(new Action(ShowProgressBar));

                                await CreateGif(bitmaps);

                                _timer.Dispose();
                                return;
                            }

                            try {
                                //Add Frames
                                bitmaps.Add(showMouse
                                    ? Screenshot.GetScreenshotWithMouse(_size)
                                    : Screenshot.GetScreenshot(_size));
                            } catch {
                                // frame skip

                                // Add last frame as current in case of frame skip
                                bitmaps.Add(bitmaps[bitmaps.Count - 1]);
                            } finally {
                                currentFrames++;

                                if (_progressIndicatorEnabled)
                                    await Dispatcher.BeginInvoke(new Action(delegate { ProgressBar.Value = currentFrames; }));
                            }
                        } catch {
                            await Dispatcher.BeginInvoke(new Action(delegate {
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


        private async Task CreateGif(List<Bitmap> bitmaps) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            new Thread(() => {
                try {
                    using (MemoryStream stream = new MemoryStream()) {
                        AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
                        gifEncoder.SetRepeat(0);
                        gifEncoder.SetFrameRate(_fps);
                        gifEncoder.Start(stream);

                        foreach (Bitmap bitmap in bitmaps) {
                            using (Bitmap compressed = Image.FromStream(ImageHelper.CompressImage(bitmap, ImageFormat.Gif, 30)) as Bitmap) {
                                gifEncoder.AddFrame(compressed);
                            }
                            bitmap.Dispose();
                        }

                        gifEncoder.Finish();

                        GifBitmapEncoder encoder = new GifBitmapEncoder();
                        //foreach (Bitmap bitmap in bitmaps) {
                        //    using (MemoryStream compressedBitmap =
                        //        BitmapHelper.CompressImage(bitmap, ImageFormat.Gif, 90)) {

                        //        BitmapFrame frame = BitmapFrame.Create(
                        //            compressedBitmap,
                        //            BitmapCreateOptions.DelayCreation,
                        //            BitmapCacheOption.OnLoad);

                        //        encoder.Frames.Add(frame);
                        //    }
                        //    bitmap.Dispose();
                        //}

                        //encoder.Save(stream);


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