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
        public enum ScaleMode
        {
            NearestNeighbor     =   InterpolationMode.NearestNeighbor,
            Low                 =   InterpolationMode.Low,
            High                =   InterpolationMode.High,
            Bicubic             =   InterpolationMode.Bicubic,
            Bilinear            =   InterpolationMode.Bilinear,
            HighQualityBicubic  =   InterpolationMode.HighQualityBicubic,
            HighQualityBilinear =   InterpolationMode.HighQualityBilinear

        }
        public enum ColorMode
        {
            AllColors,
            MostUsed,
            Random
        }
        static List<Color> ColorPallete;
        public static Bitmap Pixelate(Bitmap bitmap, IEnumerable<Color> Pallete, int blockSize, int colorsAmount, ScaleMode scaleMode, ColorMode colorMode)
        {
            GeneratePallete(bitmap, Pallete, colorsAmount, colorMode);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                for (int i = 0; i < bitmap.Width; i += blockSize)
                {
                    for (int j = 0; j < bitmap.Height; j += blockSize)
                    {
                        Color avrg = AverageColor(bitmap, i, j, blockSize, scaleMode);
                        graphics.FillRectangle(new SolidBrush(avrg), new Rectangle(i, j, blockSize, blockSize));
                    }
                }
            }
            return bitmap;
        }
        public static async Task<Bitmap> PixelateAsync(Bitmap bitmap, IEnumerable<Color> Pallete, int blockSize, int colorsAmount, ScaleMode scaleMode, ColorMode colorMode)
        {
            var task = Task.Run(() => Pixelate(bitmap, Pallete, blockSize, colorsAmount, scaleMode, colorMode));
            await task;
            return task.Result;
        }

        static Bitmap bmp = new Bitmap(1, 1);
        static Color AverageColor(Bitmap bitmap, int x, int y, int size, ScaleMode scaleMode)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = (InterpolationMode)scaleMode;
                g.DrawImage(bitmap, new Rectangle(0, 0, 1, 1), new Rectangle(x, y, size, size), GraphicsUnit.Pixel);
            }
            Color pixel = bmp.GetPixel(0, 0);
            Pixelation_.ImageHelper.DeleteObject(bmp.GetHbitmap());

            if (ColorPallete != null)
                pixel = FindNearest(pixel);
            return pixel;
        }
        static void GeneratePallete(Bitmap bitmap, IEnumerable<Color> Pallete, int colorsAmount, ColorMode colorMode)
        {
            ColorPallete = new List<Color>();
            if (Pallete != null)
                ColorPallete.AddRange(Pallete);

            switch (colorMode)
            {
                case ColorMode.Random:
                {
                    Random random = new Random((int)DateTime.Now.Ticks);

                    ColorPallete.Add(Color.Black);
                    ColorPallete.Add(Color.White);

                    BitmapWrapper wrapper = new BitmapWrapper(bitmap);
                    for (int i = 0; i < colorsAmount; i++)
                    {
                        ColorPallete.Add
                        (
                            wrapper.GetPixel(random.Next(0, wrapper.Bitmap.Width), random.Next(0, wrapper.Bitmap.Height))
                        );
                    }
                    wrapper.Dispose();
                    break;
                }

                case ColorMode.MostUsed:
                {
                    BitmapWrapper wrapper = new BitmapWrapper(bitmap);

                    Dictionary<Color, int> colors = new Dictionary<Color, int>();

                    Color color;
                    for (int i = 0; i < wrapper.Bitmap.Width; i++)
                    {
                        for (int j = 0; j < wrapper.Bitmap.Height; j++)
                        {
                            color = wrapper.GetPixel(i, j);
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

                    ColorPallete = ColorPallete.Take(colorsAmount).ToList();
                    colors = null;
                    wrapper.Dispose();
                    break;
                }

                case ColorMode.AllColors:
                {
                    ColorPallete = null;
                    break;
                }
            }
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
