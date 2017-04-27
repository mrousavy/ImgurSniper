using ImgurSniper.Libraries.Helper;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImgurSniper.Libraries.Cache {

    public class HardDiskCache : ImageCache {
        public int Count => _indexList?.Count ?? 0;

        private readonly FileStream _fsCache;
        private readonly List<LocationInfo> _indexList;
        private readonly string _file;

        public HardDiskCache(string file) {
            _file = file;
            string directory = Path.GetDirectoryName(_file);

            if (directory == null)
                throw new System.ArgumentNullException(nameof(directory));

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _fsCache = new FileStream(_file, FileMode.Create, FileAccess.Write, FileShare.Read);
            _indexList = new List<LocationInfo>();
        }

        protected override void WriteFrame(Image img) {
            using (MemoryStream ms = new MemoryStream()) {
                img.Save(ms, ImageFormat.Bmp);
                long position = _fsCache.Position;
                ms.CopyStreamTo(_fsCache);
                _indexList.Add(new LocationInfo(position, _fsCache.Length - position));
            }
        }

        public override void Dispose() {
            _fsCache?.Dispose();

            base.Dispose();
        }

        public IEnumerable<Image> GetImageEnumerator() {
            if (!IsWorking && File.Exists(_file) && _indexList != null && _indexList.Count > 0) {
                using (FileStream fsCache = new FileStream(_file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    foreach (LocationInfo index in _indexList) {
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
