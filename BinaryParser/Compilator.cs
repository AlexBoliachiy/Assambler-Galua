using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


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
            mem.Gather();
            Console.WriteLine(mem.output);

            return true;

        }

        private string[] GetDataSection(string path)
        {
            string[] data = File.ReadAllLines(path);
            List<string> out_data = new List<string>();
            for (int i=1; data[i] != "CODE"; i++ )
            {

                out_data.Add(data[i]);
            }
            return out_data.ToArray();
        }
    }
}
