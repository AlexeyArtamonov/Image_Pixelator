using Microsoft.Win32;
using Pixelate_Core;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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
            ScaleModes.ItemsSource = Enum.GetValues(typeof(Pixelator.ScaleMode));
            ScaleModes.SelectedValue = Pixelator.ScaleMode.NearestNeighbor;

            ColorModes.ItemsSource = Enum.GetValues(typeof(Pixelator.ColorMode));
            ColorModes.SelectedValue = Pixelator.ColorMode.Random;
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

            if (!Int32.TryParse(Color.Text, out int colorsAmount))
            {
                MessageBox.Show("Incorrent Colors Amount", "Error", MessageBoxButton.OK);
                return;
            }

            if (pixelatedBitmap != null)
                ImageHelper.DeleteObject(pixelatedBitmap.GetHbitmap());

            pixelatedBitmap = Pixelate_Core.Pixelator.Pixelate(
                ImageHelper.GetBitmap((BitmapSource)Source_Image.Source),
                Colors.Select(x => System.Drawing.ColorTranslator.FromHtml(x)),
                scaleFactor,
                colorsAmount,
                (Pixelator.ScaleMode)ScaleModes.SelectedItem, (Pixelator.ColorMode)ColorModes.SelectedItem);

        
            Pixelated_Image.Source = ImageHelper.GetBitmapSource(pixelatedBitmap);
            Pixelated_Image.Source.Freeze();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
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

        private void ColorModes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ColorModes.SelectedValue != null && (Pixelator.ColorMode)ColorModes.SelectedValue == Pixelator.ColorMode.AllColors)
                Color.IsEnabled = false;
            else
                Color.IsEnabled = true;
        }
    }
}
