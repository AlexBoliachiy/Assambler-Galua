using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BinaryParser
{
    enum TYPE
    {
        cons,
        vars,
        arrs
    }

    class Memory
    {
        private int m = -1;
        public string output = String.Empty;
        public string output_var = String.Empty;
        public string output_con = String.Empty;
        public string output_arr = String.Empty;
        private Dictionary<string, string> variables = new Dictionary<string, string>(); // Имя переменной : двоичный адресс
        private Dictionary<string, int >variables_values = new Dictionary<string, int>(); // Имя переменной : значение
        private PostfixNotationExpression p = new PostfixNotationExpression();
        private Regex dec_con = new Regex(@"^const\s+[A-Za-z_]+[A-Z_a-z0-9]*\s*=\s*\d+\s*$");
        private Regex dec_arr = new Regex(@"^Array\s+[A-Za-z_]+[A-Z_a-z0-9]*\s*\[\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*[+\-*/]\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*:\s*0\]\s*=\s*\(\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*,\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))*\s*\)\s*$");
        private Regex dec_var = new Regex(@"^[A-Za-z_]+[A-Z_a-z0-9]*\s*=\s*\d+\s*$");
        private Regex var = new Regex(@"\s*[A-Za-z_]+[A-Z_a-z0-9]*\s*");
        private int line;
        private List<string> ConstAdresses = new List<string>();

        public Memory()
        {

        }

        public void Gather()
        {
            output = output_con + output_var + output_arr;  
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
                            return false; //Переполнение стека
                        }
                        else
                        {
                            variables.Add(const_name, GetBinaryAdress(line));
                            variables_values.Add(const_name, const_value);
                            AddToOutput(const_value,ref output_con);
                            
                            ConstAdresses.Add(GetBinaryAdress(line));
                            line++;
                        }
                    }
                    else
                    {
                        if (m == -1)
                            m = const_value;
                        else
                            throw new ArgumentNullException("Повторное объявление m");
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
                            variables.Add(var_name, GetBinaryAdress(line));
                            variables_values.Add(var_name, var_value);
                            AddToOutput(var_value,ref output_var);
                            line++;

                        }
                    }
                    else
                    {
                        return false; // overflow
                    }
                    return true;
                }
                else
                    return false; // Ошибка в синтаксисе
                
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
            string ArrayName = GetArrayName(chars[0]);
            decimal lenght = GetLenghtExp(chars[1]);
            int CntOfVal = chars.Length; // тут и ниже начианаются некоторые проблемы, ибо сплит почему-то иногда возвращает пустую строку
            if (chars.Last() == "")
                CntOfVal--;
            if (CntOfVal - 3 != lenght)
            {
                throw new Exception("не все ячейки массива заполнены");
            }
            variables.Add(ArrayName, GetBinaryAdress(line));
            for (i = 3; i < CntOfVal; i++)
            {
                chars[i] = GetBinaryInt(Convert.ToInt32(ReplaceVariableToValue(chars[i])));
                AddToOutput(Convert.ToInt32(chars[i], 2),ref output_arr);
                line++;
            }

            

        }

        private string ReplaceVariableToValue(string str)
        {
            MatchCollection matches = var.Matches(str);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                    str = str.Replace(match.Value, variables_values[match.Value].ToString());
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
            arr = ReplaceVariableToValue(arr);
            return p.result(arr) + 1;
        }
        //Добавляет в файл памяти указанную переменную
        private bool AddToOutput(int i, ref string Output)
        {
            string str = GetBinaryInt(i);
            Output += str + "\n";
            return true;
        }
        
        // Возвращает бинарное число длинной m
        private string GetBinaryInt(int i)  
        {
            string str = Convert.ToString(i, 2);

            for (int j = str.Length; j < m; j++)
            {
                str = "0" + str;
            }
            return str;
        }
        // Возвращает бинарное адресс длиной 9 (Это не магическое число в мануале указанно, что максимальная длинна адреса 511 ( 2**9 - 1))
        private string GetBinaryAdress(int i)
        {
            string str = i.ToString();

            for (int j = str.Length; j < 9; j++)
            {
                str = "0" + str;
            }
            return str;
        }

        private string GetValue(int adress)
        {
            string[] out_split = output.Split('\n');
            if (adress >= out_split.Length)
            {
                throw new ArgumentException("обращение к невыделенной памяти");
            }
            else
                return out_split[adress];
        }

        private string GetValue(string adress)
        {
            string[] out_split = output.Split('\n');
            if (Convert.ToInt32(adress, 2) >= out_split.Length)
            {
                throw new ArgumentException("обращение к невыделенной памяти");
            }
            else
                return out_split[Convert.ToInt32(adress, 2)];
        }


        private bool SetValue(string adress, string value)
        {
            if (ConstAdresses.Contains(adress))
                throw new ArgumentException("Попытка записать в константу значение");

            throw new NotImplementedException();
            //Вроде бы ненужный метод. Стереть

        }

    }

}
