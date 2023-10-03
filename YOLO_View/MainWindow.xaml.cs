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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModel;
using static System.Net.Mime.MediaTypeNames;

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

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewData(new MessageBoxErrorReporter(), new FolderManager());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
