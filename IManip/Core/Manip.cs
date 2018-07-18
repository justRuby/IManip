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

            //Check proportionality

            if (height > width)
                height = width;
            else
                width = height;

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
            Color EmptyPixel = Color.FromArgb(255, 255, 255, 255);
            Color[,] result = pixels;
            bool skip = false;
            int currentPosition = 2;

            //Colorize Near Pixels
            #region Step 1

            for (int i = 0; i < currentWidth; i++)
            {
                for (int j = 0; j < currentHeight; j++)
                {
                    if (skip)
                    {
                        skip = false;
                        continue;
                    }

                    if (result[i, j].Name != "0")
                    {

                        if (j + 2 < currentHeight && result[i, j + 2] != Color.FromArgb(255, 255, 255, 255))
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

                        skip = true;
                    }

                }
            }

            #endregion

            //Colorize outside pixels BORDER
            #region Step 2

            if (result[1, 1] != EmptyPixel)
            {
                result[0, 0] =
                result[1, 0] =
                result[0, 1] = Color.FromArgb(result[1, 1].A,
                                              result[1, 1].R,
                                              result[1, 1].G,
                                              result[1, 1].B);
            }

            if (result[currentWidth - 2, 1] != EmptyPixel)
            {
                result[currentWidth - 1, 0] =
                result[currentWidth - 2, 0] =
                result[currentWidth - 1, 1] = Color.FromArgb(result[currentWidth - 2, 1].A,
                                                             result[currentWidth - 2, 1].R,
                                                             result[currentWidth - 2, 1].G,
                                                             result[currentWidth - 2, 1].B);
            }

            if (result[1, currentHeight - 2] != EmptyPixel)
            {
                result[0, currentHeight - 1] =
                result[0, currentHeight - 2] =
                result[1, currentHeight - 1] = Color.FromArgb(result[1, currentHeight - 2].A,
                                                              result[1, currentHeight - 2].R,
                                                              result[1, currentHeight - 2].G,
                                                              result[1, currentHeight - 2].B);
            }

            if (result[currentWidth - 2, currentHeight - 2] != EmptyPixel)
            {
                result[currentWidth - 1, currentHeight - 1] =
                result[currentWidth - 2, currentHeight - 1] =
                result[currentWidth - 1, currentHeight - 2] = Color.FromArgb(result[currentWidth - 2, currentHeight - 2].A,
                                                                             result[currentWidth - 2, currentHeight - 2].R,
                                                                             result[currentWidth - 2, currentHeight - 2].G,
                                                                             result[currentWidth - 2, currentHeight - 2].B);
            }

            #endregion

            //Colorize inner pixels
            #region Step 2.1

            for (int i = 1; i < currentWidth - 1; i++)
            {
                for (int j = 1; j < currentHeight - 1; j++)
                {
                    if (result[i, j] == EmptyPixel)
                    {
                        result[i, j] = Color.FromArgb((result[i - 1, j].A + result[i, j - 1].A + result[i + 1, j].A + result[i, j + 1].A) / 4,
                                                      (result[i - 1, j].R + result[i, j - 1].R + result[i + 1, j].R + result[i, j + 1].R) / 4,
                                                      (result[i - 1, j].G + result[i, j - 1].G + result[i + 1, j].G + result[i, j + 1].G) / 4,
                                                      (result[i - 1, j].B + result[i, j - 1].B + result[i + 1, j].B + result[i, j + 1].B) / 4);
                    }
                }
            }

            #endregion

            //Colorize outside line of pixels BORDER
            #region Step 3.1

            while (result[currentPosition, 0] == EmptyPixel)
            {
                result[currentPosition, 0] = Color.FromArgb((result[currentPosition - 1, 0].A + result[currentPosition, 1].A) / 2,
                                                            (result[currentPosition - 1, 0].R + result[currentPosition, 1].R) / 2,
                                                            (result[currentPosition - 1, 0].G + result[currentPosition, 1].G) / 2,
                                                            (result[currentPosition - 1, 0].B + result[currentPosition, 1].B) / 2);
                currentPosition++;
            }

            currentPosition = 2;

            while (result[0, currentPosition] == EmptyPixel)
            {
                result[0, currentPosition] = Color.FromArgb((result[0, currentPosition - 1].A + result[1, currentPosition].A) / 2,
                                                            (result[0, currentPosition - 1].R + result[1, currentPosition].R) / 2,
                                                            (result[0, currentPosition - 1].G + result[1, currentPosition].G) / 2,
                                                            (result[0, currentPosition - 1].B + result[1, currentPosition].B) / 2);
                currentPosition++;
            }

            currentPosition = 2;

            while (result[currentPosition, currentWidth - 1] == EmptyPixel)
            {
                result[currentPosition, currentWidth - 1] = Color.FromArgb((result[currentPosition - 1, currentWidth - 1].A + result[currentPosition, currentWidth - 2].A) / 2,
                                                                           (result[currentPosition - 1, currentWidth - 1].R + result[currentPosition, currentWidth - 2].R) / 2,
                                                                           (result[currentPosition - 1, currentWidth - 1].G + result[currentPosition, currentWidth - 2].G) / 2,
                                                                           (result[currentPosition - 1, currentWidth - 1].B + result[currentPosition, currentWidth - 2].B) / 2);
                currentPosition++;
            }

            currentPosition = 2;

            while (result[currentHeight - 1, currentPosition] == EmptyPixel)
            {
                result[currentHeight - 1, currentPosition] = Color.FromArgb((result[currentHeight - 1, currentPosition - 1].A + result[currentHeight - 2, currentPosition].A) / 2,
                                                                            (result[currentHeight - 1, currentPosition - 1].R + result[currentHeight - 2, currentPosition].R) / 2,
                                                                            (result[currentHeight - 1, currentPosition - 1].G + result[currentHeight - 2, currentPosition].G) / 2,
                                                                            (result[currentHeight - 1, currentPosition - 1].B + result[currentHeight - 2, currentPosition].B) / 2);
                currentPosition++;
            }

            #endregion

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
