using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BinaryParser
{
    class Test
    {
        public void Test1(string PathToIdealMem, string PathToCode)
        {
            bool success = true;
            string[] IdealMem = File.ReadAllLines(PathToIdealMem);
            for (int i = 0; i < IdealMem.Length; i++)    
                IdealMem[i] = IdealMem[i].Replace(" ", string.Empty);
            Compilator cmp = new Compilator();
            cmp.Compilate(PathToCode);
            string[] almostIdealMem = cmp.mem.output.Split('\n');
            int Lenght = almostIdealMem.Length;
            if (almostIdealMem.Last() == String.Empty)
                Lenght--;
            for (int i = 0; i < Lenght; i++)
                if (almostIdealMem[i] != IdealMem[i])
                {
                    Console.WriteLine("Строки не совпадают в ряду " + i.ToString() + " " + almostIdealMem[i] + " " + IdealMem[i]);
                    
                    success = false;
                }
            if (IdealMem.Length != Lenght)
            {
                Console.WriteLine(" разница между количеством строк = {0}", IdealMem.Length - Lenght);
                success = false;
            }
            if (success)
                Console.WriteLine("OK");
            else
                Console.WriteLine("False");
        }
        public void CodeTest(string PathToCodeIdeal, string Path)
        {
            Compilator cmp = new Compilator();
            cmp.Compilate(Path);
            string str = cmp.GetCode();
            string readableStr = string.Empty;
            for (int i=0; i < str.Length; i++)
            {
                readableStr += str[i];
                if (i % 8 == 0 && i!=0)
                    readableStr += "      " + (i/8).ToString() + '\n' ;
            }
            string Ideal= File.ReadAllText(PathToCodeIdeal);
            Ideal = Ideal.Replace("\t", string.Empty);
            Ideal = Ideal.Replace(" ", string.Empty);
            Ideal = Ideal.Replace("\r", string.Empty);
            Ideal = Ideal.Replace("\n", string.Empty);
            str = str.Replace("\n", string.Empty);
            try
            {
                for (int i=0; i < Ideal.Length; i++)
                {
                    if (Ideal[i] !=str[i])
                    {
                        Console.WriteLine("На линии номер {0}, {1} несовпадение:", i / 8, i % 8);
                    }
                }
            }
            catch
            {
            }

        }


    }


}
