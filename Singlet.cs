using LLama.Common;
using LLM__simple.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLM__simple
{
    public static class Singlet
    {
        public static string ModelPath { get; set; } = @"llm\llama-2-7b-chat.Q3_K_S.gguf";
        public static string Json { get; set; } = @"BD\Histori.json";

        // -----------------------------------

        public static List<Story> Dialogue = new List<Story>();

        public static ChatHistory chatHistory = new ChatHistory();

        // -----------------------------------
        
        public static List<string> Anti = new List<string> { "User:" };
        public static List<string> StopWords = new List<string> { "User:", "Assistant:" };
    }
}
