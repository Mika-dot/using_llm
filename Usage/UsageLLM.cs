using LLama;
using LLama.Common;
using LLM__simple.Model;
using LLM__simple.Word;
using LLM__simple.WorkingFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLM__simple.Usage
{
    public class UsageLLM
    {
        public List<Story> storList = new List<Story>();
        private ChatSession session;
        private InferenceParams inferenceParams;
        private LLamaWeights model;
        private LLamaContext context;
        private InteractiveExecutor executor;

        public UsageLLM(int? N)
        {
            storList = Files.LoadFromFile();

            int n = N ?? storList.Count;

#if DEBUG
            if (n < 0)
            {
                throw new Exception("Длина меньше нуля");
            }
#endif

            Singlet.Dialogue = Conversion.GetLastNStoriesWithAssistantFirst(storList, n);
            Singlet.chatHistory = Conversion.StoryToHistory(Singlet.Dialogue);

#if DEBUG
            if (Singlet.Dialogue.Count <= 0)
            {
                throw new Exception("История из файла после сериализации нету");
            }
#endif

            var parameters = new ModelParams(Singlet.ModelPath)
            {
                ContextSize = 1024,
                GpuLayerCount = 5
            };

            model = LLamaWeights.LoadFromFile(parameters);
            context = model.CreateContext(parameters);
            executor = new InteractiveExecutor(context);
            session = new ChatSession(executor, Singlet.chatHistory);

            inferenceParams = new InferenceParams()
            {
                MaxTokens = 256,
                AntiPrompts = Generate.ProcessStrings(Singlet.Anti),
                Temperature = 0.2f,
            };
        }

        public async Task<string> RunAsync(string userInput)
        {
            try
            {
                string textALL = "";

                storList.Add(new Story(Role.User, userInput));

                await foreach (var text in session.ChatAsync(
                                new ChatHistory.Message(AuthorRole.User, userInput),
                                inferenceParams))
                {
                    Console.Write(text);
                    textALL += text;
                }

                textALL = StopWords.RemoveStopWords(textALL);

                storList.Add(new Story(Role.Assistant, textALL));

                return textALL;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                // Дополнительные действия для восстановления или повторной инициализации объекта
                return "Ошибка выполнения";
            }
        }
        public string Run(string userInput)
        {
            var task = Task.Run(async () => await this.RunAsync(userInput));
            task.Wait(); // Ожидаем завершения задачи

            return task.Result;
        }

        ~UsageLLM()
        {
            // Очистка ресурсов в деструкторе, если необходимо
            model?.Dispose();
            context?.Dispose();
        }
    }

}
