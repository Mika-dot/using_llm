using LLamaSharp.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLM__simple.Usage
{
    public class Worker(
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
}
