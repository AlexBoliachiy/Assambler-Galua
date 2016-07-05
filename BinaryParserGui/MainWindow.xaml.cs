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
using System.Text.RegularExpressions;

//262626

namespace BinaryParserGui
{
    /// <summary>
    /// Interaction logic for IDE.xaml
    /// </summary>
    public partial class IDE : Window
    {
        private bool isPainting = false;
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
            if (isPainting)
                return;
            else
                isPainting = true;

            cmp = new Compilator(); // Слишком много внутренних параметров, приходится создавать новый объект
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
                PaintEditor();
            }
            isPainting = false;
        }

        //Окрашивает отдельные команды (подсветка синтаксиса)
        private void PaintEditor()
        {
            /*for (int i = 0; i < commands.Count; i++)
            {
                var matches = commands[i].Matches(textRange.Text);
                for (int j=0; j < matches.Count; j++)
                {
                    TextRange colouringText = SubTextRange(textRange, matches[j].Index, matches[j].Length);
                    colouringText.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
                }
            }*/
            FillWordFromPosition("const", "#2e95e8");
            FillWordFromPosition("MOV_ARRAY", "#2e95e8");
            FillWordFromPosition("END_LOOP", "#58b8f0");
            FillWordFromPosition("Array", "#1dbb6b");
            FillWordFromPosition("DATA", "#f9dc1a");
            FillWordFromPosition("CODE", "#f9dc1a");
            FillWordFromPosition("ADD", "#2e95e8");
            FillWordFromPosition("MULT", "#2e95e8");
            FillWordFromPosition("DIV", "#2e95e8");
            FillWordFromPosition("POW", "#2e95e8");
            FillWordFromPosition("INV", "#2e95e8");
            FillWordFromPosition("CDP", "#2e95e8");
            FillWordFromPosition("CPD", "#2e95e8");
            FillWordFromPosition("MOV_A", "#2e95e8");
            FillWordFromPosition("1JMP", "#2e95e8");
            FillWordFromPosition("LOOP", "#58b8f0");
            FillWordFromPosition("LOAD_CA_A", "#2e95e8");
            FillWordFromPosition("LOAD_CA", "#2e95e8");
            FillWordFromPosition("INC_DEC", "#2e95e8");
            FillWordFromPosition("OUT", "#2e95e8");

            // FillWordFromPosition("Array",)



        }
        private void FillWordFromPosition( string word, string color)
        {
            
            TextPointer position = editor.Document.ContentStart;
            TextPointer endPosition = null;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(word);
                    if (indexInRun >= 0)
                    {
                        position = position.GetPositionAtOffset(indexInRun);
                        endPosition = position.GetPositionAtOffset(word.Length);
                        TextRange colouringText = new TextRange(position, endPosition);
                        colouringText.ApplyPropertyValue(TextElement.ForegroundProperty, 
                            (SolidColorBrush)(new BrushConverter().ConvertFrom(color)));
                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
                else
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            // position will be null if "word" is not found.
            
        }

        private void PaintBackground()
        {
            
        }
    }
}
