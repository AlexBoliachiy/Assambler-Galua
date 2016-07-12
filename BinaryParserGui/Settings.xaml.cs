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
using System.Windows.Shapes;

namespace BinaryParserGui
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        IDE ide; 
        public Settings(bool curWrite, IDE window)
        {
            InitializeComponent();
            IsWriteFile.IsChecked = curWrite;
            ide = window;
            IsGammaBlack.IsChecked = Properties.Settings.Default.gamma;
        }

        private void IsWriteFile_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void IsWriteFile_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ide.write = (bool) IsWriteFile.IsChecked;
            if (IsGammaBlack.IsChecked != Properties.Settings.Default.gamma)
            {
                Properties.Settings.Default.gamma = !Properties.Settings.Default.gamma;
                ide.InterfaceChange(Properties.Settings.Default.gamma);
                Properties.Settings.Default.Save();
            }
        }

        private void IsGammaBlack_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void IsGammaBlack_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
