﻿using System;
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
            Test test = new Test();
            //test.Test1(@"..\..\..\Test\t1.txt", @"..\..\..\text1.txt"); // OK
            test.Test1(@"..\..\..\Test\t2.txt", @"..\..\..\text2.txt"); // Эталон некорректен

            Console.ReadKey();
        }
    }
}
