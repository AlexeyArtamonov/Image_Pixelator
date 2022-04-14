using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Pixelate_Core
{
    public class BitmapWrapper : IDisposable
    {
        Bitmap bitmap;
        BitmapData data;
        byte[] RGBs;

        public Bitmap Bitmap { get { return bitmap; } }
        public byte[] Array { get { return RGBs; } }
        public BitmapWrapper(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

            IntPtr ptr = data.Scan0;
            int bytes = Math.Abs(data.Stride) * bitmap.Height;

            RGBs = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, RGBs, 0, bytes);
        }

        public void SetPixel(int x, int y, Color color)
        { 
            RGBs[(y * data.Width + x) * 4] = color.B;
            RGBs[(y * data.Width + x) * 4 + 1] = color.G;
            RGBs[(y * data.Width + x) * 4 + 2] = color.R;
            RGBs[(y * data.Width + x) * 4 + 3] = color.A;
        }
        public Color GetPixel(int x, int y)
        {
            return Color.FromArgb(RGBs[(y * data.Width + x) * 4 + 3], RGBs[(y * data.Width + x) * 4 + 2], RGBs[(y * data.Width + x) * 4 + 1], RGBs[(y * data.Width + x) * 4]);
        }
        public void Dispose()
        {
            System.Runtime.InteropServices.Marshal.Copy(RGBs, 0, data.Scan0, Math.Abs(data.Stride) * bitmap.Height);
            bitmap.UnlockBits(data);
        }
    }
}
