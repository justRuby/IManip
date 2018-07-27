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
        private static double[,] filter1 = new double[,]
                                         {{0, -1, 0},
                                          {-1, 5, -1},
                                          {0, -1, 0}};

        private static double[,] filter2 = new double[,]
                                         {{0, -1, -1, -1, 0},
                                          {-1, -1, 2, -1, -1},
                                          {-1, 2, 6, 2, -1},
                                          {-1, -1, 2, -1, -1},
                                          {0, -1, -1, -1, 0 } };

        public static Bitmap ApplySharpen(this Bitmap content)
        {
            Bitmap sharpenImage = new Bitmap(content);

            int filterWidth = 3;
            int filterHeight = 3;
            int w = content.Width;
            int h = content.Height;

            double factor = 1;
            double bias = 3.16;

            Color[,] result = new Color[content.Width, content.Height];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    Color imageColor = content.GetPixel(x, y);

                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int imageX = (x - filterWidth / 2 + filterX + w) % w;
                            int imageY = (y - filterHeight / 2 + filterY + h) % h;

                            imageColor = content.GetPixel(imageX, imageY);


                            if (imageColor.R >= 155 &&
                                imageColor.G >= 155 &&
                                imageColor.B >= 155)
                            {
                                factor = 1;
                            }
                            else
                            {
                                factor = 0.9;
                            }

                            red += imageColor.R * filter1[filterX, filterY];
                            green += imageColor.G * filter1[filterX, filterY];
                            blue += imageColor.B * filter1[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }


            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    sharpenImage.SetPixel(i, j, result[i, j]);
                }
            }

            return sharpenImage;
        }

    }
}


