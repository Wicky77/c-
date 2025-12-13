namespace Assembler
{
    public class SymbolAnalyzer
    {
        public Dictionary<string, int> CreateSymbolsTable(string[] asmLines, out string[] cleanedLines)
        {
            var symbolTable = new Dictionary<string, int> // таблица символов: имя → адрес
            {
                { "R0", 0 }, { "R1", 1 }, { "R2", 2 }, { "R3", 3 }, { "R4", 4 },
                { "R5", 5 }, { "R6", 6 }, { "R7", 7 }, { "R8", 8 }, { "R9", 9 },
                { "R10", 10 }, { "R11", 11 }, { "R12", 12 }, { "R13", 13 }, { "R14", 14 }, { "R15", 15 },
                { "SP", 0 }, { "LCL", 1 }, { "ARG", 2 }, { "THIS", 3 }, { "THAT", 4 },
                { "SCREEN", 0x4000 }, { "KBD", 0x6000 }
            };

            var cleanList = new System.Collections.Generic.List<string>(); // сюда пойдут только команды (без меток)
            int romAddress = 0; // счётчик адресов в памяти команд (ROM)

            foreach (string line in asmLines) // обрабатываем каждую строку
            {
                if (line.StartsWith("(") && line.EndsWith(")")) // если это метка вида "(метка)"
                {
                    string label = line.Substring(1, line.Length - 2); // извлекаем имя без скобок
                    if (!symbolTable.ContainsKey(label)) // если такой метки ещё нет
                        symbolTable[label] = romAddress; // связываем имя с текущим адресом команды
                    // метка не добавляется в cleanList и не увеличивает romAddress
                }
                else
                {
                    cleanList.Add(line); // добавляем настоящую команду
                    romAddress++; // переходим к следующему адресу в ROM
                }
            }

            cleanedLines = cleanList.ToArray(); // возвращаем очищенный код без меток
            return symbolTable; // возвращаем таблицу символов
        }
    }
}
