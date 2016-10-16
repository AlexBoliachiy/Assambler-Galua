using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using DigitsPower;

namespace BinaryParserGui
{
    public class CodeGenerator
    {
        Regex add_regex = new Regex(@"ADD\s+R[0-3]\s*,\s*R[0-3]\s*$"); //Для большей читаемости кода я предпочел напилить кучу реджексов
        Regex mult_regex = new Regex(@"MUL\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex div_regex = new Regex(@"DIV\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex pow_regex = new Regex(@"POW\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex invm_regex = new Regex(@"INVM\s+R[0-3]\s*$");
        Regex inva_regex = new Regex(@"INVA\s+R[0-3]\s*$");
        Regex cdp_regex = new Regex(@"CDP\s+R[0-3]\s*$");
        Regex cpd_regex = new Regex(@"CPD\s+R[0-3]\s*$");
        Regex mov_regex = new Regex(@"MOV\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex mov_a_regex = new Regex(@"MOV\s+((R[0-3]\s*,\s*[A-Za-z_]+[A-Z_a-z0-9]*)|([A-Za-z_]+[A-Z_a-z0-9]*\s*,\s*R[0-3]))\s*$");
        Regex mov_array_regex = new Regex(@"MOV\s*((R[0-3]\s*,\s*[A-Za-z_]+[A-Z_a-z0-9]*\s*\[\s*AC[0-3]\s*([+-]\s*\d+)?\s*\])|([A-Za-z_]+[A-Z_a-z0-9]*\s*\[\s*AC[0-3]\s*([+-]\s*\d+)?\s*\]\s*,\s*R[0-3]))\s*$"); //
        Regex jmp_regex1 = new Regex(@"JMP\s+[A-Za-z]\w*\s*$");
        Regex jmp_regex2 = new Regex(@"JMP\s+R[0-2]\s*,\s*[A-Za-z]\w*\s*$");
        Regex loop_regex = new Regex(@"LOOP\s+[0-3]\s*,\s*((b'[01]+)|(h'[0-F]+)|([A-Za-z_]+[A-Z_a-z0-9]*)|(\d+))\s*(\s*[+/*-]\s*((b'[01]+)|(h'[0-F]+)|([A-Za-z_]+[A-Z_a-z0-9]*)|(\d+)))*\s*$");
        Regex end_loop_regex = new Regex(@"END_LOOP\s+[0-3]\s*$");
        Regex load_ca_regex = new Regex(@"LOAD\s+AC[0-3]\s*,\s*AC[0-3]\s*$");
        Regex load_ca_a_regex = new Regex(@"LOAD\s+AC[0-3]\s*,\s*((b'[01]+)|(h'[0-F]+)|([A-Za-z_]+[A-Z_a-z0-9]*)|(\d+))\s*(\s*[+/*-]\s*((b'[01]+)|(h'[0-F]+)|([A-Za-z_]+[A-Z_a-z0-9]*)|(\d+)))*\s*$");
        Regex inc_regex = new Regex(@"INC\s+((AC)|(R))[0-3]\s*$");
        Regex dec_regex = new Regex(@"DEC\s+((AC)|(R))[0-3]\s*$");
        Regex out_regex = new Regex(@"OUT\s+([A-Za-z_]+[A-Z_a-z0-9]*)\s*(\[AC[0-3]\s*([+-]\s*\d+)?\s*\])?\s*$");//
        Regex sub_regex = new Regex(@"SUB\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex label = new Regex(@"[A-z]\w*\s*:");
        string output;
        Memory mem;
        string[] outputs = new String[5];
        int CurrentLine = 0;
        int CurrentOutput = 0;
        int[] EnterToCycle = new int[4];
        Dictionary<int, bool> servedLoopsValue = new Dictionary<int, bool>() { { 0, false }, { 1, false }, { 2, false }, { 3, false } };
        Dictionary<string, int> labels = new Dictionary<string, int>();
        Stack<int> closingValue = new Stack<int>();
        public int rowCountData = 0;
        public Dictionary<int, string> comments = new Dictionary<int, string>();
        public bool gf = false;
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
                throw new CompilationException("При конвертуванні числа " + value.ToString() + " у двійкову послідовність трапилося переповнення");
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
                if (CurrentCmd.Length == 0 || CurrentCmd[0] == "CODE")
                {
                    i++;
                    continue;
                }
                if (label.IsMatch(currentStrCmd))
                {
                    string labelName = currentStrCmd.Replace(" ", string.Empty).Replace(":", string.Empty);
                    if (labels.ContainsKey(labelName))
                        throw new CompilationException("Повторна декларація мітки " + labelName);
                    labels.Add(labelName, CurrentLine);
                    continue;
                }

                string[] ops = GetOperands(x);
                
                switch (CurrentCmd[0])
                {
                    case "ADD": // В оригинале названо add_sub, но в коде почему-то эта Команда  ни разу не использовалась, так что я назвал её так
                        if (!add_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        ADD(ops[0], ops[1]);
                        break;
                    case "MUL":
                        if (!mult_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        MULT(ops[0], ops[1]);
                        break;
                    case "DIV":
                        if (!div_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        DIV(ops[0], ops[1]);
                        break;
                    case "POW":
                        if (!pow_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        POW(ops[0], ops[1]);
                        break;
                    case "INVM":
                        if (!invm_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        INV_(ops[0]);
                        break;
                    case "CDP":
                        if (!cdp_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        CDP(ops[0]);
                        break;
                    case "CPD":
                        if (!cpd_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        CPD(ops[0]);
                        break;
                    case "MOV":
                        bool suc = false;
                        if (mov_regex.IsMatch(currentStrCmd))
                        {
                            MOV(ops[0], ops[1]);
                            suc = true;
                        }
                        else if (mov_a_regex.IsMatch(currentStrCmd))
                        {
                            MOV_A(ops[0], ops[1]);
                            suc = true;
                        }

                        else if (mov_array_regex.IsMatch(currentStrCmd))
                        {
                            MOV_ARRAY(ops[0], ops[1]);
                            suc = true;
                        }

                        if (suc == false)
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        
                        break;
                    case "JMP":
                        if (jmp_regex1.IsMatch(currentStrCmd))
                            JMP(ops[0]);
                        else if (jmp_regex2.IsMatch(currentStrCmd))
                            JMPconditional(ops[0], ops[1]);
                        else
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                     
                        break;
                    case "LOOP":
                        if (!loop_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0] + " " + x);
                        LOOP(ops[0], ops[1]);
                        break;
                    case "LOAD":
                        bool suce = false;
                        if (load_ca_regex.IsMatch(currentStrCmd))
                        {
                            LOAD_CA(ops[0], ops[1]);
                            suce = true;
                        }
                        else if (load_ca_a_regex.IsMatch(currentStrCmd))
                        {
                            LOAD_CA_A(ops[0], ops[1]);
                        }
                        else
                        {
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        }            
                        break;
                    case "INC":
                        if (!inc_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        INC_DEC(ops[0], "0");
                        break;
                    case "DEC":
                        if (!inc_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        INC_DEC(ops[0], "1");
                        break;
                    case "OUT":
                        if (!out_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        OUT(ops[0]);
                        break;
                    case "END_LOOP":
                        if (!end_loop_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        END_LOOP(ops[0]);
                        break;
                    case "SUB":
                        if (!sub_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        SUB(ops[0], ops[1]);
                        break;
                    case "INVA":
                        if(!inva_regex.IsMatch(currentStrCmd))
                            throw new CompilationException("Помилка в синтаксисі коду команди у рядку номер " + (i + rowCountData).ToString() + "\nКоманда " + CurrentCmd[0]);
                        INVA(ops[0]);
                        break;


                    case "/*":
                        throw new CompilationException("Незакритий коментар у рядку  :" + (i + rowCountData).ToString());
                    default:
                        throw new CompilationException("Невідома команда \"" + currentStrCmd.Trim() + "\" у рядку " + (i + rowCountData).ToString());


                }
                i++;
            }
            if (closingValue.Count != 0)
            {
                throw new CompilationException("Наявний незакритий цикл з лічільником номер " + closingValue.Pop());
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
                throw new CompilationException("Команда без параметрів!");
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
            outputs[CurrentOutput] += "0000" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2);
            comments.Add(CurrentLine, "// " + "ADD " + R0 + ", " + R1);
            CurrentLine++;
        }

        private void MULT(string R0, string R1)
        {
            outputs[CurrentOutput] += "0001" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2); ;
            comments.Add(CurrentLine, "// " + "MUL " + R0 + ", " + R1);
            CurrentLine++;
        }
        private void DIV(string R0, string R1)
        {
            outputs[CurrentOutput] += "0010" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2);
            comments.Add(CurrentLine, "// " + "DIV " + R0 + ", " + R1);
            CurrentLine++;
        }

        private void POW(string R0, string R1)
        {
            outputs[CurrentOutput] += "0011" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2);
            comments.Add(CurrentLine, "// " + "POW " + R0 + ", " + R1);
            CurrentLine++;
        }
        private void INV_(string R0)
        {
            outputs[CurrentOutput] += "0100" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "00"; //В доке внятно несказанно, что должно дописываться в неиспользованные байты
            comments.Add(CurrentLine, "// " + "INV " + R0 + ", ");
            CurrentLine++;
        }

        private void CDP(string R0)
        {
            if (gf)
                throw new CompilationException("Не можна використовувати команду CDP при директиві GF(p)");
            outputs[CurrentOutput] += "0101" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "00"; //В доке внятно несказанно, что должно дописываться в неиспользованные байты
            comments.Add(CurrentLine, "// " + "CDP " + R0);
            CurrentLine++;
        }

        private void CPD(string R0)
        {
            if (gf)
                throw new CompilationException("Не можна використовувати команду CPD при директиві GF(p)");
            outputs[CurrentOutput] += "0110" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "00"; //В доке внятно несказанно, что должно дописываться в неиспользованные байты
            comments.Add(CurrentLine, "// " + "CPD " + R0 + ", ");
            CurrentLine++;
        }

        private void MOV(string R0, string R1)
        {
            outputs[CurrentOutput] += "0111" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2);
            comments.Add(CurrentLine, "// " + "MOV " + R0 + ", " + R1);
            CurrentLine++;
        }

        private void MOV_A(string R0, string R1)
        {
            if (R0 == "R0" || R0 == "R1" || R0 == "R2" || R0 == "R3")
                outputs[CurrentOutput] += "1000" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "1" + mem.GetBinaryAdress(R1);
            else
            {
                if (mem.GetType(R0) == TYPE.cons)
                {
                    throw new CompilationException("Спроба запису у константу у команді MOV_A " + R0 + ",  " + R1);
                }
                outputs[CurrentOutput] += "1000" + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "0" + mem.GetBinaryAdress(R0);
            }
            comments.Add(CurrentLine, "// " + "MOV " + R0 + ", " + R1);
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
                    string ca = ConvertToBinary(Convert.ToInt32(exp[3].ToString()), 2);
                    exp = exp.Substring(4);
                    string offset = exp.Remove(exp.IndexOf(']'), 1);
                    string sign;
                    if (offset[0] == '+')
                        sign = "0";
                    else 
                        sign = "1";
                    if (offset == string.Empty)
                        offset = "0";
                    else
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
                    outputs[CurrentOutput] += "1001" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "1" + mem.GetBinaryAdress(arrName) + ca + "0" + sign + binaryOffset;
                }
                else
                {
                    string arrName = R1.Remove(R1.IndexOf('['));
                    string exp = R1.Substring(R1.IndexOf('['));
                    string ca = ConvertToBinary(Convert.ToInt32(exp[3].ToString()), 2);
                    exp = exp.Substring(4);
                    string offset = exp.Remove(exp.IndexOf(']'), 1);
                    outputs[CurrentOutput] += "1001" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "1" + mem.GetBinaryAdress(arrName) + ca + "0" + "1" + "0000";
                }
            }

            else
            {
                if (R0.Contains("+") || R0.Contains("-"))
                {
                    string arrName = R0.Remove(R0.IndexOf('['));
                    string exp = R0.Substring(R0.IndexOf('['), R0.LastIndexOf(']') - R0.IndexOf('[') + 1);
                    string ca = ConvertToBinary(Convert.ToInt32(exp[3].ToString()), 2);
                    exp = exp.Substring(4);
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
                    outputs[CurrentOutput] += "1001" + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "0" + mem.GetBinaryAdress(arrName) + ca + "0" + sign + binaryOffset;
                }
                else
                {
                    string arrName = R0.Remove(R0.IndexOf('['));
                    string exp = R0.Substring(R0.IndexOf('['), R0.LastIndexOf(']') - R0.IndexOf('[') + 1);
                    string ca = ConvertToBinary(Convert.ToInt32(exp[3].ToString()), 2);
                    outputs[CurrentOutput] += "1001" + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2) + "0" + mem.GetBinaryAdress(arrName) + ca + "0" + "1" + "0000";
                }
            }
            comments.Add(CurrentLine, "// " + "MOV " + R0 + ", " + R1);
            CurrentLine += 3;
        }
        private void JMP(string R0)
        {

            int line;
            try
            {
                line = labels[R0];
            }
            catch
            {
                throw new CompilationException("JMP з невідомою міткою " + R0);
            }
            outputs[CurrentOutput] += "1010" + "11" + "1";
            outputs[CurrentOutput] += ConvertToBinary(line, 9);
            comments.Add(CurrentLine, "// " + "JMP " + R0);


        }
        private void JMPconditional(string R0, string R1)
        {

            int line;
            try
            {
                line = labels[R1];
            }
            catch
            {
                throw new CompilationException("JMP з невідомою міткою " + R1);
            }
            outputs[CurrentOutput] += "1010" + ConvertToBinary(Convert.ToInt32(R0.Substring(1)), 2) + "1"; 
            outputs[CurrentOutput] += ConvertToBinary(line, 9);
            comments.Add(CurrentLine, "// " + "JMP " + R0 + ", " + R1);
        }

        private void LOAD_CA(string R0, string R1)
        {
            outputs[CurrentOutput] += "1100" + ConvertToBinary(Convert.ToInt32(R0[2].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[2].ToString()), 2);
            comments.Add(CurrentLine, "// " + "LOAD " + R0 + ", " + R1);
            CurrentLine++;
        }

        private void LOAD_CA_A(string R0, string R1) //OK 1
        {
            int i = Convert.ToInt32(R0[2].ToString());
            string CA = ConvertToBinary(i, 2);
            string A = string.Empty;
            int Aint = mem.ExpressionToInt(R1, false);

            A = ConvertToBinary(Aint, 9);
            outputs[CurrentOutput] += "1101" + CA + "0" + A;
            comments.Add(CurrentLine, "// " + "LOAD " + R0 + ", " + R1);
            CurrentLine += 2;
        }

        private void INC_DEC(string R0, string R1)
        {
            if (R0[0] == 'R')// If register
            {
                outputs[CurrentOutput] += "1110" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "0" + R1;
            }
            else // if CA
            {
                outputs[CurrentOutput] += "1110" + ConvertToBinary(Convert.ToInt32(R0[2].ToString()), 2) + "1" + R1;
            }
            comments.Add(CurrentLine, "// " + ( R0=="0" ? "INC":"DEC") + R0 + ", " + R1);
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
                    outputs[CurrentOutput] += "00" + "1" + mem.GetBinaryAdress(arrName) + ca + "0" + sign + binaryOffset;
                    comments.Add(CurrentLine, "// " + "OUT " + R0 + ", ");
                    CurrentLine += 3;
                }
                else
                {
                    string arrName = R0.Remove(R0.IndexOf('['));
                    string exp = R0.Substring(R0.IndexOf('['));
                    string ca = ConvertToBinary(Convert.ToInt32(exp[3].ToString()), 2);
                    outputs[CurrentOutput] += "00" + "1" + mem.GetBinaryAdress(arrName) + ca + "0" + "1" + "0000";
                    comments.Add(CurrentLine, "// " + "OUT " + R0);
                    CurrentLine += 3;
                }
            }
            else
            {
                outputs[CurrentOutput] += "000" + mem.GetBinaryAdress(R0);
                comments.Add(CurrentLine, "// " + "OUT " + R0 );
                CurrentLine += 2;
            }
        }

        private void LOOP(string R0, string R1)
        {
            int r;
            if (!Int32.TryParse(R0, out r))
                throw new CompilationException("Значення лічильника циклу або відсутнє або некоректне!");
            if (servedLoopsValue[Convert.ToInt32(R0)])
                throw new CompilationException("Використання лічильника, що вже використовується та ще незакритий! Номер лічильника " + R0);
            outputs[CurrentOutput] += "1011" + ConvertToBinary(Convert.ToInt32(R0), 2) + "0" + mem.GetBinaryAdress(R1);
            closingValue.Push(Convert.ToInt32(R0));
            servedLoopsValue[Convert.ToInt32(R0)] = true;
            CurrentOutput++;
            outputs[CurrentOutput - 1] += output;
            output = string.Empty;
            EnterToCycle[CurrentOutput] = CurrentLine;
            comments.Add(CurrentLine, "// " + "LOOP " + R0 + ", " + R1);
            CurrentLine += 4;

        }

        private void END_LOOP(string R0)
        {
            if (closingValue.Count == 0)
                throw new CompilationException("Закриття циклу є, а початку немає (закриття лічильника під номером! " + R0);
            if (closingValue.Pop() != Convert.ToInt32(R0))
            {
                throw new CompilationException("Несподіваний кінець циклу номер або використання зайнятої змінної лічильника! Номер лічильнику " + R0);
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
            comments.Add(CurrentLine, "// END_LOOP " + R0);
            CurrentLine += 2;
        }

        private void SUB(string R0, string R1)
        {
            if (!gf)
                throw new CompilationException("Команду SUB можно використовувати тільки у режимі GF(p)");
            outputs[CurrentOutput] += "0110" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + ConvertToBinary(Convert.ToInt32(R1[1].ToString()), 2);
            comments.Add(CurrentLine, "// " + "SUB " + R0 + ", " + R1);
            CurrentLine++;
        }

        private void INVA(string R0)
        {
            if (!gf)
                throw new CompilationException("Команду INVA можно використовувати тільки у режимі GF(p)");
            outputs[CurrentOutput] += "0101" + ConvertToBinary(Convert.ToInt32(R0[1].ToString()), 2) + "00"; //В доке внятно несказанно, что должно дописываться в неиспользованные байты
            comments.Add(CurrentLine, "// " + "INVA " + R0 + ", ");
            CurrentLine++;
        }
    }

}

            
    
