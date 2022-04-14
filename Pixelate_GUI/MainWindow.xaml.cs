using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Win32;
using System.Drawing;
using Pixelate_Core;

namespace Pixelation_
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bitmap originalBitmap;
        Bitmap pixelatedBitmap;
        public ObservableCollection<string> Colors = new ObservableCollection<string>();
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Open_Image_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                if (originalBitmap != null)
                    ImageHelper.DeleteObject(originalBitmap.GetHbitmap());

                Bitmap bitmap = new Bitmap(op.FileName);

                originalBitmap = bitmap;
                Source_Image.Source = ImageHelper.GetBitmapSource(bitmap);
                Source_Image.Source.Freeze();
            }
        }

        private void Save_Image_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Pixelated Image";
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png";
            if (dlg.ShowDialog() == true)
            {
                
                var encoder = new JpegBitmapEncoder(); // Or PngBitmapEncoder, or whichever encoder you want
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)Pixelated_Image.Source));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }
            }
        }

        private void Pixelate_Click(object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(Scale_Factor.Text, out int scaleFactor))
            {
                MessageBox.Show("Incorrent Scale Factor", "Error", MessageBoxButton.OK);
                return;
            }

            if (!Int32.TryParse(Color.Text, out int colors))
            {
                MessageBox.Show("Incorrent Colors Amount", "Error", MessageBoxButton.OK);
                return;
            }
            if (All_Mode.IsChecked.Value)
            {
                colors = -1;
            }
            else
                colors += Colors.Count;

            if (pixelatedBitmap != null)
                ImageHelper.DeleteObject(pixelatedBitmap.GetHbitmap());
            
            pixelatedBitmap = Pixelate_Core.Pixelator.Pixelate(ImageHelper.GetBitmap((BitmapSource)Source_Image.Source), Colors.Select(x => System.Drawing.ColorTranslator.FromHtml(x)), scaleFactor, colors, Random_Mode.IsChecked.Value);

            Pixelated_Image.Source = ImageHelper.GetBitmapSource(pixelatedBitmap);
            Pixelated_Image.Source.Freeze();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Random_Mode_Checked(object sender, RoutedEventArgs e)
        {
            Color.IsEnabled = true;
        }

        private void All_Mode_Checked(object sender, RoutedEventArgs e)
        {
            Color.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ColorConfiguration colorConfiguration = new ColorConfiguration();
            colorConfiguration.Owner = this;
            colorConfiguration.Colors.ItemsSource = Colors;

            colorConfiguration.ShowDialog();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            originalBitmap = ImageHelper.GetBitmap((BitmapSource)Source_Image.Source);
        }
    }
}
