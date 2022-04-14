using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace Pixelation_
{
    /// <summary>
    /// Логика взаимодействия для ColorConfiguration.xaml
    /// </summary>
    public partial class ColorConfiguration : Window
    {
        System.Windows.Forms.ColorDialog colorDialog;
        public ListBox Pallete { get { return Colors; } set { Colors = value; } }
        public ColorConfiguration()
        {
            InitializeComponent();
            ColorName.Focus();
        }

        private void AddColorButton_Click(object sender, RoutedEventArgs e)
        {
            string colorName = ColorName.Text;
            if (colorName == null)
                return;

            if (Regex.Match(colorName, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
            {
                ((MainWindow)Owner).Colors.Add(colorName);
            }
            
            ColorName.Text = string.Empty;
        }

        private void ColorName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddColorButton_Click(sender, e);
        }

        private void Colors_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                if (Colors.SelectedIndex != -1)
                    ((MainWindow)Owner).Colors.RemoveAt(Colors.SelectedIndex);
            }
        }

        private void SelectColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (colorDialog == null)
                colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((MainWindow)Owner).Colors.Add(System.Drawing.ColorTranslator.ToHtml(colorDialog.Color));
            }
        }

        private void DeleteColor_Click(object sender, RoutedEventArgs e)
        {
            if (Colors.SelectedIndex != -1)
                ((MainWindow)Owner).Colors.RemoveAt(Colors.SelectedIndex);
        }
    }
}
