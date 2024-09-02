# using_llm
Основные моменты. Файл модели с расширением
> .gguf
Их можно скачать тут https://huggingface.co/models?library=gguf&sort=trending или с пометкой Text Generation, суть что это должна быть языковая модель.

Файл хранения памяти истории.
> .json

В нутри храниться, история так:
```json
[
  {
    "Role": 0,
    "Text": "You are a translator. From English to Russian. Only translation into Russian without additions."
  },
  {
    "Role": 1,
    "Text": "Hi, my name is Bob."
  },
  {
    "Role": 2,
    "Text": "Привет меня зовут Боб."
  }
]
```

В классе Singlet храиться ModelPath путь до модели и Json история модели.

Прямое использование кода.
Предварительная задача истории
```csharp
Singlet.Dialogue.Add(new Story(Role.System, "You are a translator. From English to Russian. Only translation into Russian without additions."));
Singlet.Dialogue.Add(new Story(Role.User, "Hi, my name is Bob."));
Singlet.Dialogue.Add(new Story(Role.Assistant, "Привет меня зовут Боб."));
```

Сохранение в файл истории
```csharp
Files.SaveToFile(Singlet.Dialogue);
```
Получение данных из файла истории в модельки
```csharp
List<Story> storList = Files.LoadFromFile();
```

Данные в нутрений формат сети и модельки. Цифра 4 это сколько последних диалогов будет загруженно в модель.
```csharp
Singlet.Dialogue = Conversion.GetLastNStoriesWithAssistantFirst(storList, 4);
Singlet.chatHistory = Conversion.StoryToHistory(Singlet.Dialogue);
```
Параметры модели ичкуственного интелекта.
```csharp
var parameters = new ModelParams(Singlet.ModelPath)
{
    ContextSize = 1024,
    GpuLayerCount = 5
};

using var model = LLamaWeights.LoadFromFile(parameters);
using var context = model.CreateContext(parameters);
var executor = new InteractiveExecutor(context);
ChatSession session = new(executor, Singlet.chatHistory);

InferenceParams inferenceParams = new InferenceParams()
{
    MaxTokens = 256,
    AntiPrompts = Generate.ProcessStrings(Singlet.Anti)
};
```
Использование простое, открытое.
```csharp
while (true)
{
    string userInput = Console.ReadLine() ?? "";
    string textS = "";
    storList.Add(new Story(Role.User, userInput));

    await foreach (
        var text
        in session.ChatAsync(
            new ChatHistory.Message(AuthorRole.User, userInput),
            inferenceParams))
    {
        Console.Write(text);
        textS += text;
    }
    storList.Add(new Story(Role.Assistant, StopWords.RemoveStopWords(textS)));

    userInput = Console.ReadLine();
}
```
Пример использование в коде с встроеного метода.
```csharp
UsageLLM lLM = new UsageLLM(null);
string meseg = lLM.Run("Hello World");
string meseg = await lLM.RunAsync("Hello World");
```
