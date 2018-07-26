using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IManip.Model;

namespace IManip.Core.Module
{
    public static class Harshness
    {

        private static double[,] kernel = new double[,]
                                         {{0.11, 0.11, 0.11},
                                          {0.11, 0.11, 0.11},
                                          {0.11, 0.11, 0.11}};

        public static Bitmap ApplyHarshness(this Bitmap input)
        {
            byte[] inputBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                input.Save(memoryStream, ImageFormat.Jpeg);
                inputBytes = memoryStream.ToArray();
            }

            int width = input.Width;
            int height = input.Height;

            int kernelWidth = kernel.GetLength(0);
            int kernelHeight = kernel.GetLength(1);

            ARGBModel[,] output = new ARGBModel[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double rSum = 0, gSum = 0, bSum = 0, kSum = 0;

                    output[x, y] = new ARGBModel();

                    for (int i = 0; i < kernelWidth; i++)
                    {
                        for (int j = 0; j < kernelHeight; j++)
                        {
                            int pixelPosX = x + (i - (kernelWidth / 2));
                            int pixelPosY = y + (j - (kernelHeight / 2));
                            if ((pixelPosX < 0) ||
                              (pixelPosX >= width) ||
                              (pixelPosY < 0) ||
                              (pixelPosY >= height)) continue;

                            byte r = input.GetPixel(x, y).R;
                            byte g = input.GetPixel(x, y).G;
                            byte b = input.GetPixel(x, y).B;

                            double kernelVal = kernel[i, j];

                            rSum += r * kernelVal;
                            gSum += g * kernelVal;
                            bSum += b * kernelVal;

                            kSum += kernelVal;

                        }

                    }

                    if (kSum <= 0) kSum = 1;

                    rSum /= kSum;
                    if (rSum < 0) rSum = 0;
                    if (rSum > 255) rSum = 255;

                    gSum /= kSum;
                    if (gSum < 0) gSum = 0;
                    if (gSum > 255) gSum = 255;

                    bSum /= kSum;
                    if (bSum < 0) bSum = 0;
                    if (bSum > 255) bSum = 255;

                    output[x, y].R = (byte)rSum;
                    output[x, y].G = (byte)gSum;
                    output[x, y].B = (byte)bSum;
                }
            }

                Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        result.SetPixel(i, j, Color.FromArgb(output[i, j].A,
                                                             output[i, j].R,
                                                             output[i, j].G,
                                                             output[i, j].B));
                    }
                }

                return result;
        }



    }
}


