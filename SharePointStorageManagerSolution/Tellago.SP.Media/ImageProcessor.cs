using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Tellago.SP.Media
{
    public class ImageProcessor
    {
        private const int MaxThumbWidth = 120;
        private const int MaxThumbHeight = 80;

        public bool ThumbnailCallback()
        {
            return false;
        }
        
       public void GenerateThumbnail(Stream imgFileStream, Stream thumbStream)
       {
            Image.GetThumbnailImageAbort callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
            Bitmap bitmap = new Bitmap(imgFileStream);
            int thumbWidth = MaxThumbWidth;
            int thumbHeight = MaxThumbHeight;
            if (bitmap.Width > bitmap.Height)
            {
                thumbHeight = Decimal.ToInt32(((Decimal)bitmap.Height / bitmap.Width) * thumbWidth);
                if (thumbHeight > MaxThumbHeight)
                {
                    thumbHeight = MaxThumbHeight;
                }
            }
            else
            {
                thumbWidth = Decimal.ToInt32(((Decimal)bitmap.Width / bitmap.Height) * thumbHeight);
                if (thumbWidth > MaxThumbWidth)
                {
                    thumbWidth = MaxThumbWidth;
                }
            }
            
            Image thumbnail = bitmap.GetThumbnailImage(thumbWidth, thumbHeight, callback, IntPtr.Zero);

            thumbnail.Save(thumbStream,ImageFormat.Jpeg);
        }


    }
}
