using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Memory mem = new Memory();
            mem.HandleDataString("Array   kek_228   [1+ 1 + 1 +1:0] = ( 2 , 2,sec_12  )");
            Console.ReadKey();
        }
    }
}
