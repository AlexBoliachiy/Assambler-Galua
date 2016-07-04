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
using Microsoft.Win32;
using System.IO;

namespace BinaryParserGui
{
    /// <summary>
    /// Interaction logic for IDE.xaml
    /// </summary>
    public partial class IDE : Window
    {
        private Compilator cmp = new Compilator();
        private string currentfile = string.Empty;
        private bool IsSaved = true; // Переменная, для отслеживания сохранности кода.
        public IDE()
        {
            InitializeComponent();
        }

        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                currentfile = dlg.FileName;
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                TextRange range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Text);
                IsSaved = true;
            }
        }

        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            if (currentfile == String.Empty)
            {
                this.MenuItem_SaveAs(this, e);
            }
            else
            {
                SaveCode(currentfile);
            }
        }

        private void MenuItem_SaveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == true)
            {
                SaveCode(dlg.FileName);
            }
            currentfile = dlg.FileName;
        }

        private void SaveCode(string FileName)
        {
            FileStream fileStream = new FileStream(FileName, FileMode.Create);
            TextRange range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            range.Save(fileStream, DataFormats.Text);
            IsSaved = true;
            fileStream.Close();
        }

        private void MenuItem_New(object sender, RoutedEventArgs e)
        {
            if (!IsSaved)
            {
                MessageBoxResult result = MessageBox.Show("Бажаєте зберегти зміни?", "Питанячко", MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    if (currentfile == String.Empty)
                        MenuItem_SaveAs(sender, e);
                    else
                        MenuItem_Save(sender, e);
                else if (result == MessageBoxResult.No)
                {
                    editor.DataContext = string.Empty;
                    currentfile = string.Empty;
                }
                else if (result == MessageBoxResult.Cancel)
                    return;

            }
        }

        
        

        private void Window_Exit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsSaved)
            {
                MessageBoxResult result = MessageBox.Show("Бажаєте зберегти зміни?", "Питанячко", MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    if (currentfile == String.Empty)
                        MenuItem_SaveAs(sender, null);
                    else
                        MenuItem_Save(sender, null);
                else if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;

            }

        }

        private void editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsSaved = false;
            bool suc = true; 
            cmp = new Compilator(); // Слишком много внутренних параметров, что бы просто провести очистку
            TextRange textRange = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            try
            {
                cmp.Compilate(textRange.Text);
            }
            catch (CompilationException ex)
            {
                suc = false;
            }
            if (suc == true)
            {
                data.Text = cmp.mem.output;
                code.Text = cmp.GetCode();
            }
        }

        //Окрашивает отдельные команды (подсветка синтаксиса)
        private void PaintEditor()
        {
            TextRange textRange = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd); // получаем текст
            string copy = textRange.Text;
        }
    }
}
