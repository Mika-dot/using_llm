# using_llm
Чтение грамматики для LLM
```csharp
var task_Grammar = Task.Run(async () => (await File.ReadAllTextAsync(Singlet.Grammar)).Trim());
task_Grammar.Wait();

var gbnf = task_Grammar.Result;
grammar = Grammar.Parse(gbnf, "root");
```
Стандартная настройка параметра модели к железу
```csharp
var parameters = new ModelParams(Singlet.ModelPath)
{
    Seed = 1337,
    GpuLayerCount = 5
};
```
Инициализация модели
```csharp
var task_model = Task.Run(async () => await LLamaWeights.LoadFromFileAsync(parameters));
task_model.Wait();

model = task_model.Result; // Инициализация модели
ex = new StatelessExecutor(model, parameters);
```

Запуск параметров модели 
```csharp
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
```

Стандартное использование
```csharp
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
```

Использование из под оболочки
```csharp
UsageLLM lLM = new UsageLLM();

var text = await lLM.RunAsync("Tell me the attributes of a good dish");
var text = lLM.Run("Tell me the attributes of a good dish");
```
