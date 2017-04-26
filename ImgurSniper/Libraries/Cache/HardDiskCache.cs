using ImgurSniper.Libraries.Helper;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImgurSniper.Libraries.Cache {

    public class HardDiskCache : ImageCache {
        public int Count {
            get {
                if (indexList != null) {
                    return indexList.Count;
                }

                return 0;
            }
        }

        private FileStream fsCache;
        private List<LocationInfo> indexList;
        private string _file;
        private string _directory;

        public HardDiskCache(string file) {
            _file = file;
            _directory = Path.GetDirectoryName(_file);

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            fsCache = new FileStream(_file, FileMode.Create, FileAccess.Write, FileShare.Read);
            indexList = new List<LocationInfo>();
        }

        protected override void WriteFrame(Image img) {
            using (MemoryStream ms = new MemoryStream()) {
                img.Save(ms, ImageFormat.Bmp);
                long position = fsCache.Position;
                ms.CopyStreamTo(fsCache);
                indexList.Add(new LocationInfo(position, fsCache.Length - position));
            }
        }

        public override void Dispose() {
            if (fsCache != null) {
                fsCache.Dispose();
            }

            base.Dispose();
        }

        public IEnumerable<Image> GetImageEnumerator() {
            if (!IsWorking && File.Exists(_file) && indexList != null && indexList.Count > 0) {
                using (FileStream fsCache = new FileStream(_file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    foreach (LocationInfo index in indexList) {
                        using (MemoryStream ms = new MemoryStream()) {
                            fsCache.CopyStreamTo64(ms, index.Location, (int)index.Length);
                            yield return Image.FromStream(ms);
                        }
                    }
                }
            }
        }

        public struct LocationInfo {
            public long Location { get; set; }
            public long Length { get; set; }

            public LocationInfo(long location, long length) {
                Location = location;
                Length = length;
            }
        }
    }
}
