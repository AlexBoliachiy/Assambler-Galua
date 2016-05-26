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
        private PostfixNotationExpression p = new PostfixNotationExpression();
        public Memory()
        {

        }




        

        public bool HandleDataString(string cmd)
        {
            cmd.Trim(); //Remove extra whitespaces
            Regex con = new Regex(@"^const\s+[A-Za-z_]+[A-Z_a-z0-9]*\s*=\s*\d+\s*$");
            Regex arr = new Regex(@"^Array\s+[A-Za-z_]+[A-Z_a-z0-9]*\s*\[\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*[+\-*/]\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*:\s*0\]\s*=\s*\(\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*,\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*\)\s*$");
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
                    HandleArray(cmd);
                    //Сплитами парсим массив и выделяем все значения
                    //Добавляем в variables и output
                }
            }
            else
            {
                
            }
            return true;
        }

        private void HandleArray(string arr)
        {
            string[] chars = arr.Split(new char[] {  '[', ']', '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var x in chars)
            {
                chars[i] = x.Replace(" ", "");  // Убираем лишние пробелы 
                i++;
            }
            foreach (var x in chars)
                Console.WriteLine(x);
            string ArrayName = GetArrayName("arr");
            decimal lenght = GetLenghtExp(chars[1]);
            Console.WriteLine(lenght);
        }

        private string GetArrayName(string arr) //Подаем сюда chars[0]
        {
            string name = arr.Replace(" ", "");
            return name.Replace("Array", "");
         
        }
        private decimal GetLenghtExp(string arr) //Подаем сюда chars[1]
        {
            arr = arr.Replace(":0", "");
            return p.result(arr);
        }

    }

}
