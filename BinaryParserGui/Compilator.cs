using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Numerics;
using DigitsPower;

// Иногда студия криво считывает знак "-", почему неизвестно
namespace BinaryParserGui
{

    
    public class Compilator
    {
        private static Regex CommentLine = new Regex(@"\/\/.*$", RegexOptions.Multiline);
        private static Regex CommentBetweenLine = new Regex(@"\/\*((.*)|)\*\/", RegexOptions.Singleline);
        private static Regex gfRegex = new Regex(@"#GF\(\s*((2\^[0-9]+)|(\d+))\s*\)");
        int rowCountData = 0; 
        public Compilator()
        {
            mem = new Memory();
            codeGenerator = new CodeGenerator(mem);
            rowCountData = 0;
        }


        public void Refresh()
        {
            mem.Refresh();
            rowCountData = 0;
            codeGenerator = new CodeGenerator(mem);
            
        }


        public Memory mem;
        public CodeGenerator codeGenerator;
        public bool Compilate(string input_code)
        {
            input_code = CommentLine.Replace(input_code, string.Empty);
            input_code = CommentBetweenLine.Replace(input_code, string.Empty);
            string[] commands = GetDataSection(input_code);
            int i = 0;
            int GF = -1;
            foreach (string x in commands)
            {

                if (x != string.Empty)
                {
                    if (GF == -1)  // Если м не объявлена ранее 
                    {
                        if (gfRegex.IsMatch(x))
                        {
                            if (x.Contains("^"))
                            {
                                mem.m = Convert.ToInt32(new Regex(@"\^\d+").Match(x).Value.Substring(1));
                                codeGenerator.gf = false;
                            }
                            else
                            {
                                codeGenerator.gf = true;

                                BigInteger b = (BigInteger)Convert.ToDouble(new Regex(@"\d+").Match(x).Value.Substring(0));
                                if (!b.IsProbablePrime())
                                {
                                    throw new CompilationException("p повинно бути простим числом !");
                                }
                                mem.m = (int)Math.Ceiling(Math.Log(Convert.ToDouble(new Regex(@"\d+").Match(x).Value.Substring(0)), 2));
                            }
                            GF = 1;
                        }
                        else throw new CompilationException("Спочатку повинна йти директива #GF");
                    }
                    if (!mem.HandleDataString(x))
                        throw new CompilationException("Деяка помилка трапилася в описі даних " + x);
                }
                i++;
            }
            var codeSec = GetCodeSection(input_code);
            mem.AddAllConstFromCodeSection(codeSec);
            mem.Gather();
            codeGenerator.HandleCodeSection(codeSec);
            return true;

        }


        public string GetCode()
        {
            string raw = codeGenerator.Code;
            for (int i=0; i < raw.Length-1; i+=9)
            {
                
                raw = raw.Insert(i, "\n");
            }
            if (raw.Length != 0)
                raw = raw.Substring(1);
            return raw;
        }

        public string GetCodeWithComments()
        {
            try
            {
                string[] codeSplit = GetCode().Split('\n');
                foreach (KeyValuePair<int, string> x in codeGenerator.comments)
                {
                    codeSplit[x.Key] += " " + x.Value;
                }
                string codeWithComments = string.Empty;
                foreach (string x in codeSplit)
                {
                    codeWithComments += x + "\n";
                }
                return codeWithComments;
            }
            catch(IndexOutOfRangeException )
            {
                throw new CompilationException("Здається, присутні неазакриті цикли. Закрийте їх та спробуйте ще раз");
            }
        }


        public void  PrintCodeHuman(string code)
        {
            int i = 0;
            foreach (char x in code)
            {
                if (x == '\n')
                    continue;
                Console.Write(x);
                i++;
                if (i % 8 == 0)
                    Console.Write(/*"      " + (i / 8 - 1).ToString() +*/ '\n');
            }
        }


        private string[] GetDataSection(string code)
        {

            code = code.Replace("\t", string.Empty);
            code = code.Replace("\r", string.Empty);
            string[] data = code.Split('\n');
            
            List<string> out_data = new List<string>();
            int m = 0;
            string bar = data[m].Replace(" ", string.Empty);
            bar = bar.Replace("\t", string.Empty);
            
            try
            {
                while (bar != "DATA")
                {
                    if (bar != string.Empty && bar != "DATA")
                        throw new CompilationException("Найпершою командою повина бути об'явлення секцii DATA");
                    m++;                  
                    bar = data[m].Replace(" ", string.Empty);

                }
                m++;
                
                for (; data[m] != "CODE"; m++)
                {
                    out_data.Add(data[m]);
                }
                codeGenerator.rowCountData = m; // Для отображения номера рядка ошибки синтаксиса
            }
            catch (IndexOutOfRangeException)
            {
                throw new CompilationException("Не вистачає ключевих слiв (DATA/CODE)");
            }
            
            return out_data.ToArray();
        }


        private string[] GetCodeSection(string code)
        {
            code = code.Replace("\t", " ");
            code = code.Replace("\r", string.Empty);
            string[] data = code.Split('\n');
            List<string> out_data = new List<string>();
            int i;
            for (i = 1; data[i] != "CODE"; i++)
            {
                // :)
            }

            for (; i < data.Length; i++)
            {
                out_data.Add(data[i]);
            }

            return out_data.ToArray();
        }
    }
}
