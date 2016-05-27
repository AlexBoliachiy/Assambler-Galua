using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;


namespace BinaryParser
{
    class Program
    {

        static void Main(string[] args)
        {
            Compilator cmp = new Compilator();
            cmp.Compilate(@"..\..\..\text.txt");
            Console.ReadKey();
        }
    }
}
