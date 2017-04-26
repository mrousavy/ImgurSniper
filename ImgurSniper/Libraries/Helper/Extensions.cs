using ImgurSniper.Libraries.GIF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ImgurSniper.Libraries.Helper {
    public static class Extensions {
        public const int BufferSize = 4096;

        public static void SaveGIF(this Image img, Stream stream, GIFQuality quality) {
            if (quality == GIFQuality.Default) {
                img.Save(stream, ImageFormat.Gif);
            } else {
                Quantizer quantizer;
                switch (quality) {
                    case GIFQuality.Grayscale:
                        quantizer = new GrayscaleQuantizer();
                        break;
                    case GIFQuality.Bit4:
                        quantizer = new OctreeQuantizer(15, 4);
                        break;
                    default:
                    case GIFQuality.Bit8:
                        quantizer = new OctreeQuantizer(255, 4);
                        break;
                }

                using (Bitmap quantized = quantizer.Quantize(img)) {
                    quantized.Save(stream, ImageFormat.Gif);
                }
            }
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (T item in source) {
                action(item);
            }
        }

        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison) {
            if (string.IsNullOrEmpty(oldValue)) {
                return str;
            }

            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1) {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        #region Stream
        public static void Write(this FileStream stream, byte[] array) {
            stream.Write(array, 0, array.Length);
        }

        public static void CopyStreamTo(this Stream fromStream, Stream toStream, int bufferSize = BufferSize) {
            if (fromStream.CanSeek) {
                fromStream.Position = 0;
            }

            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = fromStream.Read(buffer, 0, buffer.Length)) > 0) {
                toStream.Write(buffer, 0, bytesRead);
            }
        }

        public static int CopyStreamTo64(this FileStream fromStream, Stream toStream, long offset, int length, int bufferSize = BufferSize) {
            fromStream.Position = offset;

            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            int totalBytesRead = 0;
            int positionLimit = length - bufferSize;
            int readLength = bufferSize;

            do {
                if (totalBytesRead > positionLimit) {
                    readLength = length - totalBytesRead;
                }

                bytesRead = fromStream.Read(buffer, 0, readLength);
                toStream.Write(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
            }
            while (bytesRead > 0 && totalBytesRead < length);

            return totalBytesRead;
        }
        #endregion
    }
}