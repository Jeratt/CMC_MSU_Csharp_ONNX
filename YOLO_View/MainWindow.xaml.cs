using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ViewModel;

namespace YOLO_View
{
    public class MessageBoxErrorReporter : IErrorReporter
    {
        public void reportError(string message)
        {
            System.Windows.MessageBox.Show(message);
        }
    }

    public class FolderManager : IFolderManager
    {
        public string openFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Choose directory";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            else
            {
                System.Windows.MessageBox.Show("Dialog processing failed!");
                return string.Empty;
            }
        }
    }

    public class ImageManager : IImageManager
    {
        public void SetSource(string source)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewData(new MessageBoxErrorReporter(), new FolderManager());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string))
            {
                return new BitmapImage();
            }
            else
            {
                string x = (string)value;
                return new ImageSourceConverter().ConvertFromString(x) as ImageSource;
                //return new BitmapImage(new Uri(@x, UriKind.Relative));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
