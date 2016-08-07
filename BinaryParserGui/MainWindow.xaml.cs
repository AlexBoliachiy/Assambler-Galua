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
using System.Threading;
using System.Windows.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

//262626

namespace BinaryParserGui
{
    /// <summary>
    /// Interaction logic for IDE.xaml
    /// </summary>
    public partial class IDE : Window
    {
        private bool isPainting = false;
        public Compilator cmp = new Compilator();
        private string currentfile = string.Empty;
        private bool IsSaved = true; // Переменная, для отслеживания сохранности кода.          
        private int curTab = 0;
        private string prevText = string.Empty;
        private Color fontColor;
        public bool write = false;
        public bool SyntaxHighlite = true;
        public bool Acomp { get; set; }
        public static RoutedCommand Undo = new RoutedCommand();
        private Regex comment = new Regex(@"\/\/.*$", RegexOptions.Multiline);
        private Stopwatch stopWatch = new Stopwatch(); // using for drawing text only when user stop drawing
        private static Regex gfRegex = new Regex(@"#GF\(\s*((2\^[0-9]+)|(\d+))\s*\)");
        private static BrushConverter br = new BrushConverter();
        private static Regex hashValue = new Regex(@"#\s*((\d+)|(b'[10]+)|(h'[0-F]+))");
        public IDE()
        {
            stopWatch.Start();

            InitializeComponent();
            CommandBinding bind = new CommandBinding(ApplicationCommands.Save);
            bind.Executed += MenuItem_Save;
            this.CommandBindings.Add(bind);


            CommandBinding bind1 = new CommandBinding(ApplicationCommands.New);
            bind1.Executed += MenuItem_New;
            this.CommandBindings.Add(bind1);


            CommandBinding bind2 = new CommandBinding(ApplicationCommands.Open);
            bind2.Executed += MenuItem_Open;
            this.CommandBindings.Add(bind2);


            CommandBinding bind3 = new CommandBinding(ApplicationCommands.Close);
            bind3.Executed += MenuItem_Close;
            this.CommandBindings.Add(bind3);



            if (!Properties.Settings.Default.gamma)
            {
                editor.Background = new SolidColorBrush(Colors.White);
                code.Background = new SolidColorBrush(Colors.White);
                data.Background = new SolidColorBrush(Colors.White);
                codeNum.Background = new SolidColorBrush(Colors.White);
                dataNum.Background = new SolidColorBrush(Colors.White);


                editor.Foreground = new SolidColorBrush(Colors.Black);
                code.Foreground = new SolidColorBrush(Colors.Black);
                data.Foreground = new SolidColorBrush(Colors.Black);
                codeNum.Foreground = new SolidColorBrush(Colors.Black);
                dataNum.Foreground = new SolidColorBrush(Colors.Black);

                fontColor = Colors.Black;
            }

            else
            {
                editor.Background = (SolidColorBrush)(br.ConvertFrom("#262626"));
                code.Background = (SolidColorBrush)(br.ConvertFrom("#292929"));
                data.Background = (SolidColorBrush)(br.ConvertFrom("#292929"));
                dataNum.Background = (SolidColorBrush)(br.ConvertFrom("#292929"));
                codeNum.Background = (SolidColorBrush)(br.ConvertFrom("#292929"));

                editor.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
                code.Foreground = new SolidColorBrush(Colors.Wheat);
                data.Foreground = new SolidColorBrush(Colors.Wheat);
                codeNum.Foreground = new SolidColorBrush(Colors.Wheat);
                dataNum.Foreground = new SolidColorBrush(Colors.Wheat);

                fontColor = Colors.White;
            }
            editor.IsEnabled = false;
            Acomp = Properties.Settings.Default.Acomp;
            IsSaved = true;
            editor.Document.PageWidth = 10000;
            editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            write = Properties.Settings.Default.write;
            WriteImage.Visibility = Properties.Settings.Default.write ? Visibility.Visible : Visibility.Hidden;
            AcompIcon.Visibility = Properties.Settings.Default.Acomp ? Visibility.Visible : Visibility.Hidden;
            InterfaceIcon.Visibility = Properties.Settings.Default.gamma ? Visibility.Visible : Visibility.Hidden;
            SyntaxHighliteIcon.Visibility = SyntaxHighlite ? Visibility.Visible : Visibility.Hidden;
            OutputCode.Text = Properties.Settings.Default.CODENAME;
            OutputData.Text = Properties.Settings.Default.DATANAME;
            UserOutputIco.Visibility = Properties.Settings.Default.CODEBOOL ? Visibility.Visible : Visibility.Hidden;
            StopIcon.Visibility = Properties.Settings.Default.Stop ? Visibility.Visible : Visibility.Hidden;
        }


        public void InterfaceChange(bool gamma)
        {
            if (!gamma)
            {
                editor.Background = new SolidColorBrush(Colors.White);
                code.Background = new SolidColorBrush(Colors.White);
                data.Background = new SolidColorBrush(Colors.White);
                codeNum.Background = new SolidColorBrush(Colors.White);
                dataNum.Background = new SolidColorBrush(Colors.White);


                editor.Foreground = new SolidColorBrush(Colors.Black);
                code.Foreground = new SolidColorBrush(Colors.Black);
                data.Foreground = new SolidColorBrush(Colors.Black);
                codeNum.Foreground = new SolidColorBrush(Colors.Black);
                dataNum.Foreground = new SolidColorBrush(Colors.Black);



                fontColor = Colors.Black;
            }

            else
            {
                editor.Background = (SolidColorBrush)(br.ConvertFrom("#262626"));
                code.Background = (SolidColorBrush)(br.ConvertFrom("#292929"));
                data.Background = (SolidColorBrush)(br.ConvertFrom("#292929"));
                dataNum.Background = (SolidColorBrush)(br.ConvertFrom("#292929"));
                codeNum.Background = (SolidColorBrush)(br.ConvertFrom("#292929"));

                editor.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
                code.Foreground = new SolidColorBrush(Colors.Wheat);
                data.Foreground = new SolidColorBrush(Colors.Wheat);
                codeNum.Foreground = new SolidColorBrush(Colors.Wheat);
                dataNum.Foreground = new SolidColorBrush(Colors.Wheat);


                fontColor = Colors.White;
            }




        }
        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            editor.IsEnabled = true;
            dlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                currentfile = dlg.FileName;
                WaitWindow w = new WaitWindow();
                w.Show();
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                TextRange range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Text);
                if (SyntaxHighlite) // Если включена подсветка синтаксиса
                    PaintEditor();
                IsSaved = true;
                this.Title = dlg.FileName.Substring(dlg.FileName.LastIndexOf("\\") + 1);
                w.Close();
                fileStream.Close();
                this.Title += " - " + "Асемблер Галуа IDE";

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
            dlg.Filter = "Text File (*.txt)|*.txt|Show All Files (*.*)|*.*";
            if (currentfile != string.Empty)
                dlg.FileName = currentfile.Substring(currentfile.LastIndexOf("\\") + 1);
            else
                dlg.FileName = "New file";

            if (dlg.ShowDialog() == true)
            {
                SaveCode(dlg.FileName);
                currentfile = dlg.FileName;
                this.Title = currentfile;
            }
            else
            {
                currentfile = string.Empty;
            }

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
                {
                    if (currentfile == String.Empty)
                        MenuItem_SaveAs(sender, e);
                    else
                        MenuItem_Save(sender, e);
                    currentfile = string.Empty;
                    TextRange tr = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
                    data.Text = string.Empty;
                    tr.Text = string.Empty;
                    code.Text = string.Empty;
                    editor.IsEnabled = true;
                    this.Title = "New file - " + "Асемблер Галуа IDE";
                }
                else if (result == MessageBoxResult.No)
                {
                    TextRange tr = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
                    tr.Text = string.Empty;
                    currentfile = string.Empty;
                    data.Text = string.Empty;
                    code.Text = string.Empty;
                    editor.IsEnabled = true;
                    this.Title = "New file - " + "Асемблер Галуа IDE";
                }
                else if (result == MessageBoxResult.Cancel)
                    return;

            }
            else
            {
                TextRange tr = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
                tr.Text = string.Empty;
                currentfile = string.Empty;
                data.Text = string.Empty;
                code.Text = string.Empty;
                this.Title = "New file - " + "Асемблер Галуа IDE";
                editor.IsEnabled = true;
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
            Properties.Settings.Default.CODENAME = OutputCode.Text;
            Properties.Settings.Default.DATANAME = OutputData.Text;

            Properties.Settings.Default.Save();
           


        }

        private void editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPainting) // Для избежания рекурсии ( изменяя текст тут, рекурсивно будет вызываться эта же функция)
                return;

            IsSaved = false;
            if (Acomp)
                Dispatcher.BeginInvoke((Action)(() =>
               {
                   bool suc = true;
                   cmp = new Compilator();
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
                       code.Text = cmp.GetCodeWithComments();
                       if (Properties.Settings.Default.Stop)
                           code.Text += "01001111 // Stop-слово";
                   }
               }), DispatcherPriority.SystemIdle);



        }

        public delegate void TestThreadDelegate(ref Compilator cmp, ref TextBox data, ref TextBox code);

        public void compiliate(ref Compilator cmp, ref TextBox data, ref TextBox code)
        {
            bool suc = true;
            cmp = new Compilator();
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
            isPainting = true;



            TextRange tr = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(fontColor));

            if (!SyntaxHighlite)
            {
                isPainting = false;
                editor.UndoLimit = 100;
                return;
            }
            FillGF();
            FillHashValue();
            FillWordFromPosition("MOV_ARRAY", "#2e95e8");
            FillWordFromPosition("MOV_A", "#2e95e8");
            FillWordFromPosition("MOV", "#2e95e8");
            FillWordFromPosition("const", "#2e95e8");
            FillWordFromPosition("END_LOOP", "#58b8f0");
            FillWordFromPosition("Array", "#1dbb6b");
            FillWordFromPosition("DATA", "#f9dc1a");
            FillWordFromPosition("CODE", "#f9dc1a");
            FillWordFromPosition("ADD", "#2e95e8");
            FillWordFromPosition("MULT", "#2e95e8");
            FillWordFromPosition("DIV", "#2e95e8");
            FillWordFromPosition("POW", "#2e95e8");
            FillWordFromPosition("INVM", "#2e95e8");
            FillWordFromPosition("INVA", "#2e95e8");
            FillWordFromPosition("SUB", "#2e95e8");
            FillWordFromPosition("CDP", "#2e95e8");
            FillWordFromPosition("CPD", "#2e95e8");

            FillWordFromPosition("1JMP", "#2e95e8");
            FillWordFromPosition("LOOP", "#58b8f0");
            FillWordFromPosition("LOAD_CA_A", "#2e95e8");
            FillWordFromPosition("LOAD_CA", "#2e95e8");
            FillWordFromPosition("INC_DEC", "#2e95e8");
            FillWordFromPosition("OUT", "#2e95e8");
            PaintMultiLaneComments();
            PaintSingleLineComments();
            isPainting = false;


        }
        // Ишет все совпадения с указанным словом и окрашивает его 
        private void FillWordFromPosition(string word, string color)
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
                            (SolidColorBrush)(br.ConvertFrom(color)));
                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
                else
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
            }


        }
        private void FillGF()
        {
            TextPointer position = editor.Document.ContentStart;
            TextPointer endPosition = null;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf("#GF");

                    if (indexInRun >= 0 && gfRegex.IsMatch(textRun))
                    {

                        position = position.GetPositionAtOffset(indexInRun);
                        endPosition = position.GetPositionAtOffset(gfRegex.Match(textRun).Length);
                        TextRange colouringText = new TextRange(position, endPosition);
                        colouringText.ApplyPropertyValue(TextElement.ForegroundProperty,
                            (SolidColorBrush)(br.ConvertFrom("#FC0FC0")));
                        return;
                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
                else
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
        }
        private void FillHashValue()
        {
            TextPointer position = editor.Document.ContentStart;
            TextPointer endPosition = null;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf("#");

                    if (indexInRun >= 0 && hashValue.IsMatch(textRun))
                    {

                        position = position.GetPositionAtOffset(indexInRun);
                        endPosition = position.GetPositionAtOffset(hashValue.Match(textRun).Length);
                        TextRange colouringText = new TextRange(position, endPosition);
                        colouringText.ApplyPropertyValue(TextElement.ForegroundProperty,
                            (SolidColorBrush)(br.ConvertFrom("#FC0FC0")));
                        return;
                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
                else
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        private int CountTabulation() // Считает, сколько табуляций должно автоматически дописываться
        {
            TextPointer carret = editor.CaretPosition;
            TextPointer position = editor.Document.ContentStart;
            TextRange curTab = new TextRange(position, carret);
            Regex endloop = new Regex(@"END_LOOP\s+[0-3]\s*");
            Regex loop = new Regex(@"\s*LOOP\s+[0-3]\s*,\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*[+\-*/]\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*");
            var endLoopCol = endloop.Matches(curTab.Text); // ошибка - реджексы не матчатся
            var LoopCol = loop.Matches(curTab.Text);
            int LoopCount = LoopCol.Count;
            int endLoopCount = endLoopCol.Count;
            return LoopCount - endLoopCount;
        }


        private void editor_KeyUp(object sender, KeyEventArgs e)
        {
            var tr = new TextRange(editor.Document.ContentStart, editor.CaretPosition);
            if (e.Key == Key.Enter)
            {

                curTab = CountTabulation();
                removeTabNearEndLoop();
                for (int i = 0; i < curTab; i++)
                {
                    editor.CaretPosition.InsertTextInRun("\t");
                }
            }
            else if (e.Key == Key.Tab)
                editor.CaretPosition.InsertTextInRun("\t");
            //stopWatch.Stop();
            if (stopWatch.ElapsedMilliseconds > 2000)
                PaintEditor();
            else return;

            stopWatch.Reset();
            stopWatch.Start();


        }



        private void editor_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void removeTabNearEndLoop()
        {
            var tr = new TextRange(editor.Document.ContentStart, editor.CaretPosition);

            int index = tr.Text.LastIndexOf("\t");
            int indexENDLOOP = tr.Text.LastIndexOf("END_LOOP");
            if (index + 1 != indexENDLOOP || index == -1 || indexENDLOOP == -1)
            {
                return;
            }

            var str = tr.Text.Substring(index);
            int requireTabs = CountTabulation();
            try
            {
                for (int i = 0; tr.Text[index - i] == '\t'; i++)
                {
                    requireTabs--;
                }

            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            if (requireTabs != 0)
            {
                tr.Text = tr.Text.Remove(index, 1);

                curTab--;
            }

        }

        private void MenuItem_exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //код выглядит отвратительно
        private void MenuItem_Compile(object sender, RoutedEventArgs e)
        {
            var tr = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            bool suc = true;
            cmp = new Compilator(); // Слишком много внутренних параметров, приходится создавать новый объект
            TextRange textRange = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
            try
            {
                cmp.Compilate(textRange.Text);
            }
            catch (CompilationException ex)
            {
                suc = false;
                MessageBox.Show(ex.Message, "Помилка компіляції", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (suc == true)
                {
                    data.Text = cmp.mem.output;
                    code.Text = cmp.GetCodeWithComments();
                    if (Properties.Settings.Default.Stop)
                        code.Text += "01001111 // Stop-слово";
                    if (write == true)
                    {
                        if (currentfile == string.Empty) //Что бы записывать в файлы нужно сначала сохранить новый файл
                            MenuItem_SaveAs(null, null);
                        if (currentfile == string.Empty) //Если имя не поменялось
                        {
                            MessageBox.Show("Зроблено!\n" + "Вивід у файли не записаний, так як спочатку файл треба зберегти", "Успіх", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        }
                        else
                        {
                            if (Properties.Settings.Default.CODEBOOL == false || (Properties.Settings.Default.CODEBOOL == true && OutputCode.Text == string.Empty))
                                File.WriteAllText(currentfile.Remove(currentfile.Length - 4) + "_code" + ".txt", cmp.GetCode());
                            else
                                File.WriteAllText(currentfile.Remove(currentfile.LastIndexOf(@"\") + 1) + OutputCode.Text + ".txt", cmp.GetCode());
                            if (Properties.Settings.Default.DATABOOL == false || (Properties.Settings.Default.CODEBOOL == true && OutputCode.Text == string.Empty))
                                File.WriteAllText(currentfile.Remove(currentfile.Length - 4) + "_data" + ".txt", comment.Replace(data.Text, string.Empty));
                            else
                                File.WriteAllText(currentfile.Remove(currentfile.LastIndexOf(@"\") + 1) + OutputData.Text + ".txt", comment.Replace(data.Text, string.Empty));
                            
                            MessageBox.Show("Зроблено!\n" + "Вивід записаний у відповідні файли", "Успіх", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                        }
                    }
                    else
                        MessageBox.Show("Зроблено!\n", "Успіх", MessageBoxButton.OK, MessageBoxImage.Asterisk);




                }

            }

        }


        private void code_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lines = 0;
            codeNum.Text = string.Empty;
            for (int i = 0; i != code.Text.Length; i++)
            {
                if (code.Text[i] == '\n')
                {
                    codeNum.Text += "  " + lines.ToString() + "\n";
                    lines++;
                }
            }
            if (code.Text.Length - 1 != -1 && code.Text[code.Text.Length - 1] != '\n')
                codeNum.Text += "  " + lines.ToString() + "\n";


        }

        private void data_TextChanged(object sender, TextChangedEventArgs e)
        {
            dataNum.Text = string.Empty;
            int lines = 0;
            for (int i = 0; i != data.Text.Length; i++)
            {
                if (data.Text[i] == '\n')
                {
                    dataNum.Text += "  " + lines.ToString() + "\n";
                    lines++;
                }
            }

        }

        private void editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextPointer tp1 = editor.Selection.Start.GetLineStartPosition(0);
            TextPointer tp2 = editor.Selection.Start;

            int column = tp1.GetOffsetToPosition(tp2);

            int someBigNumber = int.MaxValue;
            int lineMoved, currentLineNumber;
            editor.Selection.Start.GetLineStartPosition(-someBigNumber, out lineMoved);
            currentLineNumber = -lineMoved;


            long size = 0;
            object o = new object();



            StatusBar.Content = "Лінія: " + currentLineNumber.ToString() + " Колонка: " + column.ToString();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                MenuItem_Compile(null, null);
            else if (e.SystemKey == Key.F10)
                AppClose();
            else if (e.Key == Key.F7)
                SyntaxHighlite = !SyntaxHighlite;

        }



        //Разукрашивает комментарии. Должно находится внутри PaintEditor
        // Чувак, мне срочно нужен RGB код вот такого цвета
        private void PaintMultiLaneComments()
        {
            TextPointer position = editor.Document.ContentStart;
            TextPointer endPosition = null;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf("/*");
                    if (indexInRun >= 0)
                    {
                        position = position.GetPositionAtOffset(indexInRun);
                        if (position.GetTextInRun(LogicalDirection.Forward).Contains("*/")) //частный случай
                        {
                            int index = position.GetTextInRun(LogicalDirection.Forward).IndexOf("*/");
                            TextRange tr = new TextRange(position, position.GetPositionAtOffset(index + 2));
                            tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                                    (SolidColorBrush)(br.ConvertFrom("#00FF00")));
                            position = position.GetNextContextPosition(LogicalDirection.Forward);
                            continue;
                        }
                        endPosition = position.GetPositionAtOffset(3);
                        while (endPosition != null) //
                        {

                            if (endPosition.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                            {
                                if (endPosition.GetTextInRun(LogicalDirection.Forward).Contains("*/"))
                                {
                                    int index = position.GetTextInRun(LogicalDirection.Forward).IndexOf("*/");
                                    TextRange tr = new TextRange(position, endPosition.GetPositionAtOffset(index + 3));
                                    tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                                    (SolidColorBrush)(br.ConvertFrom("#C0C0C0")));

                                    position = endPosition.GetPositionAtOffset(index);
                                    break;
                                }
                                else
                                    endPosition = endPosition.GetNextContextPosition(LogicalDirection.Forward);
                            }
                            else
                                endPosition = endPosition.GetNextContextPosition(LogicalDirection.Forward);

                        }




                    }
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
                else
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        public void PaintSingleLineComments()
        {
            TextPointer position = editor.Document.ContentStart;
            TextPointer endPosition;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf("//");
                    if (indexInRun >= 0)
                    {

                        position = position.GetPositionAtOffset(indexInRun);
                        endPosition = position.GetLineStartPosition(1);
                        if (endPosition == null)
                            endPosition = position.GetPositionAtOffset(textRun.Length - 1);
                        TextRange tr = new TextRange(position, endPosition);
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                                    (SolidColorBrush)(br.ConvertFrom("#C0C0C0")));
                        position = position.GetNextContextPosition(LogicalDirection.Forward);
                    }
                    else
                        position = position.GetNextContextPosition(LogicalDirection.Forward);
                }
                else
                    position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        private void MenuItem_Close(object sender, RoutedEventArgs e)
        {
            AppClose();
        }

        private void AppClose()
        {
            if (!IsSaved)
            {
                MessageBoxResult result = MessageBox.Show("Бажаєте зберегти зміни?", "Питанячко", MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    if (currentfile == String.Empty)
                        MenuItem_SaveAs(null, null);
                    else
                        MenuItem_Save(null, null);
                else if (result == MessageBoxResult.Cancel)
                    return;
            }
            IsSaved = true;
            currentfile = string.Empty;
            editor.Document.Blocks.Clear();
            data.Text = string.Empty;
            code.Text = string.Empty;
            dataNum.Text = string.Empty;
            codeNum.Text = string.Empty;
            this.Title = "Асемблер Галуа IDE";
            this.StatusBar.Content = String.Empty;
            editor.IsEnabled = false;
            IsSaved = true;
        }

        private void Settings_Write(object sender, RoutedEventArgs e)
        {
            write = !write;
            WriteImage.Visibility = write ? Visibility.Visible : Visibility.Hidden;
            Properties.Settings.Default.write = write;
        }

        private void Settings_interface(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.gamma = !Properties.Settings.Default.gamma;
            InterfaceChange(Properties.Settings.Default.gamma);
            InterfaceIcon.Visibility = Properties.Settings.Default.gamma ? Visibility.Visible : Visibility.Hidden;
        }

        private void Settings_SyntaxHighlight(object sender, RoutedEventArgs e) //Default: turn on
        {
            SyntaxHighlite = !SyntaxHighlite;
            SyntaxHighliteIcon.Visibility = SyntaxHighlite ? Visibility.Visible : Visibility.Hidden;
        }

        private void Settings_Acomp(object sender, RoutedEventArgs e)
        {
            Acomp = !Acomp;
            AcompIcon.Visibility = Acomp ? Visibility.Visible : Visibility.Hidden;
            Properties.Settings.Default.Acomp = Acomp;
        }

        private void Settings_OutputUserNames(object sender, RoutedEventArgs e)
        {
            
            Properties.Settings.Default.DATABOOL = ! Properties.Settings.Default.DATABOOL;
            Properties.Settings.Default.CODEBOOL = !Properties.Settings.Default.CODEBOOL;
            UserOutputIco.Visibility = Properties.Settings.Default.CODEBOOL ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItem_SettingsClosing(object sender, ContextMenuEventArgs e)
        {

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Stop = !Properties.Settings.Default.Stop;
         
            StopIcon.Visibility = Properties.Settings.Default.Stop ? Visibility.Visible : Visibility.Hidden;
        }
    }
}



