using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

// Иногда студия криво считывает знак "-", почему неизвестно
namespace BinaryParser
{
    
    class Compilator
    {
        public Memory mem = new Memory();
        public bool Compilate(string path)
        {
            string[] commands = GetDataSection(path);
            int i = 0;
            foreach (string x in commands)
            {
                if (x != "")
                    if (!mem.HandleDataString(x))
                        throw new Exception("some error occured in line " + i.ToString());
                i++;
            }
            mem.AddAllConstFromCodeSection(GetCodeSection(path));
            mem.Gather();
            Console.WriteLine(mem.output);

            return true;

        }

        private string[] GetDataSection(string path)
        {
            string[] data = File.ReadAllLines(path);
            List<string> out_data = new List<string>();
            string bar = data[0].Replace(" ", string.Empty);
            bar = bar.Replace("\t", string.Empty);
            if (bar != "DATA")
                throw new Exception("Отсуствует ключевое слово DATA");
            for (int i=1; data[i] != "CODE"; i++ )
            {

                out_data.Add(data[i]);
            }
            return out_data.ToArray();
        }

        private string[] GetCodeSection(string path)
        {
            string[] data = File.ReadAllLines(path);
            List<string> out_data = new List<string>();
            int i;
            for (i = 1; data[i] != "CODE"; i++)
            {
                // :)
            }

            for (; i < data.Length; i++)
                out_data.Add(data[i]);

            return out_data.ToArray();
        }
    }
}
