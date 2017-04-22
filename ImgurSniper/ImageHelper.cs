using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImgurSniper {
    internal class ImageHelper {
        public static MemoryStream CompressImage(Image image, ImageFormat format, byte compression) {
            if (image == null) {
                throw new ArgumentNullException();
            }

            ImageCodecInfo codec = GetEncoder(format);

            Encoder encoder = Encoder.Quality;
            EncoderParameters parameters = new EncoderParameters(1);
            EncoderParameter parameter = new EncoderParameter(encoder, (long)compression);
            parameters.Param[0] = parameter;

            MemoryStream stream = new MemoryStream();
            image.Save(stream, codec, parameters);
            return stream;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}
