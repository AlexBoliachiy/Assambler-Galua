using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BinaryParser
{
    class Memory
    {
        private int m;
        private Dictionary<string, int> variables = new Dictionary<string, int>();

        public Memory(string DATA)
        {
            Regex regex = new Regex("^const m = \\d+$");
            int count = regex.Matches(DATA).Count;
            //Проверка на то, есть ли m
            if (count == 0)
            {
                throw new Exception("M not specified!");
            }// Не слишком ли много раз m задана
            else if (count > 1) 
            {
                throw new Exception("Too many M!");
            }




        }

        private bool HandleData           String(string cmd)
        {
            cmd.Trim(); //Remove extra whitespaces
            Regex con = new Regex(@"^const\s+[A-Za-z_]+[A-Z_a-z0-9]*\s*=\s*\d+\s*$"); 
            Regex arr = new Regex(@"Array\s+[A-Za-z_]+[A-Z_a-z0-9]*
                                \[\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*[+\-*/]\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*:\s*0\]\s*=
                                \s*\(\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*,\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*\)\s*");
            Regex var = new Regex(@"[A-Za-z_]+[A-Z_a-z0-9]*\s*=\s*\d+\s*$");
            string[] cmd_split = cmd.Split(' ');
            if (cmd_split[0] == "const")
            {
                if (con.IsMatch(cmd))
                {
                    //Добавляем переменную и её значение в variables
                    //Добавляем в output данные
                }
            }
            else if (cmd_split[0] == "Array")
            {
                if (arr.IsMatch(cmd))
                {
                    //Сплитами парсим массив и выделяем все значения
                    //Добавляем в variables и output
                }
            }
            else
            {
                
            }
    
        }

    }

}
