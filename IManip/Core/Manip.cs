using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

using IManip.Model;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using System;

namespace IManip.Core
{
    internal class Manip 
    {
        private const int EXPANSION = 1;
        private string pathToSaveFile = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        internal Manip() { }

        internal void TakePixelsFromByteArray(string path)
        {
            if (!Directory.Exists(pathToSaveFile + "\\temp"))
                Directory.CreateDirectory(pathToSaveFile + "\\temp");

            JsonSerializer serializer = new JsonSerializer();

            Bitmap bmp = new Bitmap(path);
            ARGBModel[,] argbObject = new ARGBModel[bmp.Height, bmp.Width];

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < bmp.Height; y++)
                {
                    var row = ptr + (y * bmpData.Stride);

                    for (int x = 0; x < bmp.Width; x++)
                    {
                        argbObject[y, x] = new ARGBModel();
                        var pixel = row + x * 4;

                        argbObject[y, x].A = pixel[3];
                        argbObject[y, x].R = pixel[2];
                        argbObject[y, x].G = pixel[1];
                        argbObject[y, x].B = pixel[0];
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter(pathToSaveFile + "\\temp\\log.json", false, System.Text.Encoding.UTF8))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, argbObject);
            }
        }

        internal Bitmap GettingPixelsFromJsonFile()
        {
            string result;

            using (StreamReader fs = new StreamReader(pathToSaveFile + "\\temp\\log.json"))
            {
                result = fs.ReadToEnd();
            }

            var ARGBModel = JsonConvert.DeserializeObject<ARGBModel[,]>(result);
            var ARGBList = new List<ARGBModel>();

            int height = ARGBModel.GetLength(0), width = ARGBModel.GetLength(1);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    ARGBList.Add(ARGBModel[i, j]);
                }
            }

            int currentWidth = (width * 2) + EXPANSION;
            int currentHeight = (height * 2) + EXPANSION;
            
            var PixelArray = AddNewPixelsToOld(ARGBList, currentWidth, currentHeight);
            var Result = ColorizeNewPixels(PixelArray, currentWidth, currentHeight);

            return GetBitmap(Result, currentWidth, currentHeight);

        }

        private Color[,] AddNewPixelsToOld(List<ARGBModel> ARGBList, int currentWidth, int currentHeight)
        {
            int position = 0;

            Color[,] result = new Color[currentWidth, currentHeight];

            for (int i = 0; i < currentWidth; i++)
            {
                for (int j = 0; j < currentHeight; j++)
                {
                    result[i, j] = Color.FromArgb(255, 255, 255, 255);

                    if (i % 2 == 0)
                        continue;

                    if (j % 2 != 0)
                    {
                        result[i, j] = Color.FromArgb(ARGBList[position].A,
                                                      ARGBList[position].R,
                                                      ARGBList[position].G,
                                                      ARGBList[position].B);
                        position++;
                    }
                }
            }

            return result;
        }

        private Color[,] ColorizeNewPixels(Color[,] pixels, int currentWidth, int currentHeight)
        {
            Color[,] result = pixels;
            bool skip = false;

            //Step 1
            //Colorize Near Pixels
            for (int i = 0; i < currentWidth; i++)
            {
                for (int j = 0; j < currentHeight; j++)
                {
                    if (skip)
                        continue;

                    if(result[i,j].Name != "0")
                    {
                        if(j + 2 < currentHeight && result[i, j + 2] != Color.FromArgb(255, 255, 255, 255))
                        {
                            Color currentColor = result[i, j];
                            Color nextColor = result[i, j + 2];

                            result[i, j + 1] = Color.FromArgb((currentColor.A + nextColor.A) / 2,
                                                              (currentColor.R + nextColor.R) / 2,
                                                              (currentColor.G + nextColor.G) / 2,
                                                              (currentColor.B + nextColor.B) / 2);
                        }

                        if (i + 2 < currentWidth && result[i + 2, j] != Color.FromArgb(255, 255, 255, 255))
                        {
                            Color currentColor = result[i, j];
                            Color nextColor = result[i + 2, j];
                            Color color = Color.FromArgb((currentColor.A + nextColor.A) / 2,
                                                         (currentColor.R + nextColor.R) / 2,
                                                         (currentColor.G + nextColor.G) / 2,
                                                         (currentColor.B + nextColor.B) / 2);
                            result[i + 1, j] = color;
                        }

                        //skip = true;
                    }
                    //Step 2
                    //Colorize even Line Pixels BORDER
                    else
                    {
                        result[i, j] = Color.FromArgb(255, 0, 0, 0); //Black
                    }
                }
            }

            return result;
        }

        private Bitmap GetBitmap(Color[,] pixels, int currentWidth, int currentHeight)
        {
            Bitmap result = new Bitmap(currentWidth, currentHeight, PixelFormat.Format32bppArgb);

            result.RotateFlip(RotateFlipType.RotateNoneFlipNone);
            
            for (int i = 0; i < currentWidth; i++)
            {
                for (int j = 0; j < currentHeight; j++)
                {
                    result.SetPixel(i, j, pixels[i, j]);
                }
            }

            return result;
        }

        internal BitmapImage MyConvertBitmap()
        {
            Bitmap bitmap = GettingPixelsFromJsonFile();

            MemoryStream ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Png);
            BitmapImage image = new BitmapImage();

            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.Rotation = Rotation.Rotate90;
            image.EndInit();

            Image img = Image.FromStream(ms);
            img.RotateFlip(RotateFlipType.Rotate90FlipX);
            img.Save(pathToSaveFile + "\\temp\\Picture" + String.Format("{0:yMdhms}", DateTime.Now) + ".png", ImageFormat.Png);

            return image;
        }

    }
}
