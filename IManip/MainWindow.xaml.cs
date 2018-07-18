using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using IManip.Core;

namespace IManip
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(openFileDialog.FileName))
                };

                BasedImage.Source = image.Source;
                
                Manip test = new Manip();
                test.TakePixelsFromByteArray(openFileDialog.FileName);
            }
            

        }

        private void ConvertImageButton_Click(object sender, RoutedEventArgs e)
        {
            Manip test = new Manip();
            ImageSourceConverter imageSourceConverter = new ImageSourceConverter();

            ConvertedImage.Source = test.MyConvertBitmap();
        }


    }
}
