// Пространство имён, группирующее связанные классы ассемблера
namespace Assembler
{
    // Класс, переводящий ассемблерные инструкции Hack в 16-битный машинный код
    public class HackTranslator
    {
        // Словарь: соответствие мнемоник dest (куда сохранить результат) и 3-битных кодов
        private static readonly Dictionary<string, string> DestTable = CreateDestTable();
        // Словарь: соответствие мнемоник comp (что вычислить) и 6-битных кодов ALU
        private static readonly Dictionary<string, string> CompTable = CreateCompTable();
        // Словарь: соответствие мнемоник jump (условие перехода) и 3-битных кодов
        private static readonly Dictionary<string, string> JumpTable = CreateJumpTable();

        // Инициализация таблицы dest: ключ — мнемоника, значение — 3 бита (A,D,M)
        private static Dictionary<string, string> CreateDestTable()
        {
            return new Dictionary<string, string>
            {
                { "null", "000" }, // Ничего не записывать
                { "M",    "001" }, // Запись в RAM[A] (память)
                { "D",    "010" }, // Запись в регистр D
                { "MD",   "011" }, // Запись в M и D
                { "A",    "100" }, // Запись в регистр A
                { "AM",   "101" }, // Запись в A и M
                { "AD",   "110" }, // Запись в A и D
                { "AMD",  "111" }  // Запись во все: A, M, D
            };
        }

        // Инициализация таблицы comp: соответствие выражений и 6-битных кодов ALU
        private static Dictionary<string, string> CreateCompTable()
        {
            return new Dictionary<string, string>
            {
                // Вычисления с a=0 → используют регистр A
                { "0",   "101010" }, // 0
                { "1",   "111111" }, // 1
                { "-1",  "111010" }, // -1
                { "D",   "001100" }, // D
                { "A",   "110000" }, // A
                { "!D",  "001101" }, // не D
                { "!A",  "110001" }, // не A
                { "-D",  "001111" }, // -D
                { "-A",  "110011" }, // -A
                { "D+1", "011111" }, // D+1
                { "A+1", "110111" }, // A+1
                { "D-1", "001110" }, // D-1
                { "A-1", "110010" }, // A-1
                { "D+A", "000010" }, // D+A
                { "D-A", "010011" }, // D-A
                { "A-D", "000111" }, // A-D
                { "D&A", "000000" }, // D И A
                { "D|A", "010101" }, // D ИЛИ A

                // Те же операции, но с a=1 → используют RAM[A] (M вместо A)
                { "M",   "110000" }, // M
                { "!M",  "110001" }, // не M
                { "-M",  "110011" }, // -M
                { "M+1", "110111" }, // M+1
                { "M-1", "110010" }, // M-1
                { "D+M", "000010" }, // D+M
                { "D-M", "010011" }, // D-M
                { "M-D", "000111" }, // M-D
                { "D&M", "000000" }, // D И M
                { "D|M", "010101" }  // D ИЛИ M
            };
        }

        // Инициализация таблицы jump: соответствие условий и 3-битных кодов
        private static Dictionary<string, string> CreateJumpTable()
        {
            return new Dictionary<string, string>
            {
                { "null", "000" }, // Никогда не перейти
                { "JGT",  "001" }, // Перейти, если результат > 0
                { "JEQ",  "010" }, // Перейти, если результат = 0
                { "JGE",  "011" }, // Перейти, если ≥ 0
                { "JLT",  "100" }, // Перейти, если < 0
                { "JNE",  "101" }, // Перейти, если ≠ 0
                { "JLE",  "110" }, // Перейти, если ≤ 0
                { "JMP",  "111" }  // Всегда перейти
            };
        }

        // Счётчик для автоматического присвоения адресов новым переменным (начиная с 16)
        private static int nextVariableAddress = 16;

        // Преобразует A-инструкцию (@symbol или @число) в 16-битную двоичную строку
        public string AInstructionToCode(string instruction, Dictionary<string, int> symbolTable)
        {
            // Извлекаем часть после '@'
            string symbol = instruction.Substring(1);

            // Если это число — конвертируем напрямую
            if (int.TryParse(symbol, out int address))
            {
                // Переводим в двоичную строку и дополняем до 16 бит нулями слева
                return Convert.ToString(address, 2).PadLeft(16, '0');
            }
            else
            {
                // Если символ ещё не в таблице — выделяем новый адрес (начиная с 16)
                if (!symbolTable.ContainsKey(symbol))
                {
                    symbolTable[symbol] = nextVariableAddress;
                    nextVariableAddress++; // Увеличиваем для следующей переменной
                }
                // Берём адрес из таблицы
                address = symbolTable[symbol];
                // Конвертируем в 16-битную двоичную строку
                return Convert.ToString(address, 2).PadLeft(16, '0');
            }
        }

        // Преобразует C-инструкцию (dest=comp;jump) в 16-битную двоичную строку
        public string CInstructionToCode(string instruction)
        {
            // Разбираем строку на три компонента
            var (dest, comp, jump) = ParseCInstruction(instruction);
            // Кодируем компоненты в биты
            return EncodeCInstruction(dest, comp, jump);
        }

        // Разбирает C-инструкцию на части: dest, comp, jump
        private (string dest, string comp, string jump) ParseCInstruction(string instruction)
        {
            // Значения по умолчанию
            string dest = "null"; // Нет назначения
            string comp = "";     // Обязательная часть — будет заполнена
            string jump = "null"; // Нет перехода

            // Ищем позиции разделителей '=' и ';'
            int eqIndex = instruction.IndexOf('=');
            int semiIndex = instruction.IndexOf(';');

            // Случай 1: dest=comp;jump (полная форма)
            if (eqIndex != -1 && semiIndex != -1)
            {
                dest = instruction.Substring(0, eqIndex);
                comp = instruction.Substring(eqIndex + 1, semiIndex - eqIndex - 1);
                jump = instruction.Substring(semiIndex + 1);
            }
            // Случай 2: dest=comp (без перехода)
            else if (eqIndex != -1)
            {
                dest = instruction.Substring(0, eqIndex);
                comp = instruction.Substring(eqIndex + 1);
            }
            // Случай 3: comp;jump (без назначения)
            else if (semiIndex != -1)
            {
                comp = instruction.Substring(0, semiIndex);
                jump = instruction.Substring(semiIndex + 1);
            }
            // Случай 4: только comp (например, "D")
            else
            {
                comp = instruction;
            }

            // Возвращаем разобранные части
            return (dest, comp, jump);
        }

        // Кодирует части C-инструкции в 16-битную строку
        private string EncodeCInstruction(string dest, string comp, string jump)
        {
            // Бит 'a': 1 если используется M (память), иначе 0 (регистр A)
            string aBit = comp.Contains('M') ? "1" : "0";
            // Получаем 6-битный код операции из таблицы
            string compBits = CompTable[comp];
            // Получаем 3-битный код назначения
            string destBits = DestTable[dest];
            // Получаем 3-битный код условия перехода
            string jumpBits = JumpTable[jump];

            // Формат C-инструкции: "111" + a(1) + comp(6) + dest(3) + jump(3) = 16 бит
            return "111" + aBit + compBits + destBits + jumpBits;
        }

        // Основной метод: переводит весь массив ассемблерных строк в машинный код
        public string[] TranslateAsmToHack(string[] instructions, Dictionary<string, int> symbolTable)
        {
            // Сбрасываем счётчик переменных перед новым проходом
            nextVariableAddress = 16;
            // Массив для результата той же длины
            var result = new string[instructions.Length];
            // Обрабатываем каждую инструкцию
            for (int i = 0; i < instructions.Length; i++)
            {
                // Если начинается с '@' — это A-инструкция
                if (instructions[i].StartsWith('@'))
                {
                    result[i] = AInstructionToCode(instructions[i], symbolTable);
                }
                else
                {
                    // Иначе — C-инструкция
                    result[i] = CInstructionToCode(instructions[i]);
                }
            }
            // Возвращаем массив 16-битных машинных инструкций
            return result;
        }
    }
}
