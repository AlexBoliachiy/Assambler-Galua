using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BinaryParserGui
{
    public class CodeGenerator
    {
        Regex add_regex = new Regex(@"ADD\s+R[0-3]\s*,\s*R[0-3]\s*$"); //Для большей читаемости кода я предпочел напилить кучу реджексов
        Regex mult_regex = new Regex(@"MULT\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex div_regex = new Regex(@"DIV\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex pow_regex = new Regex(@"POW\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex inv_regex = new Regex(@"INV_\s+R[0-3]\s*$");
        Regex cdp_regex = new Regex(@"CDP\s+R[0-3]\s*$");
        Regex cpd_regex = new Regex(@"CPD\s+R[0-3]\s*$");
        Regex mov_regex = new Regex(@"MOV\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex mov_a_regex = new Regex(@"MOV_A\s+((R[0-3]\s*,\s*[A-Za-z_]+[A-Z_a-z0-9]*)|([A-Za-z_]+[A-Z_a-z0-9]*\s*,\s*R[0-3]))\s*$");
        Regex mov_array_regex = new Regex(@"MOV_ARRAY\s*((R[0-3]\s*,\s*[A-Za-z_]+[A-Z_a-z0-9]*\s*\[\s*CA_[0-3]\s*([+-]\s*\d+)?])|([A-Za-z_]+[A-Z_a-z0-9]*\s*\[\s*CA_[0-3]\s*([+-]\s*\d+)?]\s*,\s*R[0-3]))\s*$"); //
        Regex jmp_regex = new Regex(@"JMP\s+(R[0-3]\s*,\s*)?\s*(([A-Za-z_]+[A-Z_a-z0-9]*)|([0-9]{9}))\s*$");
        Regex loop_regex = new Regex(@"LOOP\s+[0-3]\s*,\s*(((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+)))\s*(\s*[+\-*/]\s*((([A-za-z_]+[A-Z_a-z0-9]*)|(\d+))))+)|(h'[0-F]+)|(b'[01]+)\s*$");
        Regex end_loop_regex = new Regex(@"END_LOOP\s+[0-3]\s*$");
        Regex load_ca_regex = new Regex(@"LOAD_CA\s+CA_[0-3]\s*,\s*CA_[0-3]\s*$");
        Regex load_ca_a_regex = new Regex(@"LOAD_CA_A\s+CA_[0-3]\s*,\s*((b'[0-1]+)|(h'[0-F]+)|(\d+))\s*$");
        Regex inc_dec_regex = new Regex(@"INC_DEC\s+((CA_)|(R))[0-3]\s*,\s*[0-1]\s*$");
        Regex out_regex = new Regex(@"OUT\s+([A-Za-z_]+[A-Z_a-z0-9]*)\s*(\[CA_[0-3]\s*([+-]\s*\d+)?\])?\s*$");//
        string output;
        Memory mem;
        string[] outputs = new String[5];
        int CurrentLine = 0;
        int CurrentOutput = 0;
        int[] EnterToCycle = new int[4];
        Dictionary<int, bool> servedLoopsValue = new Dictionary<int, bool>() { { 0, false }, { 1, false }, { 2, false }, { 3, false } };
        Stack<int> closingValue = new Stack<int>();
        public int rowCountData = 0;

        public CodeGenerator(Memory mem)
        {

            this.mem = mem;
            for (int i = 0; i < outputs.Length; i++)
            {
                outputs[i] = string.Empty;
            }
            output = outputs[0];
        }

        public string Code { get { return outputs[0]; } }

        private string ConvertToBinary(int value, int rank)
        {
            string binary = Convert.ToString(value, 2);
            if (binary.Length > rank)
            {
                throw new CompilationException("При конвертуванні числа " +  value.ToString() + " у двійкову послідовність трапилося переповнення");
            }
            while (binary.Length < rank)
            {
                binary = "0" + binary;
            }
            return binary;
        }


        public void HandleCodeSection(string[] Code)
        {
            int i = 0;
            foreach (string x in Code)
            {
                string currentStrCmd = x.Replace("\t", " ");
                string[] CurrentCmd = currentStrCmd.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if ( CurrentCmd.Length == 0 || CurrentCmd[0] == "CODE" )
                {
                    i++;
                    continue;
                }
                    
                string[] ops = GetOperands(x);
                switch (CurrentCmd[0])
                {
                    case "ADD": // В оригинале названо add_sub, но в коде почему-то эта команда ни разу не использовалась, так что я назвал её так
                        if (!add_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        ADD(ops[0], ops[1]);
                        break;
                    case "MULT":
                        if (!mult_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        MULT(ops[0], ops[1]);
                        break;
                    case "DIV":
                        if (!div_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        DIV(ops[0], ops[1]);
                        break;
                    case "POW":
                        if (!pow_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  : - " + (i + rowCountData).ToString());
                        POW(ops[0], ops[1]);
                        break;
                    case "INV_":
                        if (!inv_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        INV_(ops[0]);
                        break;
                    case "CDP":
                        if (!cdp_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        CDP(ops[0]);
                        break;
                    case "CPD":
                        if (!cpd_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        CPD(ops[0]);
                        break;
                    case "MOV":
                        if (!mov_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        MOV(ops[0], ops[1]);
                        break;
                    case "MOV_A":
                        bool success = false;
                        if (mov_a_regex.IsMatch(currentStrCmd))
                        { 
                            MOV_A(ops[0], ops[1]);
                            success = true;
                        }
                        

                        if (!success)
                        {
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        }

                        break;
                    case "MOV_ARRAY":
                        bool suc = false;
                        if (mov_array_regex.IsMatch(currentStrCmd))
                        {
                            MOV_ARRAY(ops[0], ops[1]);
                            suc = true;
                        }

                        if (!suc)
                        {
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        }
                        break;
                    case "JMP":
                        if (!jmp_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        JMP(ops[0], ops[1]);
                        break;
                    case "LOOP":
                        if (!loop_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString() + " " + x);
                        LOOP(ops[0], ops[1]);
                        break;
                    case "LOAD_CA":
                        if (!load_ca_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        LOAD_CA(ops[0], ops[1]);
                        break;
                    case "LOAD_CA_A":
                        if (!load_ca_a_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        LOAD_CA_A(ops[0], ops[1]);
                        break;
                    case "INC_DEC":
                        if (!inc_dec_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        INC_DEC(ops[0], ops[1]);
                        break;
                    case "OUT":
                        if (!out_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        OUT(ops[0]);
                        break;
                    case "END_LOOP":
                        if (!end_loop_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка у синтаксисі коду команд, команда номер  :" + (i + rowCountData).ToString());
                        END_LOOP(ops[0]);
                        break;
                    case "/*":
                        throw new CompilationException("Незакритий коментар у рядку  :" + (i + rowCountData).ToString());
                    default:
                        throw new CompilationException("Невідома команда " + currentStrCmd + " у рядку " + (i + rowCountData).ToString());
                        
                        
                }
                i++;
            }
        }
        private string[] GetOperands(string raw)
        {
            raw = raw.Replace("\t", " ");
            while (raw[0] == ' ')
                raw = raw.Substring(1);
            string ops = string.Empty;
            try
            {
                ops = raw.Substring(raw.IndexOf(' '));
            }
            catch
            {
                throw new CompilationException("one word command");
            }
            ops = ops.Replace(" ", string.Empty);
            return ops.Split(',');
        }
        private bool IsNumber(string str)
        {
            int n;
            return int.TryParse(str, out n);
        }
        private void ADD(string R0, string R1)
        {
            outputs[CurrentOutput] += "0000" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "{ " + "ADD " + R0 + "," + R1 + "}";
            CurrentLine++;
        }

        private void MULT(string R0, string R1)
        {
            outputs[CurrentOutput] += "0001" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "{ " + "MULT " + R0 + "," + R1 + "}"; ;
            CurrentLine++;
        }
        private void DIV(string R0, string R1)
        {
            outputs[CurrentOutput] += "0010" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "{ " + "DIV " + R0 + "," + R1 + "}"; ;
            CurrentLine++;
        }

        private void POW(string R0, string R1)
        {
            outputs[CurrentOutput] += "0011" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "{ " + "POW " + R0 + "," + R1 + "}"; ;
            CurrentLine++;
        }
        private void INV_(string R0)
        {
            outputs[CurrentOutput] += "0100" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "00" + "{ " + "INV " + R0 + "," +  "}"; ; //В доке внятно несказанно, что должно дописываться в неиспользованные байты
            CurrentLine++;
        }

        private void CDP(string R0)
        {
            outputs[CurrentOutput] += "0101" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "00" + "{ " + "CDP " + R0 + "," + "}"; ; //В доке внятно несказанно, что должно дописываться в неиспользованные байты
            CurrentLine++;
        }

        private void CPD(string R0)
        {
            outputs[CurrentOutput] += "0110" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "00" + "{ " + "CPD " + R0 + "," + "}"; ; //В доке внятно несказанно, что должно дописываться в неиспользованные байты
            CurrentLine++;
        }

        private void MOV(string R0, string R1)
        {
            outputs[CurrentOutput] += "0111" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "{ " + "MOV " + R0 + "," + R1 + "}"; ;
            CurrentLine++;
        }

        private void MOV_A(string R0, string R1)
        {
            if (R0 == "R0" || R0 == "R1" || R0 == "R2" || R0 == "R3")
                outputs[CurrentOutput] += "1000" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "1" + mem.GetBinaryAdress(R1) + "{ " + "MOV_A " + R0 + "," + R1 + "}"; 
            else
            {
                if ( mem.GetType(R0) == TYPE.cons)
                {
                    throw new CompilationException("Спроба запису у константу у команді MOV_A " + R0 + ",  " + R1 );
                }
                outputs[CurrentOutput] += "1000" + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "0" + mem.GetBinaryAdress(R0)+ "{ " + "MOV_A " + R0 + "," + R1 + "}";
            }
            CurrentLine += 2;
        }
        private void MOV_ARRAY(string R0, string R1)
        {
            if (R0 == "R0" || R0 == "R1" || R0 == "R2" || R0 == "R3")
            {
                if (R1.Contains("+") || R1.Contains("-"))
                {
                    string arrName = R1.Remove(R1.IndexOf('['));
                    string exp = R1.Substring(R1.IndexOf('['));
                    string ca = ConvertToBinary(Convert.ToInt32(exp[4].ToString()), 2);
                    exp = exp.Substring(5);
                    string offset = exp.Remove(exp.IndexOf(']'), 1);
                    string sign;
                    if (offset[0] == '+')
                        sign = "0";
                    else
                        sign = "1";
                    offset = offset.Substring(1);
                    string binaryOffset = ConvertToBinary(Convert.ToInt32(offset), 2);
                    if (binaryOffset.Length > 4)
                    {
                        throw new CompilationException("Значение смещения больше 4-х байт");
                    }
                    while (binaryOffset.Length != 4)
                    {
                        binaryOffset = "0" + binaryOffset;
                    }
                    outputs[CurrentOutput] += "1001" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "1" + mem.GetBinaryAdress(arrName) + ca + "0" + sign + binaryOffset + "{ " + "MOV_ARRAY " + R0 + "," + R1 + "}";
                }
                else
                {
                    string arrName = R1.Remove(R1.IndexOf('['));
                    string exp = R1.Substring(R1.IndexOf('['));
                    string ca = ConvertToBinary(Convert.ToInt32(exp[4].ToString()), 2);
                    exp = exp.Substring(5);
                    string offset = exp.Remove(exp.IndexOf(']'), 1);
                    outputs[CurrentOutput] += "1001" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "1" + mem.GetBinaryAdress(arrName) + ca + "0" + "1" + "0000" + "{ " + "MOV_ARRAY" + R0 + "," + R1 + "}";
                }
            }

            else
            {
                if (R0.Contains("+") || R0.Contains("-"))
                {
                    string arrName = R0.Remove(R0.IndexOf('['));
                    string exp = R0.Substring(R0.IndexOf('['), R0.LastIndexOf(']') - R0.IndexOf('[') + 1);
                    string ca = ConvertToBinary(Convert.ToInt32(exp[4].ToString()), 2);
                    exp = exp.Substring(5);
                    string offset = exp.Remove(exp.IndexOf(']'), 1);
                    string sign;
                    if (offset[0] == '+')
                        sign = "0";
                    else
                        sign = "1";
                    offset = offset.Substring(1);
                    string binaryOffset = Convert.ToString(Convert.ToInt32(offset), 2);
                    if (binaryOffset.Length > 4)
                    {
                        throw new CompilationException("Значення зміщення більще 4-х біт");
                    }
                    while (binaryOffset.Length != 4)
                    {
                        binaryOffset = "0" + binaryOffset;
                    }
                    outputs[CurrentOutput] += "1001" + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "0" + mem.GetBinaryAdress(arrName) + ca + "0" + sign + binaryOffset + "{ " + "MOV_ARRAY " + R0 + "," + R1 + "}";
                }
                else
                {
                    string arrName = R0.Remove(R0.IndexOf('['));
                    string exp = R0.Substring(R0.IndexOf('['), R0.LastIndexOf(']') - R0.IndexOf('[') + 1);
                    string ca = ConvertToBinary(Convert.ToInt32(exp[4].ToString()), 2);
                    outputs[CurrentOutput] += "1001" + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "0" + mem.GetBinaryAdress(arrName) + ca + "0" + "1" + "0000" + "{ " + "MOV_ARRAY " + R0 + "," + R1 + "}";
                }
            }
            CurrentLine += 3;
        }
        private void JMP(string R0, string R1)
        {
            if (R1 == string.Empty || R1 == null) // Значит безусловный переход
            {
                outputs[CurrentOutput] += "1010" + "11" + "1";
                
            }
            else
            {

                outputs[CurrentOutput] += "1010" + Convert.ToString(Convert.ToInt32(R0[1]), 2) + "0"; 
            }

            if (IsNumber(R0))
            {
                outputs[CurrentOutput] += R1;
            }
            else
                outputs[CurrentOutput] += mem.GetBinaryAdress(R0) + "{ " + "JMP " + R0 + "," + R1 + "}";
            CurrentLine += 2;
        }

     
        private void LOAD_CA(string R0, string R1)
        {
            outputs[CurrentOutput] += "1100" + ConvertToBinary(Convert.ToInt32(R0[3].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[3].ToString()), 2) + "{ " + "LOAD_CA " + R0 + "," + R1 + "}";
            CurrentLine++;
        }

        private void LOAD_CA_A(string R0, string R1) //OK 1
        {
            int i = Convert.ToInt32(R0[3].ToString());
            string CA = ConvertToBinary(i, 2);
            string A = string.Empty;
            int Aint = -1;
            try {
                if (char.IsDigit(R1[0]))
                    Aint= Convert.ToInt32(R1);
                else if (R1.Remove(2) == "b'") // Binary number
                    Aint = Convert.ToInt32(R1.Substring(2), 2);
                else if (R1.Remove(2) == "h'") //Hexadecimal number
                    Aint = Convert.ToInt32(R1.Substring(2), 16);
                else
                    throw new CompilationException("Число");
            }
                    catch
            {
                throw new CompilationException("Допишіть будь ласка значення змінної у команді LOAD_CA_A" + R0 + ", " + R1);
            }
            A = ConvertToBinary(Aint, 9);
            outputs[CurrentOutput] += "1101" + CA + "0" + A + "{ " + "LOAD_CA_A " + R0 + "," + R1 + "}"; 
            CurrentLine += 2;
        }

        private void INC_DEC(string R0, string R1)
        {
            if (R0[0] == 'R')// If register
            {
                outputs[CurrentOutput] += "1110" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "0" + R1 + "{ " + "INC_DEC" + R0 + "," + R1 + "}";
            }
            else // if CA
            {
                outputs[CurrentOutput] += "1110" + ConvertToBinary(Convert.ToInt32(R0[3].ToString()), 2) + "1" + R1 + "{ " + "INC_DEC " + R0 + "," + R1 + "}";
            }
            CurrentLine++;
        }

        private void OUT(string R0)
        {
           
            outputs[CurrentOutput] += "1111";
            if (R0.Contains("[")) // if array
            {
                if (R0.Contains("+") || R0.Contains("-"))
                {
                    string arrName = R0.Remove(R0.IndexOf('['));
                    string exp = R0.Substring(R0.IndexOf('['));
                    string ca = ConvertToBinary(Convert.ToInt32(exp[4].ToString()), 2);
                    exp = exp.Substring(5);
                    string offset = exp.Remove(exp.IndexOf(']'), 1);
                    string sign;
                    if (offset[0] == '+')
                        sign = "0";
                    else
                        sign = "1";
                    offset = offset.Substring(1);
                    string binaryOffset = Convert.ToString(Convert.ToInt32(offset), 2);
                    if (binaryOffset.Length > 4)
                    {
                        throw new CompilationException("Значення зміщення більще 4-х біт");
                    }
                    while (binaryOffset.Length != 4)
                    {
                        binaryOffset = "0" + binaryOffset;
                    }
                    outputs[CurrentOutput] += "00" + "1" + mem.GetBinaryAdress(arrName) + ca + "0" + sign + binaryOffset + "{" + "OUT " + R0 + "," + "}";
                    CurrentLine += 3;
                }
                else
                {
                    string arrName = R0.Remove(R0.IndexOf('['));
                    string exp = R0.Substring(R0.IndexOf('['));
                    string ca = ConvertToBinary(Convert.ToInt32(exp[4].ToString()), 2);
                    outputs[CurrentOutput] += "00" + "1" + mem.GetBinaryAdress(arrName) + ca + "0" + "1" + "0000" + "{" + "OUT " + R0 + "," + "}";
                    CurrentLine += 3;
                }
            }
            else
            {
                outputs[CurrentOutput] += "0" + mem.GetBinaryAdress(R0) + " {" + "OUT " + R0 + "," + "}";
                CurrentLine += 2;
            }
        }

        private void LOOP(string R0, string R1)
        {
            int r;
            if (!Int32.TryParse(R0, out r))
                throw new CompilationException("Значення лічильника циклу або відсутнє або некоректне!");
            if (servedLoopsValue[Convert.ToInt32(R0)])
                throw new CompilationException("Використання лічильника, що вже використовується та ще незакритий! Номер лічильника:" + R0);
            outputs[CurrentOutput] += "1011" + ConvertToBinary(Convert.ToInt32(R0), 2) + "0" + mem.GetBinaryAdress(R1) + " {" + "LOOP " + R0 + "," + R1 + "}";
            closingValue.Push(Convert.ToInt32(R0));
            servedLoopsValue[Convert.ToInt32(R0)] = true;
            CurrentOutput++;
            outputs[CurrentOutput-1] += output;
            output = string.Empty;
            EnterToCycle[CurrentOutput] = CurrentLine;
            CurrentLine += 4;
            
        }
        private void END_LOOP(string R0)
        {
            if (closingValue.Count == 0)
                throw new CompilationException("Закриття циклу є, а початку немає (закриття лічильника під номером! " + R0);
            if (closingValue.Pop() != Convert.ToInt32(R0))
            {
                throw new CompilationException("Несподіваний кінець циклу номер або використання зайнятої змінної лічильника!" + R0);
            }
            servedLoopsValue[Convert.ToInt32(R0)] = false;
            string str = Convert.ToString(CurrentLine + 2, 2);

            for (int j = str.Length; j < 9; j++)
            {
                str = "0" + str;
            }

            outputs[CurrentOutput - 1] += "1010" + ConvertToBinary(Convert.ToInt32(R0), 2) + "0" + str; 
            outputs[CurrentOutput] += "1010" + "11" + "1" + ConvertToBinary(EnterToCycle[CurrentOutput] + 2, 9);
            CurrentOutput--;
            outputs[CurrentOutput] += outputs[CurrentOutput + 1];
            outputs[CurrentOutput + 1] = string.Empty;

            CurrentLine += 2;
        }

    }
            
}
