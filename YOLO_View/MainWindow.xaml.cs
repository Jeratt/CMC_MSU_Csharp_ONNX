using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
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
using static System.Net.Mime.MediaTypeNames;

namespace YOLO_View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public BitmapImage bitmap { get; set; }

        public static RoutedCommand LoadImageFromFileCommand = new RoutedCommand("LoadImageFromFile", typeof(MainWindow));
        public MainWindow()
        {
            bitmap = new BitmapImage();
            InitializeComponent();
            this.DataContext = this;
            this.CommandBindings.Add(new CommandBinding(LoadImageFromFileCommand, LoadImageFromFile));
        }

        private void LoadImageFromFile(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("KEK");
            Microsoft.Win32.OpenFileDialog loader = new Microsoft.Win32.OpenFileDialog();
            try
            {
                if ((bool)loader.ShowDialog())
                {
                    /*                    bitmap.BeginInit();
                                        bitmap.CacheOption = BitmapCacheOption.None;
                                        bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                        bitmap.UriSource = new Uri(loader.FileName, UriKind.Relative);
                                        bitmap.EndInit();
                                        pic.Stretch = Stretch.Fill;*/
                    //pic.Source = bitmap;
                    pic.Source = new BitmapImage(new Uri(loader.FileName));
                }
                else
                {
                    MessageBox.Show("Error on loading\n raw data!");
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("Wrong data read\n from file!!!");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
