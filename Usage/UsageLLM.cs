using LLama;
using LLama.Common;
using LLamaSharp.KernelMemory;
using LLamaSharp.SemanticKernel;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Diagnostics;
using AuthorRole = Microsoft.SemanticKernel.ChatCompletion.AuthorRole;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;


namespace LLM__simple.Usage
{
    public class UsageLLM
    {
        IKernelMemory memory;
        public UsageLLM()
        {
            // Setup the kernel memory with the LLM model
            memory = CreateMemory(Singlet.ModelPath);

            for (int i = 0; i < Singlet.FilePath.Count; i++)
            {
                string path = Singlet.FilePath[i];
                Stopwatch sw = Stopwatch.StartNew();
                Console.WriteLine($"Importing {i + 1} of {Singlet.FilePath.Count}: {path}");
                var task = Task.Run(async () => (await memory.ImportDocumentAsync(path, steps: Constants.PipelineWithoutSummary)));
                task.Wait();

                var gbnf = task.Result;
                Console.WriteLine($"Completed in {sw.Elapsed}\n");
            }
        }


        public async Task<(string, List<string>)> RunAsync(string userInput)
        {
            try
            {
                return await AnswerQuestion(memory, userInput);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return ("Ошибка выполнения", new List<string>());
            }
        }

        public (string, List<string>) Run(string userInput)
        {
            var task = Task.Run(async () => await this.RunAsync(userInput));
            task.Wait(); // Ожидаем завершения задачи

            return task.Result;
        }

        static IKernelMemory CreateMemory(string modelPath)
        {
            LLama.Common.InferenceParams infParams = new() { AntiPrompts = ["\n\n"] };

            LLamaSharpConfig lsConfig = new(modelPath) { DefaultInferenceParams = infParams };

            SearchClientConfig searchClientConfig = new()
            {
                MaxMatchesCount = 1,
                AnswerTokens = 100,
            };

            TextPartitioningOptions parseOptions = new()
            {
                MaxTokensPerParagraph = 300,
                MaxTokensPerLine = 100,
                OverlappingTokens = 30
            };

            return new KernelMemoryBuilder()
                .WithLLamaSharpDefaults(lsConfig)
                .WithSearchClientConfig(searchClientConfig)
                .With(parseOptions)
                .Build();
        }

        static async Task<(string, List<string>)> AnswerQuestion(IKernelMemory memory, string question)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Generating answer...");

            MemoryAnswer answer = await memory.AskAsync(question);
            Console.WriteLine($"Answer generated in {sw.Elapsed}");

            Console.WriteLine($"Answer: {answer.Result}");

            List<string> links = new List<string>();
            foreach (var source in answer.RelevantSources)
            {
                Console.WriteLine($"Source: {source.SourceName}");
                links.Add(source.SourceName);
            }
            return (answer.Result, links);
        }
    }

    


}
