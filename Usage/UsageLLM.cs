using LLama;
using LLama.Common;
using LLamaSharp.SemanticKernel;
using LLamaSharp.SemanticKernel.ChatCompletion;
using LLM__simple.Usage.Func;
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

    


}
