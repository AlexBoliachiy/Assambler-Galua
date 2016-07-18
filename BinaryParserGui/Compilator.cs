using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

// Иногда студия криво считывает знак "-", почему неизвестно
namespace BinaryParserGui
{
    
    public class Compilator
    {
        int rowCountData = 0;
        public Compilator()
        {
            mem = new Memory();
            codeGenerator = new CodeGenerator(mem);
            rowCountData = 0;
        }


        public void Refresh()
        {
            mem = new Memory();
            codeGenerator = new CodeGenerator(mem);
            rowCountData = 0;
        }


        public Memory mem;
        public CodeGenerator codeGenerator;
        public bool Compilate(string input_code)
        {
            string[] commands = GetDataSection(input_code);
            int i = 0;
            foreach (string x in commands)
            {
                if (x != string.Empty)
                    if (!mem.HandleDataString(x))
                        throw new CompilationException("Деяка помилка трапилася у рядку" + i.ToString());
                i++;
            }
            var codeSec = GetCodeSection(input_code);
            mem.AddAllConstFromCodeSection(codeSec);
            mem.Gather();
            codeGenerator.HandleCodeSection(codeSec);
            PrintCodeHuman(codeGenerator.Code);
            return true;

        }


        public string GetCode()
        {
            string ouput = string.Empty;
            int i = 0;
            foreach (char x in codeGenerator.Code)
            {
                if (x == '\n')
                    continue;
                ouput += x;
                i++;
                if (i % 8 == 0)
                    ouput += '\n';
            }
            return ouput;

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
