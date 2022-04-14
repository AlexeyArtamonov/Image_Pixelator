using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Pixelate_Core
{
    public class Pixelator
    {
        static List<Color> ColorPallete;
        /// <summary>
        /// Pixalates given Bitmap
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="blockSize">Sample Factor, how many pixels colors transforms into one color</param>
        /// <param name="colors">Amount of color in pallete, -1 for using all colors in Bitmap</param>
        /// <param name="randomColors">Color selection mode, random or most used</param>
        /// <returns>Pixelated Bitmap</returns>
        public static Bitmap Pixelate(Bitmap bitmap, IEnumerable<Color> Pallete, int blockSize = 4, int colors = -1, bool randomColors = true)
        {
            if (Pallete != null)
                ColorPallete = new List<Color>(Pallete);
            else
                ColorPallete = new List<Color>();

            if (colors != -1)
                GeneratePallete(new BitmapWrapper(bitmap), colors, randomColors);
            else
                ColorPallete = null;
                

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                for (int i = 0; i < bitmap.Width; i += blockSize)
                {
                    for (int j = 0; j < bitmap.Height; j += blockSize)
                    {
                        Color avrg = AverageColor(bitmap, i, j, blockSize);
                        graphics.FillRectangle(new SolidBrush(avrg), new Rectangle(i, j, blockSize, blockSize));
                    }
                }
            }
            return bitmap;
        }
        
        public static Bitmap Pixelate(Bitmap bitmap, int blockSize = 4, int colors = -1, bool randomColors = true)
        {
            return Pixelate(bitmap, null, blockSize, colors);
        }
        static Color AverageColor(Bitmap bitmap, int x, int y, int size)
        {
            Bitmap bmp = new Bitmap(1, 1);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bitmap, new Rectangle(0, 0, 1, 1), new Rectangle(x, y, size, size), GraphicsUnit.Pixel);
            }
            Color pixel = bmp.GetPixel(0, 0);
            Pixelation_.ImageHelper.DeleteObject(bmp.GetHbitmap());
            // if ColorPallete == null, then we use all color mode
            if (ColorPallete != null)
                pixel = FindNearest(pixel);
            return pixel;
        }

        /// <summary>
        /// Generates color pallete
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="size"></param>
        /// <param name="randomColors">Mode: random or most used</param>
        static void GeneratePallete(BitmapWrapper bitmap, int size, bool randomColors)
        {
            if (randomColors)
            {
                Random random = new Random((int)DateTime.Now.Ticks);
                ColorPallete.Add(Color.Black);
                ColorPallete.Add(Color.White);
                while(ColorPallete.Count < size + 2)
                {
                    ColorPallete.Add
                    (
                        bitmap.GetPixel(random.Next(0, bitmap.Bitmap.Width), random.Next(0, bitmap.Bitmap.Height))
                    );
                }
            }
            else
            {
                Dictionary<Color, int> colors = new Dictionary<Color, int>();
                Color color;
                for (int i = 0; i < bitmap.Bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Bitmap.Height; j++)
                    {
                        color = bitmap.GetPixel(i, j);
                        if (colors.ContainsKey(color))
                        {
                            colors[color]++;
                        }
                        else
                            colors.Add(color, 1);
                    }
                }
                List<Color> avrgColors = colors.OrderByDescending(item => item.Value).Select(item => item.Key).ToList();
                ColorPallete.Add(avrgColors[0]);
                const int  step = 30;
                for (int i = 1; i < colors.Count; i++)
                {
                    if (Math.Abs(RGBSum(avrgColors[i]) - RGBSum(ColorPallete.Last())) > step)
                        ColorPallete.Add(avrgColors[i]);
                }
                ColorPallete = ColorPallete.Take(size).ToList();
                colors = null;

            }
            bitmap.Dispose();
        }
        static Color FindNearest(Color color)
        {
            int difference = 1000;
            int rgb_color = color.R + color.G + color.B;

            Color nearest = color;

            int temp;
            foreach(var item in ColorPallete)
            {
                temp = item.R + item.G + item.B;
                if (Math.Abs(temp - rgb_color) < difference)
                {
                    difference = Math.Abs(temp - rgb_color);
                    nearest = item;
                }
            }
            return nearest;
        }
        public static int RGBSum(Color color)
        {
            return color.R + color.G + color.B;
        }
    }
}
