using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace TestApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Test("o.txt.", "s.txt", 8);
            Console.Read();
        }

        static void Test(string pathetalon, string str2, int t)
        {
            string etalon = File.ReadAllText(pathetalon);
            str2 = File.ReadAllText(str2);
            try {
                for (int i = 0; i < etalon.Length; i++)
                {
                    if (etalon[i] != str2[i])
                        Console.Write("*");
                    else
                        Console.Write(etalon[i]);
                    
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Идекс аут оф ренж");
            }
        }
    }
}
