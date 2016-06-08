using System;
using System.Collections.Generic;
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
using System.IO;
using Microsoft.Win32;


namespace BinaryParserGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Compilator cmp = new Compilator();
            OpenFileDialog o = new OpenFileDialog();
            string path;
            try
            {
                if (o.ShowDialog() == true)
                {
                    cmp.Compilate(o.FileName);
                    code.Text = cmp.GetCode();
                    data.Text = cmp.mem.output;
                    System.IO.File.WriteAllText(@"ram_init_code.txt", code.Text);
                    System.IO.File.WriteAllText(@"ram_init_data.txt.txt", data.Text);
                }
            }
            catch (CompilationException ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
