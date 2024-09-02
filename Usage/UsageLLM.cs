using LLama;
using LLama.Common;
using LLama.Grammars;
using LLama.Sampling;
using LLM__simple.Word;


namespace LLM__simple.Usage
{
    public class UsageLLM
    {
        private Grammar grammar;
        private DefaultSamplingPipeline samplingPipeline;
        private InferenceParams inferenceParams;
        private StatelessExecutor ex;
        private LLamaWeights model; // Поле для хранения модели

        public UsageLLM()
        {
            var task_Grammar = Task.Run(async () => (await File.ReadAllTextAsync(Singlet.Grammar)).Trim());
            task_Grammar.Wait();

            var gbnf = task_Grammar.Result;
            grammar = Grammar.Parse(gbnf, "root");

            var parameters = new ModelParams(Singlet.ModelPath)
            {
                Seed = 1337,
                GpuLayerCount = 5
            };

            var task_model = Task.Run(async () => await LLamaWeights.LoadFromFileAsync(parameters));
            task_model.Wait();

            model = task_model.Result; // Инициализация модели
            ex = new StatelessExecutor(model, parameters);

            samplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.6f
            };
            inferenceParams = new InferenceParams()
            {
                SamplingPipeline = samplingPipeline,
                AntiPrompts = Generate.ProcessStrings(Singlet.Anti),
                MaxTokens = 300,
            };
        }

        public async Task<string> RunAsync(string userInput)
        {
            try
            {
                using var grammarInstance = grammar.CreateInstance();
                samplingPipeline.Grammar = grammarInstance;

                string textALL = "";
                userInput = $"Question: {userInput?.Trim()} Answer: ";
                await foreach (var text in ex.InferAsync(userInput, inferenceParams))
                {
                    Console.Write(text);
                    textALL += text;
                }

                textALL = StopWords.RemoveStopWords(textALL);

                return textALL;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return "Ошибка выполнения";
            }
        }

        public string Run(string userInput)
        {
            var task = Task.Run(async () => await this.RunAsync(userInput));
            task.Wait(); // Ожидаем завершения задачи

            return task.Result;
        }
    }
}
