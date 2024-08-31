using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLM__simple.Word
{
    public static class Generate
    {

        // Метод для обработки списка строк
        public static List<string> ProcessStrings(List<string> inputList)
        {
            foreach (var item in inputList)
            {
                // Извлекаем слово из строки (удаляем двоеточие и пробелы)
                var word = item.TrimEnd(':').Trim();

                // Генерируем все возможные комбинации регистра букв
                var combinations = GenerateCaseCombinations(word);

                // Выводим результат
                PrintCombinations(combinations);
            }
            return inputList;
        }

        // Метод для генерации всех возможных комбинаций регистра букв
        public static List<string> GenerateCaseCombinations(string input)
        {
            var results = new List<string>();
            var charArray = input.ToCharArray();
            int numOfCombinations = 1 << charArray.Length; // 2^n комбинаций

            for (int i = 0; i < numOfCombinations; i++)
            {
                var sb = new StringBuilder();
                for (int j = 0; j < charArray.Length; j++)
                {
                    char c = charArray[j];
                    if ((i & 1 << j) != 0)
                    {
                        sb.Append(char.ToUpper(c));
                    }
                    else
                    {
                        sb.Append(char.ToLower(c));
                    }
                }
                results.Add(sb.ToString());
            }

            return results;
        }

        // Метод для вывода всех комбинаций
        public static void PrintCombinations(List<string> combinations)
        {
            foreach (var combination in combinations)
            {
                Console.WriteLine(combination);
            }
        }
    }
}
