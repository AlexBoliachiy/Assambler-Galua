using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BinaryParser
{
    class CodeGenerator
    {
        Regex add_regex = new Regex(@"ADD\s+R[0-3]\s*,\s*R[0-3]\s*$"); //Для большей читаемости кода я предпочел напилить кучу реджексов
        Regex mult_regex = new Regex(@"MULT\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex div_regex = new Regex(@"DIV\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex pow_regex = new Regex(@"POW\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex inv_regex = new Regex(@"INV_\s+R[0-3]\s*$");
        Regex cdp_regex = new Regex(@"CDP\s+R[0-3]\s*$");
        Regex cpd_regex = new Regex(@"CPD\s+R[0-3]\s*$");
        Regex mov_regex = new Regex(@"MOV\s+R[0-3]\s*,\s*R[0-3]\s*$");
        Regex mov_a_regex = new Regex(@"MOV_A\s+((R[0-3]\s*,\s*[A-Za-z_]+[A-Z_a-z0-9]*)|([A-Za-z_]+[A-Z_a-z0-9]*\s*,\s*R[0-3]))$");
        Regex mov_array_regex = new Regex(@"MOV_A\s*((R[0-3]\s*,\s*[A-Za-z_]+[A-Z_a-z0-9]*\[\s*CA_[0-3]\s*[+-]\s*\d+])|([A-Za-z_]+[A-Z_a-z0-9]*\[\s*CA_[0-3]\s*[+-]\s*\d+]\s*,\s*R[0-3]))$");




        public void HandleCodeSection(Memory mem, string[] Code)
        {
            string output = String.Empty;
            foreach (string x in Code)
            {
                string[] CurrentCmd = x.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                switch (CurrentCmd[0])
                {
                    case "ADD": // В оригинале названо add_sub, но в коде почему-то эта команда ни разу не использовалась, так что я назвал её так
                        break;
                    case "MULT":
                        break;
                    case "DIV":
                        break;
                    case "POW":
                        break;
                    case "INV_":
                        break;
                    case "CDP":
                        break;
                    case "CPD":
                        break;
                    case "MOV":
                        break;
                    case "MOV_A":
                        break;
                    case "MOV_ARRAY":
                        break;
                    case "JMP":
                        break;
                    case "LOOP":
                        break;
                    case "LOAD_CA":
                        break;
                    case "LOAD_CA_A":
                        break;
                    case "INC_DEC":
                        break;
                    case "OUT":
                        break;
                }
            }
        }

        public void ADD(string R0, string R1)
        {
            
        }
    }
}
