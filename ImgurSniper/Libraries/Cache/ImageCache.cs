using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;

namespace ImgurSniper.Libraries.Cache {

    public abstract class ImageCache : IDisposable {
        public bool IsWorking { get; protected set; }

        protected Thread Task;
        protected BlockingCollection<Image> ImageQueue;

        protected ImageCache() {
            ImageQueue = new BlockingCollection<Image>();
        }

        public void AddImageAsync(Image img) {
            if (!IsWorking) {
                StartConsumerThread();
            }

            ImageQueue.Add(img);
        }

        protected virtual void StartConsumerThread() {
            if (!IsWorking) {
                IsWorking = true;

                Task = new Thread(() => {
                    try {
                        while (!ImageQueue.IsCompleted) {
                            Image img = null;

                            try {
                                img = ImageQueue.Take();

                                if (img != null) {
                                    //using (new DebugTimer("WriteFrame"))
                                    WriteFrame(img);
                                }
                            } catch (InvalidOperationException) {
                            } finally {
                                img?.Dispose();
                            }
                        }
                    } finally {
                        IsWorking = false;
                    }
                });

                Task.Start();
            }
        }

        protected abstract void WriteFrame(Image img);

        public void Finish() {
            if (IsWorking) {
                ImageQueue.CompleteAdding();
                Task.Join();
            }

            Dispose();
        }

        public virtual void Dispose() {
            ImageQueue?.Dispose();
        }
    }
}
