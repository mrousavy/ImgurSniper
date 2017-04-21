using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgurSniper {
    internal class BitmapHelper {
        public static MemoryStream CompressImage(Bitmap bitmap, ImageFormat format, long compression) {
            if (bitmap == null) {
                throw new ArgumentNullException();
            }

            ImageCodecInfo codec = GetEncoder(format); 
            
            System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters parameters = new EncoderParameters(1);
            EncoderParameter parameter = new EncoderParameter(encoder, compression);
            parameters.Param[0] = parameter;

            MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, codec, parameters);
            return stream;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}
