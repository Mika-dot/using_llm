namespace LLM__simple
{
    public static class Singlet
    {
        public static string ModelPath { get; set; } = @"llm\llava-v1.6-mistral-7b.Q3_K_XS.gguf";
        public static string Grammar { get; set; } = @"llm\json.gbnf";

        // -----------------------------------

        public static List<string> Anti = new List<string> { "User:", "Question:", "#", "Question: ", ".\n" };
        public static List<string> StopWords = new List<string> { "User:", "Assistant:" };
    }
}
