using LLM__simple.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLM__simple.Word
{
    public static class StopWords
    {
        // Метод для удаления стоп-слов
        public static string RemoveStopWords(string input)
        {
            List<string> stopWords = Generate.ProcessStrings(Singlet.StopWords);
            stopWords.Add("\r");
            stopWords.Add("\n");

            foreach (string stopWord in Singlet.StopWords)
            {
                input = input.Replace(stopWord, string.Empty);
            }

            // Удаление лишних пробелов, которые могли остаться после удаления стоп-слов
            return string.Join(" ", input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
