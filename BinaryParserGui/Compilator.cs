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
        private static Regex hashValue = new Regex(@"#\s*((\d+)|(b'[10]+)|(h'[0-F]+))");
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
            string[] commands = GetDataAndSetGFSection(input_code);
            int i = 0;
            foreach (string x in commands)
            {

                if (x != string.Empty && !mem.HandleDataString(x))
                {    
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


        private string[] GetDataAndSetGFSection(string code) // Получить секцию Дата и попутно установить размерность поля
        {

            code = code.Replace("\t", string.Empty);
            code = code.Replace("\r", string.Empty);
            string[] data = code.Split('\n');
            
            List<string> out_data = new List<string>();
            int m = 0;
            string bar = data[m].Replace(" ", string.Empty);
            bar = bar.Replace("\t", string.Empty);
            int GF = -1;
            string irreducible_polynomial = string.Empty;
            try
            {
               
                while (bar != "DATA")
                {

                    if (GF == -1 && gfRegex.IsMatch(bar))  // Если м не объявлена ранее 
                    {

                        if (bar.Contains("^"))
                        {
                            mem.m = Convert.ToInt32(new Regex(@"\^\d+").Match(bar).Value.Substring(1));
                            codeGenerator.gf = false;
                        }
                        else
                        {
                            codeGenerator.gf = true;

                            BigInteger b = (BigInteger)Convert.ToDouble(new Regex(@"\d+").Match(bar).Value.Substring(0));
                            if (!b.IsProbablePrime())
                            {
                                throw new CompilationException("p повинно бути простим числом !");
                            }
                            double p = Convert.ToDouble(new Regex(@"\d+").Match(bar).Value.Substring(0));
                            mem.m = (int)Math.Ceiling(Math.Log(p, 2));
                            irreducible_polynomial = "const p = " + ((int)p).ToString();
                        }
                        GF = 1;
                    }
                    else if (GF != -1 && gfRegex.IsMatch(bar)) 
                        throw new CompilationException("Повторна об'ява директиви GF");
                    else if (GF == 1 && codeGenerator.gf == false && hashValue.IsMatch(bar))
                    {
                        int value = mem.ExpressionToInt(bar.Substring(bar.IndexOf("#") + 1));
                        irreducible_polynomial = "const irreducible_polynomial = " + value.ToString() ;

                    }
                    else if (bar != string.Empty && bar != "DATA")
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
            if (GF == -1)
                throw new CompilationException("Відсутня директива GF (вона повина йти перед початком секції даних)");
            out_data.Insert(0, irreducible_polynomial); // Добавляем первой командой строку со значением # в режиме GF(2^m) 
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
