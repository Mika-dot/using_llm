namespace LLM__simple
{
    public static class Singlet
    {
        public static string ModelPath { get; set; } = @"llm\llava-v1.6-mistral-7b.Q3_K_XS.gguf";

        // -----------------------------------

        public static List<string> FilePath = new List<string>() { 
            Path.GetFullPath(@"llm\help.pdf"),
            Path.GetFullPath((@"llm\instruction.pdf"))
                };
    }
}
