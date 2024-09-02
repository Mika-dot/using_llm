using LLama;
using LLama.Common;
using LLamaSharp.SemanticKernel;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using AuthorRole = Microsoft.SemanticKernel.ChatCompletion.AuthorRole;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;


namespace LLM__simple.Usage
{
    public class UsageLLM
    {

        public UsageLLM()
        {

            var parameters = new ModelParams(Singlet.ModelPath);

            var task_model = Task.Run(async () => await LLamaWeights.LoadFromFileAsync(parameters));
            task_model.Wait();

            using var model = task_model.Result;
            var slex = new StatelessExecutor(model, parameters);

            HostApplicationBuilder builder = Host.CreateApplicationBuilder();

            // Фактический код для выполнения находится в классе Worker.
            builder.Services.AddHostedService<Worker>();

            builder.Services.AddSingleton<IChatCompletionService>(sp =>
            {
                return new LLamaSharpChatCompletion(slex,
                    new LLamaSharpPromptExecutionSettings()
                    {
                        MaxTokens = -1,
                        Temperature = 0,
                        TopP = 0.1,
                    });
            });

            // Добавить плагины, которые могут использоваться ядрами
            builder.Services.AddKeyedSingleton<MyServoPlugin>("ServoController");

            // Добавьте ядро ​​домашней автоматизации в контейнер внедрения зависимостей
            builder.Services.AddKeyedTransient<Kernel>("ServoAutomationKernel", (sp, key) =>
            {
                // Создайте коллекцию плагинов, которые будет использовать ядро.
                KernelPluginCollection pluginCollection = [];
                pluginCollection.AddFromObject(sp.GetRequiredKeyedService<MyServoPlugin>("ServoController"), "ServoController");

                // При создании контейнера внедрения зависимостей журналирование семантического ядра включается по умолчанию.
                return new Kernel(sp, pluginCollection);
            });

            //удалить регистрацию
            builder.Services.AddSingleton<ILoggerFactory>(sp => { return NullLoggerFactory.Instance; });

            using IHost host = builder.Build();

            var task_host = Task.Run(async () => host.RunAsync());
            task_host.Wait();

            var temp = task_host.Result;

        }
    }

    class Worker(
        IHostApplicationLifetime hostApplicationLifetime,
        [FromKeyedServices("ServoAutomationKernel")] Kernel kernel) : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
        private readonly Kernel _kernel = kernel;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Получить услугу завершения чата
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            Console.WriteLine("Задавайте вопросы или давайте указания второму пилоту, например::\n" +
                              "- Set the servo angle to 90 degrees.\n" +
                              "- What is the current servo angle?\n");

            Console.Write("> ");

            LLamaSharpPromptExecutionSettings llamaSharpPromptExecutionSettings = new()
            {
                Temperature = 0.0f,
                TopP = 0.1f
            };

            string? input = null;

            while ((input = Console.ReadLine()) != null)
            {
                ChatHistory chatHistory = new ChatHistory();
                // Подсказка для модели
                chatHistory.Add(new ChatMessageContent(AuthorRole.System,
                    """
                    You are an assistant who determines the user's intent. Here are the possible intents:
                    - If the user wants to set the servo angle, then answer with the following:
                    ```answer
                    [SET SERVO ANGLE]
                    ```
                    - If the user wants to know the current servo angle, then answer with the following:
                    ```answer
                    [GET SERVO ANGLE]
                    ```
                    IMPORTANT: only return the answer without further comments with the format ```answer{intent}```, where {intent} is the user's intent.
                    """
                ));

                Console.WriteLine();

                chatHistory.Add(new ChatMessageContent(AuthorRole.User, input));

                ChatMessageContent chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory, llamaSharpPromptExecutionSettings, _kernel, stoppingToken);

                FunctionResult? fres = null;
                if (chatResult.Content.Contains("[SET SERVO ANGLE]"))
                {
                    // Получаем угол из ввода пользователя
                    int angle = int.Parse(input.Split("to").Last().Trim().Replace("degrees", "").Trim());

                    // Создаем KernelArguments с параметром angle
                    var arguments = new KernelArguments(new Dictionary<string, object>
                    {
                        { "angle", angle }
                    });

                    // Вызываем функцию с использованием созданных аргументов
                    var result = await _kernel.InvokeAsync("ServoController", "SetAngle", arguments, stoppingToken);
                }
                else if (chatResult.Content.Contains("[GET SERVO ANGLE]"))
                {
                    // Вызов функции без параметров
                    var result = await _kernel.InvokeAsync("ServoController", "GetAngle", null, stoppingToken);
                }


                if (fres != null)
                {
                    Console.Write($">>> Result: {fres.GetValue<int>()} degrees\n\n> ");
                }
                else
                {
                    Console.Write($">>> Result: {chatResult}\n\n> ");
                }

            }

            _hostApplicationLifetime.StopApplication();
        }

    }

    /// <summary>
    /// Управление сервоприводом
    /// </summary>
    /// <param name="turnedOn"></param>
    [Description("Represents a servo motor")]
    class MyServoPlugin(int angle = 0)
    {
        private int _angle = angle;

        [KernelFunction, Description("Returns the current angle of the servo")]
        public int GetAngle() => _angle;

        [KernelFunction, Description("Sets the angle of the servo")]
        public void SetAngle(int angle) => _angle = angle;
    }
}
