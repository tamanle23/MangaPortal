using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using ProjectX.Common.Extensions;

namespace BlueWind.Crawler.Core
{
    public static class Utility
    {
        public static string RemoveInvalidChars(this string str)
        {
            string invalidChars = "^/:*?<>\"\\|']*";
            var builder = new StringBuilder(str);
            foreach (char ch in invalidChars.ToArray())
            {
                builder.Replace(ch, ' ');
            }
            return builder.ToString();
        }
        public static byte[] GetThumbnail(this Stream stream, int width, int height)
        {
            Image image = new Bitmap(width, height);
            System.Drawing.Graphics.FromImage(image).DrawImage(Bitmap.FromStream(stream), 0, 0, width, height);
            Stream buffer = new MemoryStream();
            image.Save(buffer, ImageFormat.Jpeg);
            return buffer.ReadToEnd();
        }
        public static byte[] GetThumbnail(this byte[] buffer, int width, int height)
        {
            Image sourceImage=Bitmap.FromStream(new MemoryStream(buffer));
            Image image = new Bitmap(width, height);
            var graphic = System.Drawing.Graphics.FromImage(image);
                graphic.DrawImage(sourceImage, 0, 0, width, height);
            Stream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            return stream.ReadToEnd();
        }
        public static IEnumerable<byte[]> GetThumbnail(this byte[] buffer,IEnumerable<int[]> sizes)
        {
            Image sourceImage = Bitmap.FromStream(new MemoryStream(buffer));
            Image image;
            Graphics graphic;
            Stream stream ;
            foreach (var size in sizes)
            {
                if (size.Length > 1)
                {
                    image = new Bitmap(size[0], size[1]);
                    graphic = System.Drawing.Graphics.FromImage(image);
                    graphic.DrawImage(sourceImage, 0, 0, size[0], size[1]);
                    stream = new MemoryStream();
                    image.Save(stream, ImageFormat.Jpeg);
                    yield return stream.ReadToEnd();
                }
            }
            
        }
    }
   public enum WriteFileCode : byte
   {
       Success = 255,
       Fail=0
   }
}
