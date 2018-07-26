using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IManip.Core.Module
{
    public static class Contrast
    {
        public static Bitmap ApplyContrast(this Bitmap currentBitmap)
        {
            Color c;
            float contrast = 4.0f;

            if (contrast < -100.0f)
                contrast = -100.0f;

            if (contrast > 100.0f)
                contrast = 100.0f;

            contrast = (100.0f + contrast) / 100.0f;
            contrast *= contrast;

            Bitmap temp = currentBitmap;
            Bitmap result = temp.Clone() as Bitmap;

            Rectangle rect = new Rectangle(0, 0, temp.Width, temp.Height);
            BitmapData bmpData = temp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < temp.Width; y++)
                {
                    var row = ptr + (y * bmpData.Stride);

                    for (int x = 0; x < temp.Height; x++)
                    {
                        var pixel = row + x * 4;

                        c = Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);

                        float pR = c.R / 255.0f;
                        pR -= 0.5f;
                        pR *= contrast;
                        pR += 0.5f;
                        pR *= 255;

                        if (pR < 0)
                            pR = 0;

                        if (pR > 255)
                            pR = 255;

                        float pG = c.G / 255.0f;
                        pG -= 0.5f;
                        pG *= contrast;
                        pG += 0.5f;
                        pG *= 255;

                        if (pG < 0)
                            pG = 0;

                        if (pG > 255)
                            pG = 255;

                        float pB = c.B / 255.0f;
                        pB -= 0.5f;
                        pB *= contrast;
                        pB += 0.5f;
                        pB *= 255;

                        if (pB < 0)
                            pB = 0;

                        if (pB > 255)
                            pB = 255;

                        result.SetPixel(y, x, Color.FromArgb((byte)pR, (byte)pG, (byte)pB));
                    }
                }
            }

            return result;
        }
    }
}
