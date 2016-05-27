using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BinaryParser
{
    enum types
    {
        cons,
        vars,
        arrs
    }

    class Memory
    {
        private int m;
        private string output;
        private Dictionary<string, int> variables = new Dictionary<string, int>();
        private PostfixNotationExpression p = new PostfixNotationExpression();
        private Regex dec_con = new Regex(@"^const\s+[A-Za-z_]+[A-Z_a-z0-9]*\s*=\s*\d+\s*$");
        private Regex dec_arr = new Regex(@"^Array\s+[A-Za-z_]+[A-Z_a-z0-9]*\s*\[\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*[+\-*/]\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*:\s*0\]\s*=\s*\(\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*,\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*\)\s*$");
        private Regex dec_var = new Regex(@"^[A-Za-z_]+[A-Z_a-z0-9]*\s*=\s*\d+\s*$");
        private Regex var = new Regex(@"[A-Za-z_]+[A-Z_a-z0-9]*");

        public Memory()
        {

        }




        

        public bool HandleDataString(string cmd)
        {
            cmd.Trim(); //Remove extra whitespaces
            string[] cmd_split = cmd.Split(' ');
            if (cmd_split[0] == "const")
            {
                if (dec_con.IsMatch(cmd))
                {
                    //Добавляем переменную и её значение в variables
                    //Добавляем в output данные
                    string const_name = cmd_split[1];
                    if (variables.ContainsKey(const_name))
                        return false;
                    int const_value = Convert.ToInt32(ReplaceVariableToValue(cmd_split[3]));

                    if (const_name != "m")
                    {
                        if (Math.Pow(2, (double)m) - 1 < const_value)
                        {
                            return false;
                        }
                        else
                        {
                            variables.Add(const_name, const_value);
                        }
                    }
                    else
                    {
                        variables.Add("m", const_value);
                        m = const_value;
                    }


                }
            }
            else if (cmd_split[0] == "Array")
            {
                if (dec_arr.IsMatch(cmd))
                {
                    HandleArray(cmd);
                    //Сплитами парсим массив и выделяем все значения
                    //Добавляем в variables и output
                }
            }
            else
            {
                if (dec_var.IsMatch(cmd))
                {
                    //Добавляем переменную и её значение в variables
                    //Добавляем в output данные
                    string var_name = cmd_split[0];
                    if (variables.ContainsKey(var_name))
                        return false;
                    int var_value = Convert.ToInt32(ReplaceVariableToValue(cmd_split[2]));

                    if (var_name != "m")
                    {
                        if (Math.Pow(2, (double)m) - 1 < var_value)
                        {
                            return false;
                        }
                        else
                        {
                            variables.Add(var_name, var_value);
                        }
                    }
                    else
                    {
                        return false; // overflow
                    }
                }
                return true;
            }
            return false; // Ошибка в синтаксисе
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
            for (i = 3; i < chars.Length; i++)
            {
                chars[i] = ReplaceVariableToValue(chars[i]);
            }

        }

        private string ReplaceVariableToValue(string str)
        {
            MatchCollection matches = var.Matches(str);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                    str = str.Replace(match.Value, variables[match.Value].ToString());
            }
            return str;
        }

        private string GetArrayName(string arr) //Подаем сюда chars[0]
        {
            string name = arr.Replace(" ", "");
            return name.Replace("Array", "");
         
        }

        private decimal GetLenghtExp(string arr) //Подаем сюда chars[1]
        {
            arr = arr.Replace(":0", "");
            MatchCollection matches = var.Matches(arr);
            arr = ReplaceVariableToValue(arr);
            return p.result(arr) + 1;
        }

        private bool AddToOutput(int i)
        {
            string str = i.ToString();

            for (int j = str.Length; j < m; j++)
            {
                str = "0" + str;
            }

            output = str + "\n";
            return true;
        }

    }

}
